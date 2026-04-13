import SwiftUI
import WebKit

// Protocol used to abstract evaluateJavaScript for testing
protocol WebViewEvaluating: AnyObject {
    func evaluateJavaScript(_ javaScriptString: String, completionHandler: ((Any?, Error?) -> Void)?)
}

// Make WKWebView conform to WebViewEvaluating so production code can pass it directly
extension WKWebView: WebViewEvaluating {}

// Protocol to abstract folder picking so we can unit test without NSOpenPanel
protocol FilePicking {
    // Return the selected folder URL or nil if cancelled
    func pickFolder(completion: @escaping (URL?) -> Void)
}

// Default implementation that uses NSOpenPanel
final class NSOpenPanelFilePicker: FilePicking {
    func pickFolder(completion: @escaping (URL?) -> Void) {
        DispatchQueue.main.async {
            let panel = NSOpenPanel()
            panel.canChooseFiles = false
            panel.canChooseDirectories = true
            panel.allowsMultipleSelection = false
            panel.prompt = "Select"

            let response = panel.runModal()
            if response == .OK, let url = panel.url {
                completion(url)
            } else {
                completion(nil)
            }
        }
    }
}

// Controller that performs the pick operation and calls back into the webview via JS
final class FilePickerController {
    private let picker: FilePicking

    init(picker: FilePicking) {
        self.picker = picker
    }

    // Helper that formats the JavaScript call to resolve a Promise by id
    static func jsForResult(requestId: String, url: URL?) -> String {
        let pathJson: String
        let bookmarkJson: String

        if let url = url {
            // Encode path as JSON string literal
            if let data = try? JSONEncoder().encode(url.path), let quoted = String(data: data, encoding: .utf8) {
                pathJson = quoted
            } else {
                pathJson = "null"
            }

            // Try to create a security-scoped bookmark and pass it as a base64 string
            if let bookmarkData = try? url.bookmarkData(options: [.withSecurityScope], includingResourceValuesForKeys: nil, relativeTo: nil) {
                let base64 = bookmarkData.base64EncodedString()
                if let data = try? JSONEncoder().encode(base64), let quoted = String(data: data, encoding: .utf8) {
                    bookmarkJson = quoted
                } else {
                    bookmarkJson = "null"
                }
            } else {
                bookmarkJson = "null"
            }
        } else {
            pathJson = "null"
            bookmarkJson = "null"
        }

        let idJson: String
        if let data = try? JSONEncoder().encode(requestId), let quoted = String(data: data, encoding: .utf8) {
            idJson = quoted
        } else {
            idJson = "null"
        }

        // Simple, deterministic: call _resolveFolderPick(id, path, bookmark) in the bridge
        return "window.__starskyNative._resolveFolderPick(\(idJson),\(pathJson),\(bookmarkJson));"
    }

    // Perform folder pick and notify the web view by evaluating JS
    // Accepts WebViewEvaluating to make unit testing possible
    func performPick(webView: WebViewEvaluating?, requestId: String) {
        picker.pickFolder { url in
            DispatchQueue.main.async {
                guard let webView = webView else { return }
                let js = FilePickerController.jsForResult(requestId: requestId, url: url)
                webView.evaluateJavaScript(js, completionHandler: { _, error in
                    if let error = error {
                        NSLog("[WebView][EvalError] %@", String(describing: error))
                    } else {
                        NSLog("[WebView][FolderPickResult] resolved requestId=\(requestId)")
                    }
                })
            }
        }
    }
}

struct WebView: NSViewRepresentable {
    let url: URL

    func makeCoordinator() -> Coordinator {
        return Coordinator(picker: NSOpenPanelFilePicker())
    }

    func makeNSView(context: Context) -> WKWebView {
        // Create configuration so we can enable developer extras in Debug builds
        let preferences = WKPreferences()
        #if DEBUG
        // Enable the Web Inspector / Developer Extras for debugging
        // `developerExtrasEnabled` is exposed via KVC on macOS WebKit
        preferences.setValue(true, forKey: "developerExtrasEnabled")
        #endif

        let userContentController = WKUserContentController()

        // Register native script message handlers BEFORE injecting any bridge scripts so
        // the injected script can capture the real native handlers (if present) and
        // avoid installing a shim that would block messages.
        userContentController.add(context.coordinator, name: "filePicker")

        #if DEBUG
        userContentController.add(context.coordinator, name: "console")
        #endif

        // Inject a clean id-based Promise bridge at document start.
        // Page calls window.__starskyNative.selectFolder(timeoutMs) and gets a Promise.
        // Native posts { action: 'selectFolder', id }.
        // Native resolves by calling window.__starskyNative._resolveFolderPick(id, path, bookmark).
        let nativeBridgeJS = """
        (function() {
            try {
                window.__starskyNative = window.__starskyNative || {};
                window.__starskyNative._pending = window.__starskyNative._pending || {};

                // Resolve a pending request by id
                window.__starskyNative._resolveFolderPick = function(id, path, bookmark) {
                    try {
                        var resolver = window.__starskyNative._pending[id];
                        if (typeof resolver === 'function') {
                            resolver({ path: path ?? null, bookmark: bookmark ?? null });
                            delete window.__starskyNative._pending[id];
                            return true;
                        }
                    } catch (e) {
                        try { console.warn('[starskyBridge] _resolveFolderPick error', e); } catch (e) {}
                    }
                    return false;
                };

                // Promise-based folder picker
                window.__starskyNative.selectFolder = function(timeoutMs) {
                    timeoutMs = typeof timeoutMs === 'number' ? timeoutMs : 30000;
                    var id = 'fp_' + Date.now() + '_' + Math.random().toString(36).slice(2);

                    return new Promise(function(resolve) {
                        window.__starskyNative._pending[id] = resolve;

                        try {
                            if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.filePicker) {
                                window.webkit.messageHandlers.filePicker.postMessage({ action: 'selectFolder', id: id });
                            } else {
                                delete window.__starskyNative._pending[id];
                                resolve({ path: null, bookmark: null });
                                return;
                            }
                        } catch (e) {
                            delete window.__starskyNative._pending[id];
                            resolve({ path: null, bookmark: null });
                            return;
                        }

                        // Timeout safety
                        setTimeout(function() {
                            if (window.__starskyNative._pending[id]) {
                                var resolver = window.__starskyNative._pending[id];
                                delete window.__starskyNative._pending[id];
                                try { resolver({ path: null, bookmark: null }); } catch (e) {}
                            }
                        }, timeoutMs);
                    });
                };
            } catch (e) {
                try { console.error('[starskyBridge] init error', e); } catch (e) {}
            }
        })();
        """
        let nativeBridgeUserScript = WKUserScript(source: nativeBridgeJS, injectionTime: .atDocumentStart, forMainFrameOnly: false)
        userContentController.addUserScript(nativeBridgeUserScript)

        #if DEBUG
        // Inject a small script to forward console messages from the webpage
        // to the native app via a message handler so they show up in the Xcode console.
        let consoleForwardingJS = """
        (function() {
            function sendConsole(level, args) {
                try {
                    window.webkit.messageHandlers.console.postMessage({ level: level, message: Array.from(args).map(function(a){ try { return a.toString(); } catch(e) { return String(a); } }).join(' ') });
                } catch (e) { }
            }
            ['log','info','warn','error'].forEach(function(level) {
                var orig = console[level];
                console[level] = function() {
                    sendConsole(level, arguments);
                    try { orig.apply(console, arguments); } catch (e) { }
                }
            });
        })();
        """
        let userScript = WKUserScript(source: consoleForwardingJS, injectionTime: .atDocumentStart, forMainFrameOnly: false)
        userContentController.addUserScript(userScript)
        #endif

        let configuration = WKWebViewConfiguration()
        configuration.preferences = preferences
        configuration.userContentController = userContentController

        let webView = WKWebView(frame: .zero, configuration: configuration)
        webView.allowsBackForwardNavigationGestures = true

        // Keep a reference to the webView on the coordinator so it can evaluate JavaScript
        context.coordinator.webView = webView
        // Set navigation delegate so coordinator can run sanity JS checks on load
        webView.navigationDelegate = context.coordinator

        return webView
    }

    func updateNSView(_ nsView: WKWebView, context: Context) {
        nsView.load(URLRequest(url: url))
    }

    // Coordinator to receive console messages from the web content and handle native file picking
    class Coordinator: NSObject, WKScriptMessageHandler, WKNavigationDelegate {
        // Weak reference to the web view for evaluating JavaScript callbacks
        weak var webView: WKWebView?
        private let filePickerController: FilePickerController

        init(picker: FilePicking) {
            self.filePickerController = FilePickerController(picker: picker)
            super.init()
        }

        // Called when a navigation finishes; run a quick JS check to verify bridge presence
        func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
            // Check for bridge and native handler exposure
            let checkJS = "(function(){ try { return JSON.stringify({ hasBridge: !!window.__starskyNative, hasMessageHandler: !!(window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.filePicker) }); } catch(e) { return JSON.stringify({ error: String(e) }); } })();"
            webView.evaluateJavaScript(checkJS) { result, error in
                if let error = error {
                    NSLog("[WebView][BridgeCheck][Error] %@", String(describing: error))
                } else if let s = result as? String {
                    NSLog("[WebView][BridgeCheck] %@", s)
                } else {
                    NSLog("[WebView][BridgeCheck] unknown result: %@", String(describing: result))
                }
            }
        }

        func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
            // Debug log incoming messages so we can see if the page is posting to the handler
            NSLog("[WebView][ScriptMessage] name=\(message.name) body=\(String(describing: message.body))")

            // Handle file picker requests
            if message.name == "filePicker" {
                // Allow a lightweight 'ping' action for diagnostics that does not open the NSOpenPanel
                if let body = message.body as? [String: Any], let action = body["action"] as? String, action == "ping" {
                    NSLog("[WebView][ScriptMessage][ping] received")
                    return
                }
                // Read id from the incoming message body
                if let body = message.body as? [String: Any], let requestId = body["id"] as? String {
                    NSLog("[WebView][ScriptMessage] filePicker request; requestId=\(requestId)")
                    filePickerController.performPick(webView: webView, requestId: requestId)
                } else {
                    NSLog("[WebView][ScriptMessage] filePicker request but no id found")
                }
                return
            }

            // Handle forwarded console messages (Debug only)
            if message.name == "console", let body = message.body as? [String: Any], let level = body["level"] as? String, let text = body["message"] as? String {
                switch level {
                case "error":
                    NSLog("[WebConsole][error] %s", text)
                case "warn":
                    NSLog("[WebConsole][warn] %s", text)
                case "info":
                    NSLog("[WebConsole][info] %s", text)
                default:
                    NSLog("[WebConsole][log] %s", text)
                }
            }
        }
    }
}

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

    // Helper that formats the JavaScript call for a URL (path + bookmark), or nulls
    static func jsForURL(_ url: URL?) -> String {
        if let url = url {
            // Encode path as JSON string literal
            var pathJson: String = "null"
            if let data = try? JSONEncoder().encode(url.path), let quoted = String(data: data, encoding: .utf8) {
                pathJson = quoted
            } else {
                let escaped = url.path.replacingOccurrences(of: "'", with: "\\'")
                pathJson = "'\(escaped)'"
            }

            // Try to create a security-scoped bookmark and pass it as a base64 string
            var bookmarkJson: String = "null"
            if let bookmarkData = try? url.bookmarkData(options: [.withSecurityScope], includingResourceValuesForKeys: nil, relativeTo: nil) {
                let base64 = bookmarkData.base64EncodedString()
                if let data = try? JSONEncoder().encode(base64), let quoted = String(data: data, encoding: .utf8) {
                    bookmarkJson = quoted
                }
            }

            // Use an IIFE that prefers calling the global callback but falls back to dispatching an event
            // Dispatch the event on both window and window.top to reach listeners in other frames.
            return "(function(){try{var path=\(pathJson);var bookmark=\(bookmarkJson);if(typeof window.onFolderSelected==='function'){try{window.onFolderSelected(path,bookmark);}catch(e){}}var ev=null;try{ev=new CustomEvent('starskyFolderSelected',{detail:{path:path,bookmark:bookmark}});}catch(e){try{ev=document.createEvent('CustomEvent');ev.initCustomEvent('starskyFolderSelected',true,true,{path:path,bookmark:bookmark});}catch(e){}}try{if(ev){try{window.dispatchEvent(ev);}catch(e){}}}catch(e){}try{if(window.top&&window.top!==window){try{window.top.dispatchEvent(ev);}catch(e){}}}catch(e){} }catch(e){} })();"
        } else {
            return "(function(){try{if(typeof window.onFolderSelected==='function'){try{window.onFolderSelected(null,null);}catch(e){}}var ev=null;try{ev=new CustomEvent('starskyFolderSelected',{detail:{path:null,bookmark:null}});}catch(e){try{ev=document.createEvent('CustomEvent');ev.initCustomEvent('starskyFolderSelected',true,true,{path:null,bookmark:null});}catch(e){}}try{if(ev){try{window.dispatchEvent(ev);}catch(e){}}}catch(e){}try{if(window.top&&window.top!==window){try{window.top.dispatchEvent(ev);}catch(e){}}}catch(e){} }catch(e){} })();"
        }
    }

    // Perform folder pick and notify the web view by evaluating JS
    // Accepts WebViewEvaluating to make unit testing possible
    func performPick(webView: WebViewEvaluating?) {
        picker.pickFolder { url in
            DispatchQueue.main.async {
                guard let webView = webView else { return }
                let js = FilePickerController.jsForURL(url)
                webView.evaluateJavaScript(js, completionHandler: { _, error in
                    if let error = error {
                        NSLog("[WebView][EvalError] %@", String(describing: error))
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

        // Inject a small native bridge at document start so pages can reliably call
        // a single function to request native actions. This helps when pages check
        // `window.webkit.messageHandlers` too early or replace it.
        let nativeBridgeJS = """
        (function() {
            try {
                if (!window.__starskyNative) {
                    window.__starskyNative = {
                        // selectFolder(timeoutMs?) => Promise<{path,bookmark}>
                        selectFolder: function(timeoutMs) {
                            timeoutMs = typeof timeoutMs === 'number' ? timeoutMs : 30000;
                            return new Promise(function(resolve) {
                                var finished = false;
                                function cleanup() {
                                    finished = true;
                                    try { window.removeEventListener('starskyFolderSelected', onEvent); } catch (e) {}
                                }
                                function onEvent(e) {
                                    if (finished) return;
                                    cleanup();
                                    try { resolve({ path: e.detail.path, bookmark: e.detail.bookmark }); } catch (err) { resolve({ path: null, bookmark: null }); }
                                }
                                window.addEventListener('starskyFolderSelected', onEvent);

                                try {
                                    if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.filePicker) {
                                        window.webkit.messageHandlers.filePicker.postMessage({ action: 'selectFolder' });
                                    } else {
                                        // No native handler available; resolve immediately with nulls
                                        cleanup();
                                        resolve({ path: null, bookmark: null });
                                    }
                                } catch (e) {
                                    cleanup();
                                    resolve({ path: null, bookmark: null });
                                }

                                // Timeout fallback
                                setTimeout(function() { if (finished) return; cleanup(); resolve({ path: null, bookmark: null }); }, timeoutMs);
                            });
                        }
                    };
                }
            } catch (e) { }
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
                filePickerController.performPick(webView: webView)
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

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

            return "window.onFolderSelected(\(pathJson), \(bookmarkJson))"
        } else {
            return "window.onFolderSelected(null, null)"
        }
    }

    // Perform folder pick and notify the web view by evaluating JS
    // Accepts WebViewEvaluating to make unit testing possible
    func performPick(webView: WebViewEvaluating?) {
        picker.pickFolder { url in
            DispatchQueue.main.async {
                guard let webView = webView else { return }
                let js = FilePickerController.jsForURL(url)
                webView.evaluateJavaScript(js, completionHandler: nil)
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

        // Always add the filePicker handler so the webpage can request a folder selection
        userContentController.add(context.coordinator, name: "filePicker")

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
        userContentController.add(context.coordinator, name: "console")
        #endif

        let configuration = WKWebViewConfiguration()
        configuration.preferences = preferences
        configuration.userContentController = userContentController

        let webView = WKWebView(frame: .zero, configuration: configuration)
        webView.allowsBackForwardNavigationGestures = true

        // Keep a reference to the webView on the coordinator so it can evaluate JavaScript
        context.coordinator.webView = webView

        return webView
    }

    func updateNSView(_ nsView: WKWebView, context: Context) {
        nsView.load(URLRequest(url: url))
    }

    // Coordinator to receive console messages from the web content and handle native file picking
    class Coordinator: NSObject, WKScriptMessageHandler {
        // Weak reference to the web view for evaluating JavaScript callbacks
        weak var webView: WKWebView?
        private let filePickerController: FilePickerController

        init(picker: FilePicking) {
            self.filePickerController = FilePickerController(picker: picker)
            super.init()
        }

        func userContentController(_ userContentController: WKUserContentController, didReceive message: WKScriptMessage) {
            // Handle file picker requests
            if message.name == "filePicker" {
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

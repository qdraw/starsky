import WebKit

/// Protocol to make WKWebView testable; allows injection of fakes in tests
protocol WebViewEvaluating {
    func evaluateJavaScript(_ javaScriptString: String, completionHandler: ((Any?, Error?) -> Void)?)
}

extension WKWebView: WebViewEvaluating {}

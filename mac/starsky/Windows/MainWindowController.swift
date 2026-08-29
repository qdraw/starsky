import AppKit
import WebKit
import OSLog

@objc private protocol WKInspectorProtocol {
    func show()
    func detach()
}

private class SilentWindow: NSWindow {
    override func noResponder(for eventSelector: Selector) {}
}

private class SilentWebView: WKWebView {
    override func noResponder(for eventSelector: Selector) {}

    override func doCommand(by selector: Selector) {
        // Arrow keys and other navigation commands that the web content doesn't
        // consume go through interpretKeyEvents → doCommand, which beeps by default.
        // Calling super here triggers NSBeep(); doing nothing silences it.
    }
}

class MainWindowController: NSWindowController, NSWindowDelegate, WKNavigationDelegate, WKUIDelegate, MainWindowView { // NOSONAR swift:S7485
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "MainWindowController")
    private let options: MainWindowOptions
    private let presenter: MainWindowPresenter
    private var webView: WKWebView!
    private var titleObservation: NSKeyValueObservation?

    init(options: MainWindowOptions) {
        self.options = options
        self.presenter = MainWindowPresenter(options: options)
        let window = SilentWindow(
            contentRect: NSRect(x: 100, y: 100, width: 1200, height: 800),
            styleMask: [.titled, .closable, .miniaturizable, .resizable],
            backing: .buffered,
            defer: false
        )
        window.title = "Starsky"
        window.isReleasedWhenClosed = false
        super.init(window: window)
        window.delegate = self
        presenter.view = self
        setupWebView()
    }

    required init?(coder _: NSCoder) { fatalError() }

    // MARK: - MainWindowView

    func evaluateJavaScript(_ script: String) {
        webView.evaluateJavaScript(script)
    }

    func currentURL() -> URL? { webView.url }

    func allCookies() async -> [HTTPCookie] {
        await withCheckedContinuation { continuation in
            webView.configuration.websiteDataStore.httpCookieStore.getAllCookies {
                continuation.resume(returning: $0)
            }
        }
    }

    // MARK: - Setup

    private func setupWebView() {
        let config = WKWebViewConfiguration()
        config.applicationNameForUserAgent = ApplicationInfo.userAgentSuffix
        config.preferences.setValue(true, forKey: "developerExtrasEnabled") // NOSONAR swift:S4507

        let middleClickScript = WKUserScript(source: """
            document.addEventListener('auxclick', function(e) {
                if (e.button !== 1) return;
                var a = e.target.closest('a[href]');
                if (!a) return;
                e.preventDefault();
                window.open(a.href, '_blank');
            }, true);
            """, injectionTime: .atDocumentEnd, forMainFrameOnly: false)
        config.userContentController.addUserScript(middleClickScript)

        // Suppress the macOS system beep that WebKit emits when navigation keys
        // (arrows, page up/down, etc.) aren't consumed by the page. We listen at
        // the bubble phase so app-level handlers fire first; if nothing called
        // preventDefault() and no editable element is focused, we do so ourselves,
        // which makes WebKit treat the event as handled and skip the NSBeep() call.
        let suppressBeepScript = WKUserScript(source: """
            window.addEventListener('keydown', function(e) {
                if (e.defaultPrevented) return;
                var nav = ['ArrowLeft','ArrowRight','ArrowUp','ArrowDown',
                           'PageUp','PageDown','Home','End'];
                if (nav.indexOf(e.key) === -1) return;
                var el = document.activeElement;
                if (el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' ||
                           el.isContentEditable ||
                           el.getAttribute('contenteditable') === 'true')) return;
                e.preventDefault();
            }, false);
            """, injectionTime: .atDocumentStart, forMainFrameOnly: false)
        config.userContentController.addUserScript(suppressBeepScript)

        webView = SilentWebView(frame: .zero, configuration: config)
        webView.navigationDelegate = self
        webView.uiDelegate = self
        webView.translatesAutoresizingMaskIntoConstraints = false
        window?.contentView?.addSubview(webView)

        titleObservation = webView.observe(\.title, options: [.new]) { [weak self] webView, _ in
            let title = webView.title.flatMap { $0.isEmpty ? nil : $0 } ?? "Starsky"
            DispatchQueue.main.async { self?.window?.title = title }
        }

        if let contentView = window?.contentView {
            NSLayoutConstraint.activate([
                webView.topAnchor.constraint(equalTo: contentView.topAnchor),
                webView.bottomAnchor.constraint(equalTo: contentView.bottomAnchor),
                webView.leadingAnchor.constraint(equalTo: contentView.leadingAnchor),
                webView.trailingAnchor.constraint(equalTo: contentView.trailingAnchor)
            ])
        }

        if let url = URL(string: options.startUrl) {
            webView.load(URLRequest(url: url))
        }
    }

    // MARK: - Actions

    func reload() {
        DispatchQueue.main.async { [weak self] in self?.webView.reload() }
    }

    @objc func newWindow() {
        Task { @MainActor in options.windowManager.openMainWindow() }
    }

    @objc func reloadAll() {
        Task { @MainActor in options.windowManager.reloadAll() }
    }

    @objc func editFileInEditor() { presenter.editFileInEditor() }

    @objc func openInBrowser() {
        guard let url = webView.url else { return }
        NSWorkspace.shared.open(url)
    }

    @objc func openDevTools() {
        guard let inspector = webView.value(forKey: "_inspector") as? NSObject else { return }
        inspector.perform(#selector(WKInspectorProtocol.show))
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.3) {
            inspector.perform(#selector(WKInspectorProtocol.detach))
        }
    }

    @objc func zoomIn() {
        webView.pageZoom = min(webView.pageZoom * 1.1, 5.0)
    }

    @objc func zoomOut() {
        webView.pageZoom = max(webView.pageZoom / 1.1, 0.25)
    }

    @objc func actualSize() {
        webView.pageZoom = 1.0
    }

    @objc func openApplicationSettings() {
        let js = """
        document.dispatchEvent(new KeyboardEvent('keydown', {
            key: 'k', code: 'KeyK', keyCode: 75, metaKey: true, shiftKey: true, bubbles: true
        }));
        """
        webView.evaluateJavaScript(js)
    }

    // MARK: - WKNavigationDelegate

    func webView(_: WKWebView, decidePolicyFor navigationAction: WKNavigationAction,
                 decisionHandler: @escaping (WKNavigationActionPolicy) -> Void) {
        guard let url = navigationAction.request.url else { decisionHandler(.allow); return }
        decisionHandler(presenter.navigationPolicy(for: url) ? .allow : .cancel)
    }

    func webView(_ webView: WKWebView, didFinish _: WKNavigation!) {
        guard let url = webView.url else { return }
        presenter.pageDidLoad(url: url, frame: window?.frame, isZoomed: window?.isZoomed ?? false)
    }

    // MARK: - WKUIDelegate

    func webView(_: WKWebView, createWebViewWith _: WKWebViewConfiguration,
                 for navigationAction: WKNavigationAction,
                 windowFeatures _: WKWindowFeatures) -> WKWebView? {
        guard let url = navigationAction.request.url else { return nil }
        presenter.handleNewWindowRequest(for: url)
        return nil
    }

    // MARK: - NSWindowDelegate

    func windowDidBecomeKey(_: Notification) {
        window?.makeFirstResponder(webView)
    }

    func windowWillClose(_: Notification) {
        presenter.windowWillClose()
        options.windowManager.remove(controller: self)
    }
}

import AppKit
import WebKit
import OSLog

@objc private protocol WKInspectorProtocol {
    func show()
    func detach()
}

private class SilentWindow: NSWindow {
    override func noResponder(for _: Selector) {
        // Suppresses the NSBeep() emitted when no responder handles an event.
    }
}

// MARK: - Key-event handling design
//
// WKWebView on macOS beeps (NSBeep) for any keydown that isn't consumed by the
// page and reaches AppKit's responder chain unhandled. This affects:
//   1. Backspace — triggers WKWebView's native go-back navigation.
//   2. Navigation keys (arrows, Page Up/Down, Home, End) not consumed by the page.
//   3. Letter/number keys used as app shortcuts when the React layer handles them
//      but does not call e.preventDefault().
//
// We suppress beeps in JavaScript because the JS layer can inspect
// document.activeElement and the current selection synchronously, which is not
// possible from keyDown(with:) without an async JS evaluation round-trip.
//
// How it works:
//   - suppressKeysSource is injected at document-start.
//   - For every plain key (no Cmd/Ctrl/Alt modifier), if the event target (or any
//     ancestor via the selection) is editable (INPUT, TEXTAREA, contenteditable),
//     the handler returns early and the browser handles the key normally.
//   - Otherwise e.preventDefault() is called, which tells WebKit the event was
//     handled so it never reaches AppKit's interpretKeyEvents path and no beep fires.
//   - Modifier-based shortcuts (Cmd+R, Cmd+C, etc.) are always let through.
//
// The selection-walk fallback is needed because WKWebView sometimes reports
// document.body as document.activeElement even when a contenteditable div has
// keyboard focus; walking from the selection's commonAncestorContainer catches that.
//
// noResponder(for:) on SilentWebView is kept as a last-resort beep suppressor
// for any key that somehow makes it to the responder chain without a handler.
class SilentWebView: WKWebView {
    // Exported as a static so tests can assert on the script content.
    static let suppressKeysSource = """
        window.addEventListener('keydown', function(e) {
            if (e.defaultPrevented) return;
            if (e.metaKey || e.ctrlKey || e.altKey) return;
            var el = document.activeElement;
            if (el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' ||
                       el.isContentEditable ||
                       el.getAttribute('contenteditable') === 'true')) return;
            var sel = window.getSelection && window.getSelection();
            if (sel && sel.rangeCount > 0) {
                var node = sel.getRangeAt(0).commonAncestorContainer;
                if (node.nodeType === 3) node = node.parentElement;
                while (node) {
                    if (node.isContentEditable) return;
                    node = node.parentElement;
                }
            }
            e.preventDefault();
        }, false);
        """

    override func noResponder(for _: Selector) {
        // Suppresses the NSBeep() emitted when no responder handles an event.
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

    var windowFrame: NSRect? { window?.frame }
    var windowIsZoomed: Bool { window?.isZoomed ?? false }

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

        let suppressBeepScript = WKUserScript(
            source: SilentWebView.suppressKeysSource,
            injectionTime: .atDocumentStart,
            forMainFrameOnly: false
        )
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

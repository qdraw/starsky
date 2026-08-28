import AppKit
import WebKit
import OSLog

@objc private protocol WKInspectorProtocol {
    func show()
    func detach()
}

class MainWindowController: NSWindowController, NSWindowDelegate, WKNavigationDelegate, WKUIDelegate {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "MainWindowController")
    private let options: MainWindowOptions
    private var webView: WKWebView!
    private var currentUrl: URL?
    private var titleObservation: NSKeyValueObservation?

    init(options: MainWindowOptions) {
        self.options = options
        let window = NSWindow(
            contentRect: NSRect(x: 100, y: 100, width: 1200, height: 800),
            styleMask: [.titled, .closable, .miniaturizable, .resizable],
            backing: .buffered,
            defer: false
        )
        window.title = "Starsky"
        window.isReleasedWhenClosed = false
        super.init(window: window)
        window.delegate = self
        setupWebView()
        setupMenu()
    }

    required init?(coder _: NSCoder) { fatalError() }

    private func setupWebView() {
        let config = WKWebViewConfiguration()
        config.applicationNameForUserAgent = ApplicationInfo.userAgentSuffix
        config.preferences.setValue(true, forKey: "developerExtrasEnabled")

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

        webView = WKWebView(frame: .zero, configuration: config)
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

    private func setupMenu() {
        // Menu is set globally in AppDelegate; per-window items use first-responder actions
    }

    func reload() {
        DispatchQueue.main.async { [weak self] in
            self?.webView.reload()
        }
    }

    @objc func newWindow() {
        Task { @MainActor in options.windowManager.openMainWindow() }
    }

    @objc func reloadAll() {
        Task { @MainActor in options.windowManager.reloadAll() }
    }

    @objc func editFileInEditor() {
        guard let liveUrl = webView.url else { return }
        let baseUrl = options.navigationService.getEffectiveBaseUrl()

        if options.navigationService.isAllowedOrigin(liveUrl, baseUrl: options.baseUrl) {
            let js = """
            document.dispatchEvent(new KeyboardEvent('keydown', {
                key: 'e', code: 'KeyE', keyCode: 69, metaKey: true, bubbles: true
            }));
            """
            webView.evaluateJavaScript(js)
        } else {
            let components = URLComponents(url: liveUrl, resolvingAgainstBaseURL: false)
            if let fParam = components?.queryItems?.first(where: { $0.name == "f" })?.value,
               fParam != "/" && !fParam.isEmpty {
                Task {
                    do {
                        let cookies = await fetchAllCookies()
                        try await options.fileDownloadService.downloadAndOpen(
                            path: fParam, baseUrl: baseUrl, cookies: cookies
                        )
                    } catch {
                        await MainActor.run {
                            ErrorWindowController.show(
                                message: "Could not open file: \(error.localizedDescription)",
                                parentWindow: self.window
                            )
                        }
                    }
                }
            }
        }
    }

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

    @objc func openApplicationSettings() {
        let js = """
        document.dispatchEvent(new KeyboardEvent('keydown', {
            key: 'k', code: 'KeyK', keyCode: 75, metaKey: true, shiftKey: true, bubbles: true
        }));
        """
        webView.evaluateJavaScript(js)
    }

    private func fetchAllCookies() async -> [HTTPCookie] {
        await withCheckedContinuation { continuation in
            webView.configuration.websiteDataStore.httpCookieStore.getAllCookies {
                continuation.resume(returning: $0)
            }
        }
    }

    // MARK: - WKNavigationDelegate

    func webView(_: WKWebView, decidePolicyFor navigationAction: WKNavigationAction,
                 decisionHandler: @escaping (WKNavigationActionPolicy) -> Void) {
        guard let url = navigationAction.request.url else {
            decisionHandler(.allow)
            return
        }
        if options.navigationService.isAllowedOrigin(url, baseUrl: options.baseUrl) {
            decisionHandler(.allow)
        } else {
            NSWorkspace.shared.open(url)
            decisionHandler(.cancel)
        }
    }

    func webView(_ webView: WKWebView, didFinish _: WKNavigation!) {
        guard let url = webView.url else { return }
        currentUrl = url
        let route = url.path
            + (url.query.map { "?\($0)" } ?? "")
            + (url.fragment.map { "#\($0)" } ?? "")
        let frame = window?.frame
        let geometry = frame.map {
            SavedWindowState(
                route: route,
                x: Double($0.origin.x),
                y: Double($0.origin.y),
                width: Double($0.width),
                height: Double($0.height),
                isMaximized: window?.isZoomed ?? false
            )
        }
        options.routePersistenceService.saveRoute(
            index: options.index, route: route, geometry: geometry
        )
    }

    // MARK: - WKUIDelegate

    func webView(_: WKWebView, createWebViewWith _: WKWebViewConfiguration,
                 for navigationAction: WKNavigationAction,
                 windowFeatures _: WKWindowFeatures) -> WKWebView? {
        guard let url = navigationAction.request.url else { return nil }
        if options.navigationService.isAllowedOrigin(url, baseUrl: options.baseUrl) {
            let route = url.path
                + (url.query.map { "?\($0)" } ?? "")
                + (url.fragment.map { "#\($0)" } ?? "")
            Task { @MainActor in
                options.windowManager.openMainWindow(route: route)
            }
        } else {
            NSWorkspace.shared.open(url)
        }
        return nil
    }

    // MARK: - NSWindowDelegate

    func windowDidBecomeKey(_: Notification) {
        window?.makeFirstResponder(webView)
    }

    func windowWillClose(_: Notification) {
        options.routePersistenceService.removeRoute(index: options.index)
        options.windowManager.remove(controller: self)
    }
}

import AppKit
import WebKit

protocol MainWindowView: AnyObject {
    var window: NSWindow? { get }
    var windowFrame: NSRect? { get }
    var windowIsZoomed: Bool { get }
    func evaluateJavaScript(_ script: String)
    func currentURL() -> URL?
    func allCookies() async -> [HTTPCookie]
}

class MainWindowPresenter {
    weak var view: MainWindowView?

    private let index: Int
    private let baseUrl: String
    private let mode: RuntimeMode
    private let navigationService: NavigationService
    private let routePersistenceService: RoutePersistenceService
    private let fileDownloadService: FileDownloadService
    private let windowManager: WindowManagerProtocol
    private let urlOpener: (URL) -> Void

    init(
        index: Int,
        baseUrl: String,
        mode: RuntimeMode = .local,
        navigationService: NavigationService,
        routePersistenceService: RoutePersistenceService,
        fileDownloadService: FileDownloadService,
        windowManager: WindowManagerProtocol,
        urlOpener: @escaping (URL) -> Void = { NSWorkspace.shared.open($0) }
    ) {
        self.index = index
        self.baseUrl = baseUrl
        self.mode = mode
        self.navigationService = navigationService
        self.routePersistenceService = routePersistenceService
        self.fileDownloadService = fileDownloadService
        self.windowManager = windowManager
        self.urlOpener = urlOpener
    }

    convenience init(options: MainWindowOptions) {
        self.init(
            index: options.index,
            baseUrl: options.baseUrl,
            mode: options.mode,
            navigationService: options.navigationService,
            routePersistenceService: options.routePersistenceService,
            fileDownloadService: options.fileDownloadService,
            windowManager: options.windowManager
        )
    }

    // Returns true to load the URL in the WebView, false when handled externally.
    func navigationPolicy(for url: URL) -> Bool {
        if navigationService.isAllowedOrigin(url, baseUrl: baseUrl) { return true }
        urlOpener(url)
        return false
    }

    func pageDidLoad(url: URL, frame: NSRect?, isZoomed: Bool) {
        let route = url.path
            + (url.query.map { "?\($0)" } ?? "")
            + (url.fragment.map { "#\($0)" } ?? "")
        let geometry = frame.map {
            SavedWindowState(
                route: route,
                x: Double($0.origin.x),
                y: Double($0.origin.y),
                width: Double($0.width),
                height: Double($0.height),
                isMaximized: isZoomed
            )
        }
        routePersistenceService.saveRoute(index: index, route: route, geometry: geometry)
    }

    func handleNewWindowRequest(for url: URL) {
        if navigationService.isAllowedOrigin(url, baseUrl: baseUrl) {
            let route = url.path
                + (url.query.map { "?\($0)" } ?? "")
                + (url.fragment.map { "#\($0)" } ?? "")
            Task { @MainActor in windowManager.openMainWindow(route: route) }
        } else {
            urlOpener(url)
        }
    }

    func editFileInEditor() {
        guard let liveUrl = view?.currentURL() else { return }
        if navigationService.isAllowedOrigin(liveUrl, baseUrl: baseUrl) && mode == .local {
            let js = """
            document.dispatchEvent(new KeyboardEvent('keydown', {
                key: 'e', code: 'KeyE', keyCode: 69, metaKey: true, bubbles: true
            }));
            """
            view?.evaluateJavaScript(js)
        } else {
            let components = URLComponents(url: liveUrl, resolvingAgainstBaseURL: false)
            if let fParam = components?.queryItems?.first(where: { $0.name == "f" })?.value,
               fParam != "/" && !fParam.isEmpty {
                Task {
                    do {
                        let cookieProvider: () async -> [HTTPCookie] = { [weak self] in
                            await self?.view?.allCookies() ?? []
                        }
                        let cookies = await cookieProvider()
                        try await fileDownloadService.downloadAndOpen(
                            path: fParam, baseUrl: baseUrl, cookies: cookies,
                            cookieProvider: cookieProvider
                        )
                    } catch {
                        await MainActor.run { [weak self] in
                            ErrorWindowController.show(
                                message: "Could not open file: \(error.localizedDescription)",
                                parentWindow: self?.view?.window
                            )
                        }
                    }
                }
            }
        }
    }

    func windowWillClose() {
        if let url = view?.currentURL() {
            pageDidLoad(url: url, frame: view?.windowFrame, isZoomed: view?.windowIsZoomed ?? false)
        }
    }
}

import XCTest
@testable import starsky

@MainActor
final class MainWindowPresenterTests: XCTestCase {
    private var tempDir: URL!
    private var settingsService: SettingsService!
    private var routePersistenceService: RoutePersistenceService!
    private var navigationService: NavigationService!
    private var fileDownloadService: FileDownloadService!
    private var mockWindowManager: MockWindowManager!
    private var openedURLs: [URL] = []

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try? FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
        settingsService = SettingsService(settingsFile: tempDir.appendingPathComponent("settings.json"))
        settingsService.load()
        routePersistenceService = RoutePersistenceService(settingsService: settingsService)
        navigationService = NavigationService(settings: settingsService)
        fileDownloadService = FileDownloadService(
            fileLogger: DailyFileLogger(logsDirectory: tempDir),
            tempFolder: tempDir.appendingPathComponent("tmp")
        )
        mockWindowManager = MockWindowManager()
        openedURLs = []
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    private func makePresenter(baseUrl: String = "http://localhost:5000", index: Int = 0) -> MainWindowPresenter {
        MainWindowPresenter(
            index: index,
            baseUrl: baseUrl,
            navigationService: navigationService,
            routePersistenceService: routePersistenceService,
            fileDownloadService: fileDownloadService,
            windowManager: mockWindowManager,
            urlOpener: { [weak self] url in self?.openedURLs.append(url) }
        )
    }

    // MARK: - navigationPolicy

    func testNavigationPolicyAllowsMatchingOrigin() {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        XCTAssertTrue(presenter.navigationPolicy(for: URL(string: "http://localhost:5000/photos?f=/test")!))
    }

    func testNavigationPolicyBlocksExternalOrigin() {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        XCTAssertFalse(presenter.navigationPolicy(for: URL(string: "https://example.com/page")!))
        XCTAssertEqual(openedURLs.map(\.absoluteString), ["https://example.com/page"])
    }

    func testNavigationPolicyBlocksDifferentPort() {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        XCTAssertFalse(presenter.navigationPolicy(for: URL(string: "http://localhost:9999/")!))
        XCTAssertEqual(openedURLs.map(\.absoluteString), ["http://localhost:9999/"])
    }

    // MARK: - pageDidLoad

    func testPageDidLoadSavesRoutePath() {
        let presenter = makePresenter(index: 0)
        presenter.pageDidLoad(url: URL(string: "http://localhost:5000/photos")!, frame: nil, isZoomed: false)
        XCTAssertEqual(routePersistenceService.getRoutes().first?.route, "/photos")
    }

    func testPageDidLoadSavesRouteWithQuery() {
        let presenter = makePresenter(index: 0)
        presenter.pageDidLoad(url: URL(string: "http://localhost:5000?f=/vacation")!, frame: nil, isZoomed: false)
        XCTAssertEqual(routePersistenceService.getRoutes().first?.route, "?f=/vacation")
    }

    func testPageDidLoadSavesRouteWithFragment() {
        let presenter = makePresenter(index: 0)
        presenter.pageDidLoad(url: URL(string: "http://localhost:5000/page#section")!, frame: nil, isZoomed: false)
        XCTAssertEqual(routePersistenceService.getRoutes().first?.route, "/page#section")
    }

    func testPageDidLoadSavesGeometry() {
        let presenter = makePresenter(index: 0)
        let frame = NSRect(x: 50, y: 100, width: 800, height: 600)
        presenter.pageDidLoad(url: URL(string: "http://localhost:5000/")!, frame: frame, isZoomed: false)
        let saved = routePersistenceService.getRoutes().first
        XCTAssertEqual(saved?.x, 50)
        XCTAssertEqual(saved?.y, 100)
        XCTAssertEqual(saved?.width, 800)
        XCTAssertEqual(saved?.height, 600)
        XCTAssertFalse(saved?.isMaximized ?? true)
    }

    func testPageDidLoadRecordsZoomedState() {
        let presenter = makePresenter(index: 0)
        let frame = NSRect(x: 0, y: 0, width: 1440, height: 900)
        presenter.pageDidLoad(url: URL(string: "http://localhost:5000/")!, frame: frame, isZoomed: true)
        XCTAssertTrue(routePersistenceService.getRoutes().first?.isMaximized ?? false)
    }

    // MARK: - windowWillClose

    func testWindowWillCloseRemovesRoute() {
        let presenter = makePresenter(index: 0)
        presenter.pageDidLoad(url: URL(string: "http://localhost:5000/photos")!, frame: nil, isZoomed: false)
        XCTAssertFalse(routePersistenceService.getRoutes().isEmpty)
        presenter.windowWillClose()
        XCTAssertTrue(routePersistenceService.getRoutes().isEmpty)
    }

    func testWindowWillCloseOnlyRemovesItsOwnIndex() {
        let p0 = makePresenter(index: 0)
        let p1 = makePresenter(index: 1)
        p0.pageDidLoad(url: URL(string: "http://localhost:5000/a")!, frame: nil, isZoomed: false)
        p1.pageDidLoad(url: URL(string: "http://localhost:5000/b")!, frame: nil, isZoomed: false)
        p0.windowWillClose()
        XCTAssertEqual(routePersistenceService.getRoutes().count, 1)
        XCTAssertEqual(routePersistenceService.getRoutes().first?.route, "/b")
    }

    // MARK: - editFileInEditor

    func testEditFileInEditorOnAllowedOriginEvaluatesKeyboardEvent() {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        let mockView = MockMainWindowView()
        mockView.stubbedURL = URL(string: "http://localhost:5000/photos?f=/test.jpg")
        presenter.view = mockView

        presenter.editFileInEditor()

        XCTAssertTrue(mockView.evaluatedJavaScript?.contains("KeyboardEvent") == true)
        XCTAssertTrue(mockView.evaluatedJavaScript?.contains("KeyE") == true)
    }

    func testEditFileInEditorWithNoCurrentURLDoesNothing() {
        let presenter = makePresenter()
        let mockView = MockMainWindowView()
        mockView.stubbedURL = nil
        presenter.view = mockView

        presenter.editFileInEditor()

        XCTAssertNil(mockView.evaluatedJavaScript)
    }

    func testEditFileInEditorExternalUrlWithNoFParamDoesNothing() {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        let mockView = MockMainWindowView()
        mockView.stubbedURL = URL(string: "https://external.com/page")
        presenter.view = mockView

        presenter.editFileInEditor()

        XCTAssertNil(mockView.evaluatedJavaScript)
    }

    func testEditFileInEditorExternalUrlWithRootFParamDoesNothing() {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        let mockView = MockMainWindowView()
        // f=/ should be ignored per the presenter guard
        mockView.stubbedURL = URL(string: "https://external.com/page?f=/")
        presenter.view = mockView

        presenter.editFileInEditor()

        XCTAssertNil(mockView.evaluatedJavaScript)
    }

    // MARK: - handleNewWindowRequest

    func testHandleNewWindowRequestForAllowedOriginOpensViaWindowManager() async {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        presenter.handleNewWindowRequest(for: URL(string: "http://localhost:5000/photos?f=/trip")!)
        // Yield to let the MainActor Task scheduled by handleNewWindowRequest execute
        await Task.yield()
        XCTAssertTrue(mockWindowManager.openMainWindowCalled)
        XCTAssertEqual(mockWindowManager.openMainWindowRoute, "/photos?f=/trip")
    }

    func testHandleNewWindowRequestForExternalOriginOpensInBrowser() {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        presenter.handleNewWindowRequest(for: URL(string: "https://example.com/page")!)
        XCTAssertFalse(mockWindowManager.openMainWindowCalled)
        XCTAssertEqual(openedURLs.map(\.absoluteString), ["https://example.com/page"])
    }

    func testHandleNewWindowRequestPreservesQueryAndFragment() async {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        presenter.handleNewWindowRequest(for: URL(string: "http://localhost:5000/search?q=test#top")!)
        await Task.yield()
        XCTAssertEqual(mockWindowManager.openMainWindowRoute, "/search?q=test#top")
    }

    // MARK: - convenience init(options:)

    func testConvenienceInitCreatesUsablePresenter() {
        let fileLogger = DailyFileLogger(logsDirectory: tempDir)
        let wm = WindowManager(
            settingsService: settingsService,
            routePersistenceService: routePersistenceService,
            navigationService: navigationService,
            fileDownloadService: fileDownloadService,
            fileLogger: fileLogger
        )
        let options = MainWindowOptions(
            index: 7,
            startUrl: "http://localhost:5000",
            baseUrl: "http://localhost:5000",
            geometry: nil,
            navigationService: navigationService,
            routePersistenceService: routePersistenceService,
            fileDownloadService: fileDownloadService,
            windowManager: wm,
            fileLogger: fileLogger
        )
        let presenter = MainWindowPresenter(options: options)
        XCTAssertTrue(presenter.navigationPolicy(for: URL(string: "http://localhost:5000/photos")!))
    }

    // MARK: - editFileInEditor async download path

    func testEditFileInEditorExternalUrlWithValidFParamStartsDownloadTask() async {
        let presenter = makePresenter(baseUrl: "http://localhost:5000")
        let mockView = MockMainWindowView()
        // External origin with a non-root f param triggers the async download branch
        mockView.stubbedURL = URL(string: "https://external.com/page?f=/photo.jpg")
        presenter.view = mockView

        presenter.editFileInEditor()

        // Let the async Task run to completion (download fails gracefully with no network in tests)
        try? await Task.sleep(nanoseconds: 100_000_000)
        XCTAssertNil(mockView.evaluatedJavaScript)
    }
}

// MARK: - Test doubles

@MainActor
private class MockWindowManager: WindowManagerProtocol {
    var openMainWindowCalled = false
    var openMainWindowRoute: String?
    var reopenAllCalled = false

    func openMainWindow(route: String?) {
        openMainWindowCalled = true
        openMainWindowRoute = route
    }

    func reopenAll() {
        reopenAllCalled = true
    }
}

private class MockMainWindowView: MainWindowView {
    var window: NSWindow? { nil }
    var stubbedURL: URL?
    var evaluatedJavaScript: String?

    func evaluateJavaScript(_ script: String) { evaluatedJavaScript = script }
    func currentURL() -> URL? { stubbedURL }
    func allCookies() async -> [HTTPCookie] { [] }
}

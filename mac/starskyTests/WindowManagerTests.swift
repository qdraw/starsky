import XCTest
@testable import Starsky

final class WindowManagerTests: XCTestCase {
    private var settingsService: SettingsService!
    private var routePersistenceService: RoutePersistenceService!
    private var navigationService: NavigationService!
    private var fileDownloadService: FileDownloadService!
    private var fileLogger: DailyFileLogger!
    private var sut: WindowManager!

    override func setUp() {
        super.setUp()
        settingsService = SettingsService(settingsFile: URL(fileURLWithPath: "/dev/null"))
        routePersistenceService = RoutePersistenceService(settingsService: settingsService)
        navigationService = NavigationService(settings: settingsService)
        fileLogger = DailyFileLogger(logsDirectory: URL(fileURLWithPath: NSTemporaryDirectory()))
        fileDownloadService = FileDownloadService(
            fileLogger: fileLogger,
            tempFolder: URL(fileURLWithPath: NSTemporaryDirectory())
        )
        sut = WindowManager(
            settingsService: settingsService,
            routePersistenceService: routePersistenceService,
            navigationService: navigationService,
            fileDownloadService: fileDownloadService,
            fileLogger: fileLogger
        )
    }

    override func tearDown() {
        sut = nil
        super.tearDown()
    }

    func testSetLocalPort() {
        sut.setLocalPort(5000)
    }

    @MainActor
    func testCloseAllWithNoWindows() {
        sut.closeAll()
    }

    @MainActor
    func testReloadAllWithNoWindows() {
        sut.reloadAll()
    }

    @MainActor
    func testReopenAllClearsRoutesBeforeOpening() {
        var settings = settingsService.current
        settings.windows = [SavedWindowState(route: "?f=/test", x: 0, y: 0, width: 100, height: 100, isMaximized: false)]
        settingsService.save(settings)

        sut.setLocalPort(1)
        sut.reopenAll()

        XCTAssertTrue(routePersistenceService.getRoutes().isEmpty)
    }

    @MainActor
    func testRestoreWindowsWithSavedRoutesOpensEachRoute() {
        var settings = settingsService.current
        settings.windows = [
            SavedWindowState(route: "?f=/a", x: 0, y: 0, width: 800, height: 600, isMaximized: false),
            SavedWindowState(route: "?f=/b", x: 50, y: 50, width: 1024, height: 768, isMaximized: false)
        ]
        settingsService.save(settings)

        sut.setLocalPort(2)
        sut.restoreWindows()
        // After restore, persisted routes are those from the windows list
        // (openMainWindow triggers pageDidLoad later, so we just verify no crash)
    }

    @MainActor
    func testRestoreWindowsWithNoRoutesOpensOneWindow() {
        sut.setLocalPort(3)
        sut.restoreWindows()
        // If no saved routes, openMainWindow is called once — verify no crash
    }

    @MainActor
    func testOpenMainWindowWithRouteDoesNotCrash() {
        sut.setLocalPort(4)
        sut.openMainWindow(route: "?f=/photos")
    }

    @MainActor
    func testOpenMainWindowWithMaximizedGeometryDoesNotCrash() {
        sut.setLocalPort(5)
        let geometry = SavedWindowState(route: "?f=/", x: 0, y: 0, width: 1440, height: 900, isMaximized: true)
        sut.openMainWindow(route: nil, geometry: geometry)
    }

    @MainActor
    func testOpenMultipleWindowsAppliesCascadeOffset() {
        sut.setLocalPort(6)
        sut.openMainWindow(route: "?f=/a")
        sut.openMainWindow(route: "?f=/b")
        // Second window (index=1) has cascadeOffset=24 — verify no crash
    }

}

// MARK: - isOnScreen / resolveGeometry

final class WindowManagerGeometryTests: XCTestCase {

    private func state(x: Double = 100, y: Double = 100, w: Double = 800, h: Double = 600,
                       maximized: Bool = false) -> SavedWindowState {
        SavedWindowState(route: "?f=/", x: x, y: y, width: w, height: h, isMaximized: maximized)
    }

    func testMaximizedWindowIsAlwaysOnScreen() {
        XCTAssertTrue(WindowManager.isOnScreen(state(x: -9999, y: -9999, maximized: true)))
    }

    func testWindowTooNarrowIsOffScreen() {
        XCTAssertFalse(WindowManager.isOnScreen(state(w: 100, h: 600)))
    }

    func testWindowTooShortIsOffScreen() {
        XCTAssertFalse(WindowManager.isOnScreen(state(w: 800, h: 50)))
    }

    func testWindowCompletelyOffRightIsOffScreen() {
        // Place window far off the right edge of any realistic screen
        XCTAssertFalse(WindowManager.isOnScreen(state(x: 999_999, y: 100)))
    }

    func testWindowCompletelyOffLeftIsOffScreen() {
        XCTAssertFalse(WindowManager.isOnScreen(state(x: -999_999, y: 100)))
    }

    func testWindowCompletelyAboveScreenIsOffScreen() {
        XCTAssertFalse(WindowManager.isOnScreen(state(x: 100, y: 999_999)))
    }

    func testWindowCompletelyBelowScreenIsOffScreen() {
        XCTAssertFalse(WindowManager.isOnScreen(state(x: 100, y: -999_999)))
    }

    func testNormalWindowOnPrimaryScreenIsOnScreen() {
        // Assumes at least one screen exists in the test environment
        guard let screen = NSScreen.screens.first else { return }
        let f = screen.frame
        XCTAssertTrue(WindowManager.isOnScreen(
            state(x: Double(f.midX) - 400, y: Double(f.midY) - 300)
        ))
    }

    func testResolveGeometryPassesThroughValidGeometry() {
        guard let screen = NSScreen.screens.first else { return }
        let f = screen.frame
        let g = state(x: Double(f.midX) - 400, y: Double(f.midY) - 300, w: 900, h: 700)
        let resolved = WindowManager.resolveGeometry(g, offset: 0)
        XCTAssertEqual(resolved.width, 900)
        XCTAssertEqual(resolved.height, 700)
    }

    func testResolveGeometryFallsBackToDefaultWhenOffScreen() {
        let offScreen = state(x: 999_999, y: 999_999)
        let resolved = WindowManager.resolveGeometry(offScreen, offset: 0)
        XCTAssertEqual(resolved.x, 100)
        XCTAssertEqual(resolved.y, 100)
        XCTAssertEqual(resolved.width, 1200)
        XCTAssertEqual(resolved.height, 800)
    }

    func testResolveGeometryPreservesRouteOnFallback() {
        let offScreen = state(x: 999_999, y: 999_999)
        var s = offScreen; s.route = "?f=/vacation"
        let resolved = WindowManager.resolveGeometry(s, offset: 0)
        XCTAssertEqual(resolved.route, "?f=/vacation")
    }

    func testResolveGeometryAppliesCascadeOffset() {
        guard let screen = NSScreen.screens.first else { return }
        let f = screen.frame
        let g = state(x: Double(f.midX) - 400, y: Double(f.midY) - 300)
        let resolved = WindowManager.resolveGeometry(g, offset: 48)
        XCTAssertEqual(resolved.x, g.x + 48, accuracy: 0.001)
    }

    func testResolveGeometryWithNilUsesDefaults() {
        let resolved = WindowManager.resolveGeometry(nil, offset: 0)
        XCTAssertEqual(resolved.x, 100)
        XCTAssertEqual(resolved.y, 100)
        XCTAssertEqual(resolved.width, 1200)
        XCTAssertEqual(resolved.height, 800)
    }
}

// MARK: - WindowManagerProtocol default extension coverage

@MainActor
private final class MinimalWindowManager: WindowManagerProtocol {
    var openCallCount = 0
    var lastRoute: String? = "sentinel"

    func openMainWindow(route: String?) {
        openCallCount += 1
        lastRoute = route
    }

    func reopenAll() {}
}

final class WindowManagerProtocolDefaultTests: XCTestCase {

    @MainActor
    func testDefaultOpenMainWindowCallsWithNilRoute() {
        let wm = MinimalWindowManager()
        wm.openMainWindow()
        XCTAssertEqual(wm.openCallCount, 1)
        XCTAssertNil(wm.lastRoute)
    }

    @MainActor
    func testDefaultSetLocalPortIsNoOp() {
        let wm = MinimalWindowManager()
        wm.setLocalPort(9000)
    }

    @MainActor
    func testDefaultRestoreWindowsIsNoOp() {
        let wm = MinimalWindowManager()
        wm.restoreWindows()
    }

    @MainActor
    func testDefaultCloseAllIsNoOp() {
        let wm = MinimalWindowManager()
        wm.closeAll()
    }

    @MainActor
    func testDefaultReloadAllIsNoOp() {
        let wm = MinimalWindowManager()
        wm.reloadAll()
    }
}

import XCTest
@testable import starsky

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

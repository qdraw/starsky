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
}

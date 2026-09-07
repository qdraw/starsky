import XCTest
import AppKit


/// AppDelegate is a thin NSApplicationDelegate adapter with no business logic.
/// Tests here only verify the delegate stubs — all business logic lives in AppCoreTests.
final class AppDelegateTests: XCTestCase {
    private var delegate: AppDelegate!

    override func setUp() {
        super.setUp()
        delegate = AppDelegate()
    }

    override func tearDown() {
        delegate = nil
        super.tearDown()
    }

    // MARK: - buildDockMenu helpers

    @MainActor
    private func makeWindowManager() -> WindowManager {
        let settingsService = SettingsService(settingsFile: URL(fileURLWithPath: "/dev/null"))
        let routePersistenceService = RoutePersistenceService(settingsService: settingsService)
        let navigationService = NavigationService(settings: settingsService)
        let fileLogger = DailyFileLogger(logsDirectory: URL(fileURLWithPath: NSTemporaryDirectory()))
        let fileDownloadService = FileDownloadService(
            fileLogger: fileLogger,
            tempFolder: URL(fileURLWithPath: NSTemporaryDirectory())
        )
        return WindowManager(
            settingsService: settingsService,
            routePersistenceService: routePersistenceService,
            navigationService: navigationService,
            fileDownloadService: fileDownloadService,
            fileLogger: fileLogger
        )
    }

    func testSupportsSecureRestorableStateReturnsTrue() {
        XCTAssertTrue(delegate.applicationSupportsSecureRestorableState(NSApplication.shared))
    }

    func testShouldTerminateAfterLastWindowClosedReturnsFalse() {
        XCTAssertFalse(delegate.applicationShouldTerminateAfterLastWindowClosed(NSApplication.shared))
    }

    func testShouldHandleReopenWithVisibleWindowsReturnsTrue() {
        XCTAssertTrue(delegate.applicationShouldHandleReopen(NSApplication.shared, hasVisibleWindows: true))
    }

    func testShouldHandleReopenWithoutVisibleWindowsReturnsTrue() {
        // core is nil (startup skipped in tests); optional chaining prevents a crash
        XCTAssertTrue(delegate.applicationShouldHandleReopen(NSApplication.shared, hasVisibleWindows: false))
    }

    func testApplicationWillTerminateDoesNotCrash() {
        delegate.applicationWillTerminate(Notification(name: NSApplication.willTerminateNotification))
    }

    func testApplicationShouldTerminateReturnsTerminateLater() {
        // core is nil; no-op
        let reply = delegate.applicationShouldTerminate(NSApplication.shared)
        XCTAssertEqual(reply, .terminateLater)
    }

    // MARK: - buildDockMenu

    func testBuildDockMenuWithNoWindowsHasOnlyNewWindowItem() {
        let menu = delegate.buildDockMenu(windows: [], keyWindow: nil)
        XCTAssertEqual(menu.items.count, 1)
        XCTAssertFalse(menu.items[0].isSeparatorItem)
    }

    @MainActor
    func testBuildDockMenuWithWindowsIncludesSeparatorAndWindowItems() {
        let manager = makeWindowManager()
        manager.setLocalPort(1)
        manager.openMainWindow()
        manager.openMainWindow()
        let windows = manager.allWindows()

        let menu = delegate.buildDockMenu(windows: windows, keyWindow: nil)
        // [newWindow, separator, w0, w1]
        XCTAssertEqual(menu.items.count, 4)
        XCTAssertTrue(menu.items[1].isSeparatorItem)
    }

    @MainActor
    func testBuildDockMenuActiveWindowHasCheckmark() {
        let manager = makeWindowManager()
        manager.setLocalPort(1)
        manager.openMainWindow()
        manager.openMainWindow()
        let windows = manager.allWindows()

        let menu = delegate.buildDockMenu(windows: windows, keyWindow: windows[0].window)
        XCTAssertEqual(menu.items[2].state, .on)
        XCTAssertEqual(menu.items[3].state, .off)
    }

    @MainActor
    func testBuildDockMenuNoKeyWindowHasNoCheckmarks() {
        let manager = makeWindowManager()
        manager.setLocalPort(1)
        manager.openMainWindow()
        manager.openMainWindow()
        let windows = manager.allWindows()

        let menu = delegate.buildDockMenu(windows: windows, keyWindow: nil)
        XCTAssertEqual(menu.items[2].state, .off)
        XCTAssertEqual(menu.items[3].state, .off)
    }

    @MainActor
    func testBuildDockMenuWindowItemsHaveControllerAsRepresentedObject() {
        let manager = makeWindowManager()
        manager.setLocalPort(1)
        manager.openMainWindow()
        let windows = manager.allWindows()

        let menu = delegate.buildDockMenu(windows: windows, keyWindow: nil)
        XCTAssertTrue(menu.items[2].representedObject is MainWindowController)
    }

    func testApplicationDockMenuWithNilCoreReturnsMenuWithSingleItem() {
        // core is nil when startup is skipped in tests
        let menu = delegate.applicationDockMenu(NSApplication.shared)
        XCTAssertNotNil(menu)
        XCTAssertEqual(menu?.items.count, 1)
    }
}

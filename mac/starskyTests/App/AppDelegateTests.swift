import XCTest
import AppKit
@testable import Starsky

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
}

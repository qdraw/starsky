import XCTest
import AppKit
@testable import starsky

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

    // MARK: - Simple delegate methods (services are nil because applicationDidFinishLaunching
    // early-returns in XCTest; all usages are nil-safe via optional chaining)

    func testSupportsSecureRestorableStateReturnsTrue() {
        XCTAssertTrue(delegate.applicationSupportsSecureRestorableState(NSApplication.shared))
    }

    func testShouldTerminateAfterLastWindowClosedReturnsFalse() {
        XCTAssertFalse(delegate.applicationShouldTerminateAfterLastWindowClosed(NSApplication.shared))
    }

    func testShouldHandleReopenWithVisibleWindowsReturnsTrue() {
        let result = delegate.applicationShouldHandleReopen(NSApplication.shared, hasVisibleWindows: true)
        XCTAssertTrue(result)
    }

    func testShouldHandleReopenWithoutVisibleWindowsReturnsTrue() {
        // windowManager is nil (startup skipped in tests) — the Task uses optional chaining so no crash
        let result = delegate.applicationShouldHandleReopen(NSApplication.shared, hasVisibleWindows: false)
        XCTAssertTrue(result)
    }

    func testApplicationWillTerminateDoesNotCrash() {
        delegate.applicationWillTerminate(Notification(name: NSApplication.willTerminateNotification))
    }
}

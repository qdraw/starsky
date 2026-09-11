import XCTest
import AppKit

@MainActor
final class UpdateWindowControllerTests: XCTestCase {
    private var tempDir: URL!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try? FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    private func makeUpdateService() -> UpdateService {
        let svc = SettingsService(settingsFile: tempDir.appendingPathComponent("settings.json"))
        svc.load()
        return UpdateService(settingsService: svc)
    }

    private func buttons(in controller: UpdateWindowController) -> [NSButton] {
        controller.window?.contentView?.subviews.compactMap { $0 as? NSButton } ?? []
    }

    // MARK: - Button target retention

    func testButtonTargetsAreNonNilWhileControllerIsAlive() {
        let controller = UpdateWindowController(updateService: makeUpdateService())
        controller.showWindow(nil)

        let btns = buttons(in: controller)
        XCTAssertFalse(btns.isEmpty, "Expected buttons in the update window")
        for btn in btns {
            XCTAssertNotNil(btn.target,
                "Button '\(btn.title)' has a nil target — the controller must be retained by the caller")
        }
    }

    func testButtonTargetBecomesNilWhenControllerIsReleased() {
        // This test documents the bug that existed before AppCore stored the controller:
        // creating UpdateWindowController as a local variable and not retaining it caused
        // NSButton.target (a weak reference) to become nil on deallocation, so button taps
        // did nothing. AppCore now stores the controller in updateWindowController.
        weak var weakController: UpdateWindowController?
        var capturedButtons: [NSButton] = []

        autoreleasepool {
            let controller = UpdateWindowController(updateService: makeUpdateService())
            controller.showWindow(nil)
            weakController = controller
            capturedButtons = buttons(in: controller)
            // controller goes out of scope here — not stored anywhere
        }

        XCTAssertNil(weakController, "Controller should be deallocated when no strong reference is held")
        for btn in capturedButtons {
            XCTAssertNil(btn.target,
                "Button '\(btn.title)' target should be nil after controller deallocation (weak reference)")
        }
    }

    // MARK: - Window setup

    func testWindowIsNotReleasedWhenClosed() {
        let controller = UpdateWindowController(updateService: makeUpdateService())
        controller.showWindow(nil)
        controller.window?.close()
        XCTAssertNotNil(controller.window, "Window should survive close (isReleasedWhenClosed = false)")
    }
}

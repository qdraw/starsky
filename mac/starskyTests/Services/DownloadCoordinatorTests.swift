import XCTest
import AppKit

@MainActor
final class DownloadCoordinatorTests: XCTestCase {

    // MARK: - savePanelPresenter is called with the suggested filename

    func testSavePanelPresenterReceivesSuggestedFilename() {
        let coordinator = DownloadCoordinator(window: nil)
        var capturedFilename: String?
        coordinator.savePanelPresenter = { filename, _, completion in
            capturedFilename = filename
            completion(nil)
        }

        coordinator.savePanelPresenter("export.zip", nil) { _ in }

        XCTAssertEqual(capturedFilename, "export.zip")
    }

    // MARK: - savePanelPresenter is called with the coordinator's window

    func testSavePanelPresenterReceivesWindow() {
        let win = NSWindow(
            contentRect: NSRect(x: 0, y: 0, width: 100, height: 100),
            styleMask: .borderless,
            backing: .buffered,
            defer: true
        )
        let coordinator = DownloadCoordinator(window: win)
        var capturedWindow: NSWindow?
        coordinator.savePanelPresenter = { _, window, completion in
            capturedWindow = window
            completion(nil)
        }

        coordinator.savePanelPresenter("test.zip", win) { _ in }

        XCTAssertTrue(capturedWindow === win)
    }

    // MARK: - completion is forwarded from the panel presenter

    func testSavePanelPresenterCompletionReceivesChosenURL() {
        let coordinator = DownloadCoordinator(window: nil)
        let chosen = URL(fileURLWithPath: "/tmp/chosen.zip")
        var result: URL? = nil
        coordinator.savePanelPresenter = { _, _, completion in completion(chosen) }

        let expectation = expectation(description: "completion called")
        coordinator.savePanelPresenter("file.zip", nil) { url in
            result = url
            expectation.fulfill()
        }
        wait(for: [expectation], timeout: 1)

        XCTAssertEqual(result, chosen)
    }

    func testSavePanelPresenterCompletionReceivesNilOnCancel() {
        let coordinator = DownloadCoordinator(window: nil)
        var result: URL? = URL(fileURLWithPath: "/sentinel")
        coordinator.savePanelPresenter = { _, _, completion in completion(nil) }

        let expectation = expectation(description: "completion called")
        coordinator.savePanelPresenter("file.zip", nil) { url in
            result = url
            expectation.fulfill()
        }
        wait(for: [expectation], timeout: 1)

        XCTAssertNil(result)
    }

    // MARK: - Default savePanelPresenter: nil window → completion(nil)

    func testDefaultSavePanelPresenterCallsNilWhenNoWindow() {
        let coordinator = DownloadCoordinator(window: nil)
        var result: URL? = URL(fileURLWithPath: "/sentinel")

        let expectation = expectation(description: "completion called")
        coordinator.savePanelPresenter("archive.zip", nil) { url in
            result = url
            expectation.fulfill()
        }
        wait(for: [expectation], timeout: 1)

        XCTAssertNil(result)
    }

    // MARK: - Window weak reference

    func testCoordinatorDoesNotRetainWindow() {
        weak var weakWin: NSWindow?
        autoreleasepool {
            let win = NSWindow(
                contentRect: NSRect(x: 0, y: 0, width: 100, height: 100),
                styleMask: .borderless,
                backing: .buffered,
                defer: true
            )
            weakWin = win
            let coordinator = DownloadCoordinator(window: win)
            _ = coordinator
        }
        XCTAssertNil(weakWin, "DownloadCoordinator must not retain the window strongly")
    }
}

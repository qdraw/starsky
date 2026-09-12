import XCTest
import AppKit
import WebKit

@MainActor
final class DownloadCoordinatorTests: XCTestCase {

    // MARK: - decideDownloadDestination: nil window → completion(nil)

    func testDecideDestinationCallsNilCompletionWhenWindowIsNil() {
        let coordinator = DownloadCoordinator(window: nil)
        var result: URL? = URL(fileURLWithPath: "/sentinel")

        let expectation = expectation(description: "completion called")
        coordinator.decideDownloadDestination(suggestedFilename: "export.zip") { url in
            result = url
            expectation.fulfill()
        }
        wait(for: [expectation], timeout: 1)

        XCTAssertNil(result)
    }

    // MARK: - decideDownloadDestination: configures panel before presenting

    func testDecideDestinationSetsSuggestedFilenameOnPanel() {
        let panel = SpyPanel()
        let coordinator = makeCoordinator(panel: panel)

        coordinator.decideDownloadDestination(suggestedFilename: "archive.zip") { _ in }

        XCTAssertEqual(panel.nameFieldStringValue, "archive.zip")
    }

    func testDecideDestinationEnablesDirectoryCreationOnPanel() {
        let panel = SpyPanel()
        let coordinator = makeCoordinator(panel: panel)

        coordinator.decideDownloadDestination(suggestedFilename: "archive.zip") { _ in }

        XCTAssertTrue(panel.canCreateDirectories)
    }

    // MARK: - decideDownloadDestination: forwards panel result

    func testDecideDestinationForwardsChosenURLOnOK() {
        let chosen = URL(fileURLWithPath: "/tmp/out.zip")
        let panel = SpyPanel(response: .OK, url: chosen)
        let coordinator = makeCoordinator(panel: panel)
        var result: URL?

        let expectation = expectation(description: "completion called")
        coordinator.decideDownloadDestination(suggestedFilename: "file.zip") { url in
            result = url
            expectation.fulfill()
        }
        wait(for: [expectation], timeout: 1)

        XCTAssertEqual(result, chosen)
    }

    func testDecideDestinationForwardsNilOnCancel() {
        let panel = SpyPanel(response: .cancel, url: nil)
        let coordinator = makeCoordinator(panel: panel)
        var result: URL? = URL(fileURLWithPath: "/sentinel")

        let expectation = expectation(description: "completion called")
        coordinator.decideDownloadDestination(suggestedFilename: "file.zip") { url in
            result = url
            expectation.fulfill()
        }
        wait(for: [expectation], timeout: 1)

        XCTAssertNil(result)
    }

    func testDecideDestinationForwardsNilWhenPanelURLIsNilOnOK() {
        let panel = SpyPanel(response: .OK, url: nil)
        let coordinator = makeCoordinator(panel: panel)
        var result: URL? = URL(fileURLWithPath: "/sentinel")

        let expectation = expectation(description: "completion called")
        coordinator.decideDownloadDestination(suggestedFilename: "file.zip") { url in
            result = url
            expectation.fulfill()
        }
        wait(for: [expectation], timeout: 1)

        XCTAssertNil(result)
    }

    // MARK: - handleDownloadFailure: does not crash

    func testHandleDownloadFailureDoesNotThrow() {
        let coordinator = DownloadCoordinator(window: nil)
        let error = NSError(domain: "test", code: 1, userInfo: [NSLocalizedDescriptionKey: "network error"])
        coordinator.handleDownloadFailure(error: error)
        // No assertion needed — confirms it reaches completion without crashing
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

    // MARK: - WKDownloadDelegate forwarding

    func testDelegateDecideDestinationForwardsSuggestedFilename() {
        let coordinator = SpyCoordinator(window: nil)

        coordinator.download(makeDownloadStub(),
                             decideDestinationUsing: URLResponse(),
                             suggestedFilename: "photo.jpg") { _ in }

        XCTAssertEqual(coordinator.decideCalls, ["photo.jpg"])
    }

    func testDelegateDecideDestinationForwardsCompletionHandler() {
        let coordinator = SpyCoordinator(window: nil)
        let chosen = URL(fileURLWithPath: "/tmp/photo.jpg")
        coordinator.decideResult = chosen
        var result: URL?

        let expectation = expectation(description: "completion called")
        coordinator.download(makeDownloadStub(),
                             decideDestinationUsing: URLResponse(),
                             suggestedFilename: "photo.jpg") { url in
            result = url
            expectation.fulfill()
        }
        wait(for: [expectation], timeout: 1)

        XCTAssertEqual(result, chosen)
    }

    func testDelegateDidFailWithErrorForwardsError() {
        let coordinator = SpyCoordinator(window: nil)
        let error = NSError(domain: "test", code: 42)

        coordinator.download(makeDownloadStub(), didFailWithError: error, resumeData: nil)

        XCTAssertEqual(coordinator.failures.count, 1)
        XCTAssertEqual((coordinator.failures.first as? NSError)?.code, 42)
    }

    func testDelegateDidFailWithErrorForwardsResumeDataIndependently() {
        let coordinator = SpyCoordinator(window: nil)
        let error = NSError(domain: "test", code: 7)

        coordinator.download(makeDownloadStub(),
                             didFailWithError: error,
                             resumeData: Data([0x01, 0x02]))

        XCTAssertEqual((coordinator.failures.first as? NSError)?.code, 7)
    }

    // MARK: - Helpers

    /// WKDownload has no public initializer and the delegate methods ignore the
    /// argument, so a placeholder object is reinterpreted for the call.
    private func makeDownloadStub() -> WKDownload {
        downloadPlaceholder = NSObject()
        return unsafeBitCast(downloadPlaceholder, to: WKDownload.self)
    }

    private var downloadPlaceholder = NSObject()

    private func makeCoordinator(panel: SpyPanel) -> DownloadCoordinator {
        let win = NSWindow(
            contentRect: NSRect(x: 0, y: 0, width: 200, height: 200),
            styleMask: .borderless,
            backing: .buffered,
            defer: true
        )
        let coordinator = DownloadCoordinator(window: win)
        coordinator.panelFactory = { panel }
        return coordinator
    }
}

// MARK: - SpyCoordinator

/// Records calls to the overridable internal methods so the thin
/// WKDownloadDelegate forwarding can be verified without a real download.
@MainActor
private final class SpyCoordinator: DownloadCoordinator {
    var decideCalls: [String] = []
    var decideResult: URL?
    var failures: [Error] = []

    override func decideDownloadDestination(suggestedFilename: String,
                                            completionHandler: @escaping @MainActor (URL?) -> Void) {
        decideCalls.append(suggestedFilename)
        completionHandler(decideResult)
    }

    override func handleDownloadFailure(error: Error) {
        failures.append(error)
    }
}

// MARK: - SpyPanel

/// NSSavePanel subclass that immediately calls its completion handler
/// without showing any UI, enabling synchronous unit-test assertions.
private class SpyPanel: NSSavePanel {
    private let stubbedResponse: NSApplication.ModalResponse
    private let stubbedURL: URL?

    init(response: NSApplication.ModalResponse = .OK, url: URL? = nil) {
        self.stubbedResponse = response
        self.stubbedURL = url
        super.init(contentRect: .zero, styleMask: .borderless, backing: .buffered, defer: true)
    }

    required init?(coder: NSCoder) { fatalError() }

    override var url: URL? { stubbedURL }

    override func beginSheetModal(for sheetWindow: NSWindow,
                                  completionHandler handler: @escaping (NSApplication.ModalResponse) -> Void) {
        handler(stubbedResponse)
    }
}

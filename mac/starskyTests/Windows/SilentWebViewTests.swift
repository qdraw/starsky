import XCTest
import WebKit

final class SilentWebViewTests: XCTestCase {

    // MARK: - suppressNavigationKeysSource

    func testSuppressScriptIncludesBackspace() {
        XCTAssertTrue(
            SilentWebView.suppressNavigationKeysSource.contains("'Backspace'"),
            "Script must include Backspace so WKWebView doesn't trigger go-back navigation"
        )
    }

    func testSuppressScriptCallsPreventDefault() {
        XCTAssertTrue(SilentWebView.suppressNavigationKeysSource.contains("e.preventDefault()"))
    }

    func testSuppressScriptGuardsEditableElements() {
        let src = SilentWebView.suppressNavigationKeysSource
        XCTAssertTrue(src.contains("INPUT"))
        XCTAssertTrue(src.contains("TEXTAREA"))
        XCTAssertTrue(src.contains("isContentEditable"))
    }

    // MARK: - doCommand

    @MainActor
    func testDoCommandDoesNotCrash() {
        let webView = SilentWebView(frame: .zero, configuration: WKWebViewConfiguration())
        // deleteBackward: via doCommand must be a no-op, not a crash or a goBack call.
        webView.doCommand(by: #selector(NSResponder.deleteBackward(_:)))
    }
}

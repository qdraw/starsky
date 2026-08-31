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

    func testSuppressScriptGuardsSelectionInContentEditable() {
        let src = SilentWebView.suppressNavigationKeysSource
        // WKWebView can report document.body as activeElement even when a
        // contenteditable div has focus; the selection-walk fallback catches that.
        XCTAssertTrue(src.contains("getSelection"))
        XCTAssertTrue(src.contains("commonAncestorContainer"))
        XCTAssertTrue(src.contains("parentElement"))
    }

}

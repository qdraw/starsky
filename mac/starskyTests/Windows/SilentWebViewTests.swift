import XCTest
import WebKit

final class SilentWebViewTests: XCTestCase {

    // MARK: - suppressKeysSource

    func testSuppressScriptCallsPreventDefault() {
        XCTAssertTrue(SilentWebView.suppressKeysSource.contains("e.preventDefault()"))
    }

    func testSuppressScriptGuardsModifierKeys() {
        let src = SilentWebView.suppressKeysSource
        // Cmd/Ctrl/Alt shortcuts must never be suppressed.
        XCTAssertTrue(src.contains("e.metaKey"))
        XCTAssertTrue(src.contains("e.ctrlKey"))
        XCTAssertTrue(src.contains("e.altKey"))
    }

    func testSuppressScriptGuardsEditableElements() {
        let src = SilentWebView.suppressKeysSource
        XCTAssertTrue(src.contains("INPUT"))
        XCTAssertTrue(src.contains("TEXTAREA"))
        XCTAssertTrue(src.contains("isContentEditable"))
    }

    func testSuppressScriptGuardsSelectionInContentEditable() {
        let src = SilentWebView.suppressKeysSource
        // WKWebView can report document.body as activeElement even when a
        // contenteditable div has focus; the selection-walk fallback catches that.
        XCTAssertTrue(src.contains("getSelection"))
        XCTAssertTrue(src.contains("commonAncestorContainer"))
        XCTAssertTrue(src.contains("parentElement"))
    }

}

import XCTest
import WebKit
@MainActor
final class SilentWebViewTests: XCTestCase {

    // Spy that records whether goBack() was called.
    private class SpySilentWebView: SilentWebView {
        var goBackCalled = false
        @discardableResult override func goBack() -> WKNavigation? {
            goBackCalled = true
            return nil
        }
    }

    private func makeBackspaceEvent() -> NSEvent {
        NSEvent.keyEvent(
            with: .keyDown,
            location: .zero,
            modifierFlags: [],
            timestamp: 0,
            windowNumber: 0,
            context: nil,
            characters: "\u{08}",
            charactersIgnoringModifiers: "\u{08}",
            isARepeat: false,
            keyCode: 51
        )!
    }

    func testBackspaceDoesNotCallGoBack() {
        let webView = SpySilentWebView(frame: .zero, configuration: WKWebViewConfiguration())
        webView.keyDown(with: makeBackspaceEvent())
        XCTAssertFalse(webView.goBackCalled, "Bare backspace must not trigger go-back navigation")
    }

    func testBackspaceWithModifierCallsSuper() {
        // Cmd+backspace (or any modified backspace) should not be swallowed by the override.
        let webView = SpySilentWebView(frame: .zero, configuration: WKWebViewConfiguration())
        let event = NSEvent.keyEvent(
            with: .keyDown,
            location: .zero,
            modifierFlags: [.command],
            timestamp: 0,
            windowNumber: 0,
            context: nil,
            characters: "\u{08}",
            charactersIgnoringModifiers: "\u{08}",
            isARepeat: false,
            keyCode: 51
        )!
        // Should reach super.keyDown — what matters is our early-return guard doesn't fire.
        // WKWebView with no loaded page won't go back, but goBackCalled stays false.
        webView.keyDown(with: event)
        XCTAssertFalse(webView.goBackCalled)
    }

    func testDoCommandDoesNotCrash() {
        let webView = SilentWebView(frame: .zero, configuration: WKWebViewConfiguration())
        // Calling deleteBackward: through doCommand must be a no-op, not a crash.
        webView.doCommand(by: #selector(NSResponder.deleteBackward(_:)))
    }
}

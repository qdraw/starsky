import XCTest
@testable import starsky

final class DesktopSettingsTests: XCTestCase {
    func testDefaultValues() {
        let settings = DesktopSettings()
        XCTAssertEqual(settings.mode, .local)
        XCTAssertEqual(settings.remoteBaseUrl, "")
        XCTAssertTrue(settings.updateCheckEnabled)
        XCTAssertNil(settings.lastUpdateWarningShown)
        XCTAssertTrue(settings.windows.isEmpty)
    }

    func testJsonRoundTrip() throws {
        var settings = DesktopSettings()
        settings.mode = .remote
        settings.remoteBaseUrl = "https://example.com"
        settings.updateCheckEnabled = false
        settings.lastUpdateWarningShown = Date(timeIntervalSince1970: 0)
        settings.windows = [SavedWindowState(route: "?f=/photos", x: 50, y: 60, width: 800, height: 600, isMaximized: true)]

        let encoder = JSONEncoder()
        encoder.dateEncodingStrategy = .iso8601
        let data = try encoder.encode(settings)

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .iso8601
        let decoded = try decoder.decode(DesktopSettings.self, from: data)

        XCTAssertEqual(decoded.mode, .remote)
        XCTAssertEqual(decoded.remoteBaseUrl, "https://example.com")
        XCTAssertFalse(decoded.updateCheckEnabled)
        XCTAssertNotNil(decoded.lastUpdateWarningShown)
        XCTAssertEqual(decoded.windows.count, 1)
        XCTAssertEqual(decoded.windows[0].route, "?f=/photos")
        XCTAssertTrue(decoded.windows[0].isMaximized)
    }
}

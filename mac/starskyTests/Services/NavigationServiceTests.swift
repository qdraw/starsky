import XCTest
@testable import starsky

final class NavigationServiceTests: XCTestCase {
    private func makeService(mode: RuntimeMode = .local, remoteUrl: String = "") -> NavigationService {
        let settingsService = SettingsService(settingsFile: URL(fileURLWithPath: "/dev/null"))
        var settings = DesktopSettings()
        settings.mode = mode
        settings.remoteBaseUrl = remoteUrl
        settingsService.save(settings)
        return NavigationService(settings: settingsService)
    }

    func testLocalhostIsAllowed() {
        let svc = makeService()
        let url = URL(string: "http://localhost:5000/photos")!
        XCTAssertTrue(svc.isAllowedOrigin(url, baseUrl: "http://localhost:5000"))
    }

    func testMatchingRemoteOriginIsAllowed() {
        let svc = makeService()
        let url = URL(string: "https://example.com/photos")!
        XCTAssertTrue(svc.isAllowedOrigin(url, baseUrl: "https://example.com"))
    }

    func testDifferentHostIsBlocked() {
        let svc = makeService()
        let url = URL(string: "https://evil.com/photos")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: "https://example.com"))
    }

    func testDifferentSchemeIsBlocked() {
        let svc = makeService()
        let url = URL(string: "http://example.com/photos")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: "https://example.com"))
    }

    func testBuildStartUrlAppendsRoute() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: "http://localhost:5000", route: "?f=/photos")
        XCTAssertEqual(result, "http://localhost:5000?f=/photos")
    }

    func testBuildStartUrlDefaultRoute() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: "http://localhost:5000")
        XCTAssertEqual(result, "http://localhost:5000?f=/")
    }

    func testBuildStartUrlStripsTrailingSlash() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: "http://localhost:5000/", route: "?f=/")
        XCTAssertEqual(result, "http://localhost:5000?f=/")
    }
}

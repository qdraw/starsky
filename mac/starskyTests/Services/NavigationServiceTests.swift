import XCTest
@testable import starsky

final class NavigationServiceTests: XCTestCase {
    private let localBaseUrl = "http://localhost:5000"
    private let remoteBaseUrl = "https://example.com"
    private let evilBaseUrl = "https://evil.com"
    private let altPortUrl = "http://localhost:9999"

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
        let url = URL(string: "\(localBaseUrl)/photos")!
        XCTAssertTrue(svc.isAllowedOrigin(url, baseUrl: localBaseUrl))
    }

    func testMatchingRemoteOriginIsAllowed() {
        let svc = makeService()
        let url = URL(string: "\(remoteBaseUrl)/photos")!
        XCTAssertTrue(svc.isAllowedOrigin(url, baseUrl: remoteBaseUrl))
    }

    func testDifferentHostIsBlocked() {
        let svc = makeService()
        let url = URL(string: "\(evilBaseUrl)/photos")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: remoteBaseUrl))
    }

    func testDifferentSchemeIsBlocked() {
        let svc = makeService()
        let url = URL(string: "http://example.com/photos")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: remoteBaseUrl))
    }

    func testDifferentPortIsBlocked() {
        let svc = makeService()
        let url = URL(string: "\(altPortUrl)/internal")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: localBaseUrl))
    }

    func testBuildStartUrlAppendsRoute() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: localBaseUrl, route: "?f=/photos")
        XCTAssertEqual(result, "\(localBaseUrl)?f=/photos")
    }

    func testBuildStartUrlDefaultRoute() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: localBaseUrl)
        XCTAssertEqual(result, "\(localBaseUrl)?f=/")
    }

    func testBuildStartUrlStripsTrailingSlash() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: "\(localBaseUrl)/", route: "?f=/")
        XCTAssertEqual(result, "\(localBaseUrl)?f=/")
    }
}

import XCTest
@testable import Starsky

final class NavigationServiceTests: XCTestCase {
    private static let localBaseUrl = "http://localhost:5000"
    private static let remoteBaseUrl = "https://example.com"
    private static let remoteBaseUrlHttp = "http://example.com"
    private static let evilBaseUrl = "https://evil.com"
    private static let altPortUrl = "http://localhost:9999"

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
        let url = URL(string: "\(Self.localBaseUrl)/photos")!
        XCTAssertTrue(svc.isAllowedOrigin(url, baseUrl: Self.localBaseUrl))
    }

    func testMatchingRemoteOriginIsAllowed() {
        let svc = makeService()
        let url = URL(string: "\(Self.remoteBaseUrl)/photos")!
        XCTAssertTrue(svc.isAllowedOrigin(url, baseUrl: Self.remoteBaseUrl))
    }

    func testDifferentHostIsBlocked() {
        let svc = makeService()
        let url = URL(string: "\(Self.evilBaseUrl)/photos")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: Self.remoteBaseUrl))
    }

    func testDifferentSchemeIsBlocked() {
        let svc = makeService()
        let url = URL(string: "\(Self.remoteBaseUrlHttp)/photos")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: Self.remoteBaseUrl))
    }

    func testDifferentPortIsBlocked() {
        let svc = makeService()
        let url = URL(string: "\(Self.altPortUrl)/internal")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: Self.localBaseUrl))
    }

    func testBuildStartUrlAppendsRoute() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: Self.localBaseUrl, route: "?f=/photos")
        XCTAssertEqual(result, "\(Self.localBaseUrl)?f=/photos")
    }

    func testBuildStartUrlDefaultRoute() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: Self.localBaseUrl)
        XCTAssertEqual(result, "\(Self.localBaseUrl)?f=/")
    }

    func testBuildStartUrlStripsTrailingSlash() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: "\(Self.localBaseUrl)/", route: "?f=/")
        XCTAssertEqual(result, "\(Self.localBaseUrl)?f=/")
    }

    func testBuildStartUrlPrefixesBarePath() {
        let svc = makeService()
        let result = svc.buildStartUrl(baseUrl: Self.localBaseUrl, route: "photos")
        XCTAssertEqual(result, "\(Self.localBaseUrl)/photos")
    }

    func testIsAllowedOriginReturnsFalseForUrlWithNoHost() {
        let svc = makeService()
        let url = URL(string: "file:///local/path")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: Self.localBaseUrl))
    }

    func testIsAllowedOriginReturnsFalseForBaseUrlWithNoHost() {
        let svc = makeService()
        let url = URL(string: "\(Self.localBaseUrl)/photos")!
        XCTAssertFalse(svc.isAllowedOrigin(url, baseUrl: "http://"))
    }

    func testGetEffectiveBaseUrlLocalWithNilPort() {
        let svc = makeService(mode: .local)
        let result = svc.getEffectiveBaseUrl(localPort: nil)
        XCTAssertEqual(result, "http://localhost:0")
    }

    func testGetEffectiveBaseUrlRemote() {
        let svc = makeService(mode: .remote, remoteUrl: Self.remoteBaseUrl)
        let result = svc.getEffectiveBaseUrl()
        XCTAssertEqual(result, Self.remoteBaseUrl)
    }
}

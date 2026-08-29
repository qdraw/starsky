import XCTest
@testable import Starsky

final class RemoteUrlValidatorTests: XCTestCase {
    private static let exampleBaseUrl = "https://example.com"
    private static let exampleHealthUrl = URL(string: "https://example.com/api/health")!
    private static let ftpUrl = "ftp://example.com"
    private static let unreachableUrl = "https://unreachable.invalid"

    private func makeValidator(responses: [(Int, URL, Data)] = []) -> RemoteUrlValidator {
        FakeURLProtocol.reset()
        for (status, url, data) in responses {
            FakeURLProtocol.enqueue(statusCode: status, url: url, data: data)
        }
        return RemoteUrlValidator(session: FakeURLProtocol.makeSession())
    }

    func testEmptyStringFails() async {
        let v = makeValidator()
        let result = await v.validate(urlString: "")
        XCTAssertFalse(result.success)
        XCTAssertNotNil(result.error)
    }

    func testInvalidSchemeFails() async {
        let v = makeValidator()
        let result = await v.validate(urlString: Self.ftpUrl)
        XCTAssertFalse(result.success)
        XCTAssertTrue(result.error?.contains("http") == true)
    }

    func testHttp200Succeeds() async {
        let v = makeValidator(responses: [(200, Self.exampleHealthUrl, Data())])
        let result = await v.validate(urlString: Self.exampleBaseUrl)
        XCTAssertTrue(result.success)
    }

    func testHttp503Succeeds() async {
        let v = makeValidator(responses: [(503, Self.exampleHealthUrl, Data())])
        let result = await v.validate(urlString: Self.exampleBaseUrl)
        XCTAssertTrue(result.success)
    }

    func testOtherStatusFails() async {
        let v = makeValidator(responses: [(404, Self.exampleHealthUrl, Data())])
        let result = await v.validate(urlString: Self.exampleBaseUrl)
        XCTAssertFalse(result.success)
    }

    func testTrailingSlashStripped() async {
        let v = makeValidator(responses: [(200, Self.exampleHealthUrl, Data())])
        let result = await v.validate(urlString: "\(Self.exampleBaseUrl)/")
        XCTAssertTrue(result.success)
    }

    func testNetworkErrorFails() async {
        FakeURLProtocol.reset()
        let v = RemoteUrlValidator(session: FakeURLProtocol.makeSession())
        let result = await v.validate(urlString: Self.unreachableUrl)
        XCTAssertFalse(result.success)
    }

    func testMalformedUrlFails() async {
        let v = makeValidator()
        let result = await v.validate(urlString: "not a url at all ://??")
        XCTAssertFalse(result.success)
        XCTAssertNotNil(result.error)
    }

    func testWhitespaceOnlyFails() async {
        let v = makeValidator()
        let result = await v.validate(urlString: "   ")
        XCTAssertFalse(result.success)
    }

    func testHttpSchemeSucceeds() async {
        let healthUrl = URL(string: "http://example.com/api/health")!
        let v = makeValidator(responses: [(200, healthUrl, Data())])
        let result = await v.validate(urlString: "http://example.com")
        XCTAssertTrue(result.success)
    }

    func testUrlWithTrailingWhitespaceSucceeds() async {
        let v = makeValidator(responses: [(200, Self.exampleHealthUrl, Data())])
        let result = await v.validate(urlString: "  \(Self.exampleBaseUrl)  ")
        XCTAssertTrue(result.success)
    }

    func testUrlWithNoHostFails() async {
        let v = makeValidator()
        let result = await v.validate(urlString: "https://")
        XCTAssertFalse(result.success)
    }

    func testSchemelessUrlFails() async {
        // "example.com" parses as a URL but has nil scheme — hits the nil-scheme branch
        let v = makeValidator()
        let result = await v.validate(urlString: "example.com")
        XCTAssertFalse(result.success)
        XCTAssertNotNil(result.error)
    }

    func testCustomHealthPathIsUsed() async {
        let customHealthUrl = URL(string: "https://example.com/custom/health")!
        let v = RemoteUrlValidator(
            session: FakeURLProtocol.makeSession(),
            healthPath: "/custom/health"
        )
        FakeURLProtocol.reset()
        FakeURLProtocol.enqueue(statusCode: 200, url: customHealthUrl, data: Data())
        let result = await v.validate(urlString: "https://example.com")
        XCTAssertTrue(result.success)
    }

    func testErrorDescriptionIncludesStatusCode() async {
        let v = makeValidator(responses: [(401, Self.exampleHealthUrl, Data())])
        let result = await v.validate(urlString: Self.exampleBaseUrl)
        XCTAssertFalse(result.success)
        XCTAssertTrue(result.error?.contains("401") == true)
    }
}

import XCTest
@testable import starsky

final class RemoteUrlValidatorTests: XCTestCase {
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
        let result = await v.validate(urlString: "ftp://example.com")
        XCTAssertFalse(result.success)
        XCTAssertTrue(result.error?.contains("http") == true)
    }

    func testHttp200Succeeds() async {
        let url = URL(string: "https://example.com/api/health")!
        let v = makeValidator(responses: [(200, url, Data())])
        let result = await v.validate(urlString: "https://example.com")
        XCTAssertTrue(result.success)
    }

    func testHttp503Succeeds() async {
        let url = URL(string: "https://example.com/api/health")!
        let v = makeValidator(responses: [(503, url, Data())])
        let result = await v.validate(urlString: "https://example.com")
        XCTAssertTrue(result.success)
    }

    func testOtherStatusFails() async {
        let url = URL(string: "https://example.com/api/health")!
        let v = makeValidator(responses: [(404, url, Data())])
        let result = await v.validate(urlString: "https://example.com")
        XCTAssertFalse(result.success)
    }

    func testTrailingSlashStripped() async {
        let url = URL(string: "https://example.com/api/health")!
        let v = makeValidator(responses: [(200, url, Data())])
        let result = await v.validate(urlString: "https://example.com/")
        XCTAssertTrue(result.success)
    }

    func testNetworkErrorFails() async {
        FakeURLProtocol.reset()
        let v = RemoteUrlValidator(session: FakeURLProtocol.makeSession())
        let result = await v.validate(urlString: "https://unreachable.invalid")
        XCTAssertFalse(result.success)
    }
}

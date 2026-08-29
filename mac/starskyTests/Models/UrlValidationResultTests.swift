import XCTest
@testable import Starsky

final class UrlValidationResultTests: XCTestCase {

    func testSuccessResult() {
        let result = UrlValidationResult(success: true, error: nil)
        XCTAssertTrue(result.success)
        XCTAssertNil(result.error)
    }

    func testFailureResult() {
        let result = UrlValidationResult(success: false, error: "Invalid URL")
        XCTAssertFalse(result.success)
        XCTAssertEqual(result.error, "Invalid URL")
    }

    func testSuccessWithNoError() {
        let result = UrlValidationResult(success: true, error: nil)
        XCTAssertNil(result.error)
    }

    func testFailureWithErrorMessage() {
        let message = "Host not reachable"
        let result = UrlValidationResult(success: false, error: message)
        XCTAssertEqual(result.error, message)
    }
}

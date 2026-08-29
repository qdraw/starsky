import XCTest


final class RuntimeModeTests: XCTestCase {

    func testRawValues() {
        XCTAssertEqual(RuntimeMode.local.rawValue, 0)
        XCTAssertEqual(RuntimeMode.remote.rawValue, 1)
    }

    func testInitFromRawValue() {
        XCTAssertEqual(RuntimeMode(rawValue: 0), .local)
        XCTAssertEqual(RuntimeMode(rawValue: 1), .remote)
        XCTAssertNil(RuntimeMode(rawValue: 99))
    }

    func testJsonRoundTrip() throws {
        let encoder = JSONEncoder()
        let decoder = JSONDecoder()

        let localData = try encoder.encode(RuntimeMode.local)
        XCTAssertEqual(try decoder.decode(RuntimeMode.self, from: localData), .local)

        let remoteData = try encoder.encode(RuntimeMode.remote)
        XCTAssertEqual(try decoder.decode(RuntimeMode.self, from: remoteData), .remote)
    }

    func testEquality() {
        XCTAssertEqual(RuntimeMode.local, RuntimeMode.local)
        XCTAssertEqual(RuntimeMode.remote, RuntimeMode.remote)
        XCTAssertNotEqual(RuntimeMode.local, RuntimeMode.remote)
    }
}

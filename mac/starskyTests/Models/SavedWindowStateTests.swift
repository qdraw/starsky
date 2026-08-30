import XCTest


final class SavedWindowStateTests: XCTestCase {

    func testDefaultValues() {
        let state = SavedWindowState()
        XCTAssertEqual(state.route, "?f=/")
        XCTAssertEqual(state.x, 100)
        XCTAssertEqual(state.y, 100)
        XCTAssertEqual(state.width, 1200)
        XCTAssertEqual(state.height, 800)
        XCTAssertFalse(state.isMaximized)
    }

    func testCustomInit() {
        let state = SavedWindowState(route: "?f=/photos", x: 50, y: 60, width: 800, height: 600, isMaximized: true)
        XCTAssertEqual(state.route, "?f=/photos")
        XCTAssertEqual(state.x, 50)
        XCTAssertEqual(state.y, 60)
        XCTAssertEqual(state.width, 800)
        XCTAssertEqual(state.height, 600)
        XCTAssertTrue(state.isMaximized)
    }

    func testJsonRoundTrip() throws {
        var state = SavedWindowState()
        state.route = "?f=/test"
        state.x = 200
        state.y = 300
        state.width = 1024
        state.height = 768
        state.isMaximized = true

        let data = try JSONEncoder().encode(state)
        let decoded = try JSONDecoder().decode(SavedWindowState.self, from: data)

        XCTAssertEqual(decoded.route, "?f=/test")
        XCTAssertEqual(decoded.x, 200)
        XCTAssertEqual(decoded.y, 300)
        XCTAssertEqual(decoded.width, 1024)
        XCTAssertEqual(decoded.height, 768)
        XCTAssertTrue(decoded.isMaximized)
    }

    func testMutability() {
        var state = SavedWindowState()
        state.route = "?f=/new"
        state.isMaximized = true
        XCTAssertEqual(state.route, "?f=/new")
        XCTAssertTrue(state.isMaximized)
    }
}

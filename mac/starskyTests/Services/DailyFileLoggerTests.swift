import XCTest
@testable import starsky

final class DailyFileLoggerTests: XCTestCase {
    private var tempDir: URL!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try? FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    func testInfoWritesToFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir)
        logger.info("Hello from test", category: "Test")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        XCTAssertFalse(files.isEmpty, "Expected a log file to be created")
    }

    func testLogContainsMessage() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir)
        logger.info("unique-test-message-xyz", category: "Test")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        guard let file = files.first else { XCTFail("No log file"); return }
        let content = try String(contentsOf: file)
        XCTAssertTrue(content.contains("unique-test-message-xyz"))
    }

    func testMultipleWritesAppend() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir)
        logger.info("line-one", category: "Test")
        logger.info("line-two", category: "Test")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        guard let file = files.first else { XCTFail("No log file"); return }
        let content = try String(contentsOf: file)
        XCTAssertTrue(content.contains("line-one"))
        XCTAssertTrue(content.contains("line-two"))
    }

    func testLogWithError() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir)
        let err = NSError(domain: "test", code: 42, userInfo: [NSLocalizedDescriptionKey: "test-error"])
        logger.error("error-happened", error: err, category: "Test")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        guard let file = files.first else { XCTFail("No log file"); return }
        let content = try String(contentsOf: file)
        XCTAssertTrue(content.contains("error-happened"))
    }
}

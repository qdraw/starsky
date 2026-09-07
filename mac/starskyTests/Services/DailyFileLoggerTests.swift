import XCTest


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

    func testWarningWritesToFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir)
        logger.warning("warn-message", category: "Test")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        XCTAssertFalse(files.isEmpty)
        let content = try String(contentsOf: files[0])
        XCTAssertTrue(content.contains("warn-message"))
        XCTAssertTrue(content.contains("WARN"))
    }

    func testErrorWithNoErrorObjectWritesMessage() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir)
        logger.error("just-a-message", error: nil, category: "Test")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        guard let file = files.first else { XCTFail("No log file"); return }
        let content = try String(contentsOf: file)
        XCTAssertTrue(content.contains("just-a-message"))
        XCTAssertTrue(content.contains("ERROR"))
    }

    func testLogCategoryIsIncluded() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir)
        logger.info("msg", category: "MyCategory")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        let content = try String(contentsOf: files[0])
        XCTAssertTrue(content.contains("MyCategory"))
    }

    // MARK: - Debug build file naming

    func testDebugBuildWritesToDebugSuffixedFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: true)
        logger.info("debug-msg", category: "Test")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        let logFiles = files.filter { $0.pathExtension == "log" }
        // Expected filename pattern: starsky-yyyy-MM-dd-debug.log
        let datePattern = #"\d{4}-\d{2}-\d{2}"#
        XCTAssertTrue(logFiles.allSatisfy { file in
            let name = file.lastPathComponent
            return name.contains(DailyFileLogger.debugSuffix) &&
                   name.range(of: datePattern, options: .regularExpression) != nil
        }, "Debug build should write to a starsky-yyyy-MM-dd-debug.log file")
    }

    func testReleaseBuildDoesNotWriteToDebugSuffixedFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("release-msg", category: "Test")
        let files = try FileManager.default.contentsOfDirectory(at: tempDir, includingPropertiesForKeys: nil)
        let logFiles = files.filter { $0.pathExtension == "log" && $0.lastPathComponent != DailyFileLogger.symlinkName }
        XCTAssertFalse(logFiles.allSatisfy { $0.lastPathComponent.contains(DailyFileLogger.debugSuffix) },
                       "Release build should not write to a *-debug.log file")
    }

    // MARK: - Symlink (release only)

    func testSymlinkIsCreatedInReleaseBuild() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("symlink-test", category: "Test")
        let symlink = tempDir.appendingPathComponent(DailyFileLogger.symlinkName)
        XCTAssertTrue(FileManager.default.fileExists(atPath: symlink.path), "Expected symlink to exist in release build")
    }

    func testSymlinkIsNotCreatedInDebugBuild() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: true)
        logger.info("symlink-test", category: "Test")
        let symlink = tempDir.appendingPathComponent(DailyFileLogger.symlinkName)
        XCTAssertFalse(FileManager.default.fileExists(atPath: symlink.path), "Symlink must not be created in debug build")
    }

    func testSymlinkPointsToCurrentLogFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("symlink-target-test", category: "Test")
        let symlink = tempDir.appendingPathComponent(DailyFileLogger.symlinkName)
        let destination = try FileManager.default.destinationOfSymbolicLink(atPath: symlink.path)
        XCTAssertTrue(destination.hasSuffix(".log"), "Symlink should point to a .log file")
        XCTAssertFalse(destination.hasSuffix(DailyFileLogger.symlinkName), "Symlink must not point to itself")
        XCTAssertFalse(destination.contains(DailyFileLogger.debugSuffix), "Symlink must not point to a debug log")
    }

    func testSymlinkContentMatchesLogFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("via-symlink", category: "Test")
        let symlink = tempDir.appendingPathComponent(DailyFileLogger.symlinkName)
        let content = try String(contentsOf: symlink)
        XCTAssertTrue(content.contains("via-symlink"))
    }
}

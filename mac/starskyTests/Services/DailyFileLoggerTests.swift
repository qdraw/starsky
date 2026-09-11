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

    // MARK: - Basic logging

    func testInfoWritesToLatestFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("Hello from test", category: "Test")
        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        XCTAssertTrue(FileManager.default.fileExists(atPath: latest.path))
    }

    func testLogContainsMessage() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("unique-test-message-xyz", category: "Test")
        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let content = try String(contentsOf: latest)
        XCTAssertTrue(content.contains("unique-test-message-xyz"))
    }

    func testMultipleWritesAppend() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("line-one", category: "Test")
        logger.info("line-two", category: "Test")
        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let content = try String(contentsOf: latest)
        XCTAssertTrue(content.contains("line-one"))
        XCTAssertTrue(content.contains("line-two"))
    }

    func testLogWithError() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        let err = NSError(domain: "test", code: 42, userInfo: [NSLocalizedDescriptionKey: "test-error"])
        logger.error("error-happened", error: err, category: "Test")
        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let content = try String(contentsOf: latest)
        XCTAssertTrue(content.contains("error-happened"))
    }

    func testWarningWritesToLatestFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.warning("warn-message", category: "Test")
        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let content = try String(contentsOf: latest)
        XCTAssertTrue(content.contains("warn-message"))
        XCTAssertTrue(content.contains("WARN"))
    }

    func testErrorWithNoErrorObjectWritesMessage() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.error("just-a-message", error: nil, category: "Test")
        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let content = try String(contentsOf: latest)
        XCTAssertTrue(content.contains("just-a-message"))
        XCTAssertTrue(content.contains("ERROR"))
    }

    func testLogCategoryIsIncluded() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("msg", category: "MyCategory")
        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let content = try String(contentsOf: latest)
        XCTAssertTrue(content.contains("MyCategory"))
    }

    // MARK: - Debug build file naming

    func testDebugBuildWritesToLatestDebugFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: true)
        logger.info("debug-msg", category: "Test")
        let latestDebug = tempDir.appendingPathComponent(DailyFileLogger.latestDebugFileName)
        XCTAssertTrue(FileManager.default.fileExists(atPath: latestDebug.path),
                      "Debug build should write to \(DailyFileLogger.latestDebugFileName)")
    }

    func testDebugBuildDoesNotWriteToReleasLatestFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: true)
        logger.info("debug-msg", category: "Test")
        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        XCTAssertFalse(FileManager.default.fileExists(atPath: latest.path),
                       "Debug build must not write to \(DailyFileLogger.latestFileName)")
    }

    func testReleaseBuildDoesNotWriteToDebugLatestFile() throws {
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false)
        logger.info("release-msg", category: "Test")
        let latestDebug = tempDir.appendingPathComponent(DailyFileLogger.latestDebugFileName)
        XCTAssertFalse(FileManager.default.fileExists(atPath: latestDebug.path),
                       "Release build must not write to \(DailyFileLogger.latestDebugFileName)")
    }

    // MARK: - Day rollover

    func testDayRolloverArchivesLatestToDateFile() throws {
        var day = makeDate(year: 2026, month: 1, day: 10)
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false, dateProvider: { day })
        logger.info("day-one", category: "Test")

        day = makeDate(year: 2026, month: 1, day: 11)
        logger.info("day-two", category: "Test")

        let archive = tempDir.appendingPathComponent("starsky-2026-01-10.log")
        XCTAssertTrue(FileManager.default.fileExists(atPath: archive.path),
                      "Previous day's log should be archived")
        let archiveContent = try String(contentsOf: archive)
        XCTAssertTrue(archiveContent.contains("day-one"))
        XCTAssertFalse(archiveContent.contains("day-two"))

        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let latestContent = try String(contentsOf: latest)
        XCTAssertTrue(latestContent.contains("day-two"))
        XCTAssertFalse(latestContent.contains("day-one"))
    }

    func testDayRolloverDebugArchivesWithDebugSuffix() throws {
        var day = makeDate(year: 2026, month: 1, day: 10)
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: true, dateProvider: { day })
        logger.info("debug-day-one", category: "Test")

        day = makeDate(year: 2026, month: 1, day: 11)
        logger.info("debug-day-two", category: "Test")

        let archive = tempDir.appendingPathComponent("starsky-2026-01-10\(DailyFileLogger.debugSuffix).log")
        XCTAssertTrue(FileManager.default.fileExists(atPath: archive.path),
                      "Previous day's debug log should be archived with -debug suffix")
        let content = try String(contentsOf: archive)
        XCTAssertTrue(content.contains("debug-day-one"))
    }

    // MARK: - App restart

    func testAppRestartArchivesExistingLatestFileByModDate() throws {
        let latestFile = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        try "old-session-content\n".write(to: latestFile, atomically: true, encoding: .utf8)

        // Set the modification date to 2026-01-09 so the logger can derive the archive name
        let oldDate = makeDate(year: 2026, month: 1, day: 9)
        try FileManager.default.setAttributes([.modificationDate: oldDate], ofItemAtPath: latestFile.path)

        let newDay = makeDate(year: 2026, month: 1, day: 10)
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false, dateProvider: { newDay })
        logger.info("new-session", category: "Test")

        let archive = tempDir.appendingPathComponent("starsky-2026-01-09.log")
        XCTAssertTrue(FileManager.default.fileExists(atPath: archive.path),
                      "Previous session's latest should be archived using its modification date")
        let archiveContent = try String(contentsOf: archive)
        XCTAssertTrue(archiveContent.contains("old-session-content"))

        let latestContent = try String(contentsOf: latestFile)
        XCTAssertTrue(latestContent.contains("new-session"))
        XCTAssertFalse(latestContent.contains("old-session-content"))
    }

    func testSameDayRestartAppendsToExistingLatestFile() throws {
        let today = makeDate(year: 2026, month: 1, day: 10)
        let logger1 = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false, dateProvider: { today })
        logger1.info("session-one", category: "Test")

        let logger2 = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false, dateProvider: { today })
        logger2.info("session-two", category: "Test")

        let latest = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let content = try String(contentsOf: latest)
        XCTAssertTrue(content.contains("session-one"))
        XCTAssertTrue(content.contains("session-two"))
    }

    // MARK: - Log pruning

    func testOldLogsAreDeletedAfter90Days() throws {
        let fm = FileManager.default
        // Plant a 91-day-old dated log file
        let oldFile = tempDir.appendingPathComponent("starsky-2025-01-01.log")
        try "old-content\n".write(to: oldFile, atomically: true, encoding: .utf8)

        // Plant a recent dated log (within retention window)
        let recentFile = tempDir.appendingPathComponent("starsky-2026-06-15.log")
        try "recent-content\n".write(to: recentFile, atomically: true, encoding: .utf8)

        // First write triggers pruning; "today" is 2026-09-11 (> 90 days after 2025-01-01)
        let today = makeDate(year: 2026, month: 9, day: 11)
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false, dateProvider: { today })
        logger.info("trigger-prune", category: "Test")

        XCTAssertFalse(fm.fileExists(atPath: oldFile.path), "Log older than 90 days should be deleted")
        XCTAssertTrue(fm.fileExists(atPath: recentFile.path), "Recent log within 90 days should be kept")
    }

    func testLatestFilesAreNeverPruned() throws {
        let fm = FileManager.default
        let latestFile = tempDir.appendingPathComponent(DailyFileLogger.latestFileName)
        let latestDebugFile = tempDir.appendingPathComponent(DailyFileLogger.latestDebugFileName)
        try "latest\n".write(to: latestFile, atomically: true, encoding: .utf8)
        try "latest-debug\n".write(to: latestDebugFile, atomically: true, encoding: .utf8)

        // Set old modification dates to simulate them looking ancient
        let ancientDate = makeDate(year: 2020, month: 1, day: 1)
        try fm.setAttributes([.modificationDate: ancientDate], ofItemAtPath: latestFile.path)
        try fm.setAttributes([.modificationDate: ancientDate], ofItemAtPath: latestDebugFile.path)

        let today = makeDate(year: 2026, month: 9, day: 11)
        let logger = DailyFileLogger(logsDirectory: tempDir, isDebugBuild: false, dateProvider: { today })
        logger.info("trigger-prune", category: "Test")

        // latestFile was archived (rolled over); latestDebugFile must survive — pruning must never touch it
        XCTAssertTrue(fm.fileExists(atPath: latestDebugFile.path),
                      "\(DailyFileLogger.latestDebugFileName) must not be deleted by pruning")
    }

    // MARK: - Helpers

    private func makeDate(year: Int, month: Int, day: Int) -> Date {
        var c = DateComponents()
        c.year = year; c.month = month; c.day = day
        c.hour = 12; c.minute = 0; c.second = 0
        return Calendar(identifier: .gregorian).date(from: c)!
    }
}

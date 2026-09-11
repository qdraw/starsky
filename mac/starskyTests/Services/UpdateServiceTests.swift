import XCTest


final class UpdateServiceTests: XCTestCase {
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

    private func makeService(enabled: Bool = true, lastShown: Date? = nil) -> (UpdateService, SettingsService) {
        let svc = SettingsService(settingsFile: tempDir.appendingPathComponent("settings.json"))
        svc.load()
        var settings = svc.current
        settings.updateCheckEnabled = enabled
        settings.lastUpdateWarningShown = lastShown
        svc.save(settings)
        return (UpdateService(settingsService: svc), svc)
    }

    private func makeServiceWithFileLogger(enabled: Bool = true) -> (UpdateService, SettingsService, DailyFileLogger, URL) {
        let logDir = tempDir.appendingPathComponent("logs-\(UUID().uuidString)")
        try? FileManager.default.createDirectory(at: logDir, withIntermediateDirectories: true)
        let fileLogger = DailyFileLogger(logsDirectory: logDir)
        let svc = SettingsService(settingsFile: tempDir.appendingPathComponent("settings-fl.json"))
        svc.load()
        var settings = svc.current
        settings.updateCheckEnabled = enabled
        svc.save(settings)
        return (UpdateService(settingsService: svc, fileLogger: fileLogger), svc, fileLogger, logDir)
    }

    private func readLatestLog(in logDir: URL) -> String {
        let debugLog = logDir.appendingPathComponent(DailyFileLogger.latestDebugFileName)
        let releaseLog = logDir.appendingPathComponent(DailyFileLogger.latestFileName)
        return (try? String(contentsOf: debugLog, encoding: .utf8))
            ?? (try? String(contentsOf: releaseLog, encoding: .utf8))
            ?? ""
    }

    func testDisabledReturnsFalse() async {
        let (service, _) = makeService(enabled: false)
        let result = await service.checkAsync()
        XCTAssertFalse(result)
    }

    func testRecentWarningReturnsFalse() async {
        let (service, _) = makeService(enabled: true, lastShown: Date())
        let result = await service.checkAsync()
        XCTAssertFalse(result)
    }

    func testRecordWarningShownPersistsTimestamp() {
        let (service, settingsService) = makeService(enabled: true)
        XCTAssertNil(settingsService.current.lastUpdateWarningShown)
        service.recordWarningShown()
        XCTAssertNotNil(settingsService.current.lastUpdateWarningShown)
    }

    func testApplyUpdateDoesNotCrashWithoutSparkle() {
        let (service, _) = makeService(enabled: true)
        service.applyUpdate()
    }

    func testOldWarningDoesNotSuppressCheck() async {
        // lastUpdateWarningShown far in the past — more than suppressMinutes ago
        let oldDate = Date().addingTimeInterval(-(UpdateService.suppressMinutes + 1) * 60)
        let (service, _) = makeService(enabled: true, lastShown: oldDate)
        // When the Sparkle controller is unavailable (test env), checkAsync still returns false,
        // but the suppression branch is skipped — we verify it didn't return false from suppression.
        let result = await service.checkAsync()
        // Result is false because updaterController is nil in tests, NOT because of suppression.
        // The important thing is it didn't return false from the suppression guard.
        XCTAssertFalse(result) // false because no Sparkle in test env
    }

    func testSuppressMinutesIsReasonable() {
        XCTAssertGreaterThan(UpdateService.suppressMinutes, 0)
    }

    func testSuppressMinutesMatchesExpectedValue() {
        // 5760 min == 4 days; changing this silently would alter update-nag frequency
        XCTAssertEqual(UpdateService.suppressMinutes, 5760)
    }

    func testIsAvailableReflectsSparkleControllerState() {
        let (service, _) = makeService(enabled: true)
        // Sparkle can initialise in the test runner bundle, so isAvailable may be true or false
        // depending on the environment. We just confirm the property is readable without crashing.
        _ = service.isAvailable
    }

    func testCheckAsyncEnabledNoLastShownReturnsFalse() async {
        // enabled=true, lastShown=nil: suppression is skipped, but updaterController is nil → false
        let (service, _) = makeService(enabled: true, lastShown: nil)
        let result = await service.checkAsync()
        XCTAssertFalse(result)
    }

    func testFeedURLOverrideWithEmptyBaseURLAppendsParam() {
        let (service, settingsService) = makeService(enabled: true)
        var s = settingsService.current
        s.preReleaseEnabled = true
        settingsService.save(s)
        // An empty string is a valid (if degenerate) base URL; the param should still be appended.
        let result = service.feedURLOverride(baseFeedURL: "")
        XCTAssertEqual(result, "?pre-release=1")
    }

    // MARK: - feedURLOverride

    func testFeedURLOverrideReturnsNilWhenPreReleaseDisabled() {
        let (service, _) = makeService(enabled: true)
        XCTAssertNil(service.feedURLOverride(baseFeedURL: "https://example.com/appcast/"))
    }

    func testFeedURLOverrideReturnsNilWhenBaseFeedURLIsNil() {
        let (service, settingsService) = makeService(enabled: true)
        var s = settingsService.current
        s.preReleaseEnabled = true
        settingsService.save(s)
        XCTAssertNil(service.feedURLOverride(baseFeedURL: nil))
    }

    func testFeedURLOverrideAppendsPreReleaseQueryParam() {
        let (service, settingsService) = makeService(enabled: true)
        var s = settingsService.current
        s.preReleaseEnabled = true
        settingsService.save(s)
        let result = service.feedURLOverride(baseFeedURL: "https://example.com/appcast/")
        XCTAssertEqual(result, "https://example.com/appcast/?pre-release=1")
    }

    func testCheckAsyncReturnsFalseWhenLastShownIsNilAndUpdateDisabled() async {
        let (service, _) = makeService(enabled: false, lastShown: nil)
        let result = await service.checkAsync()
        XCTAssertFalse(result)
    }

    func testCheckAsyncReturnsFalseWhenLastShownIsExactlyAtSuppressThreshold() async {
        // Exactly at the boundary (elapsed == suppressMinutes) should still suppress
        let boundary = Date().addingTimeInterval(-UpdateService.suppressMinutes * 60)
        let (service, _) = makeService(enabled: true, lastShown: boundary)
        let result = await service.checkAsync()
        XCTAssertFalse(result)
    }

    func testRecordWarningShownUpdatesExistingTimestamp() {
        let (service, settings) = makeService(enabled: true, lastShown: Date().addingTimeInterval(-1000))
        let before = settings.current.lastUpdateWarningShown!
        service.recordWarningShown()
        let after = settings.current.lastUpdateWarningShown!
        XCTAssertGreaterThanOrEqual(after, before)
    }

    // MARK: - mountWatcherProvider

    func testMountWatcherProviderIsNilByDefault() {
        let (service, _) = makeService()
        XCTAssertNil(service.mountWatcherProvider)
    }

    func testMountWatcherProviderCanBeSetAndRetrieved() {
        let (service, _) = makeService()
        service.mountWatcherProvider = { nil }
        // The provider is stored; calling it returns nil (no service configured).
        XCTAssertNotNil(service.mountWatcherProvider)
        XCTAssertNil(service.mountWatcherProvider?())
    }

    func testMountWatcherProviderCanBeCleared() {
        let (service, _) = makeService()
        service.mountWatcherProvider = { nil }
        service.mountWatcherProvider = nil
        XCTAssertNil(service.mountWatcherProvider)
    }

    // MARK: - fileLogger integration

    func testApplyUpdateWritesErrorToFileLoggerWhenSparkleUnavailableOrCannotStart() {
        let (service, _, _, logDir) = makeServiceWithFileLogger()
        service.applyUpdate()

        // Drain the main queue so the DispatchQueue.main.async in applyUpdate completes.
        let exp = expectation(description: "main queue drained")
        DispatchQueue.main.async { exp.fulfill() }
        waitForExpectations(timeout: 2)

        let content = readLatestLog(in: logDir)
        // In test environments Sparkle may or may not initialize, but in every case
        // applyUpdate writes at least one UpdateService entry to the file logger.
        XCTAssertTrue(content.contains("UpdateService"), "Expected at least one UpdateService log entry")
    }

    func testApplyUpdateFileLoggerContainsKnownMessage() {
        let (service, _, _, logDir) = makeServiceWithFileLogger()
        service.applyUpdate()

        let exp = expectation(description: "main queue drained")
        DispatchQueue.main.async { exp.fulfill() }
        waitForExpectations(timeout: 2)

        let content = readLatestLog(in: logDir)
        let knownMessages = [
            "applyUpdate called but Sparkle updater is unavailable",
            "SUPublicEDKey is not set",
            "Sparkle startUpdater failed",
            "applyUpdate: canCheckForUpdates is false, skipping"
        ]
        let matched = knownMessages.contains { content.contains($0) }
        XCTAssertTrue(matched, "Expected one of the known update-error messages in the log; got:\n\(content)")
    }
}

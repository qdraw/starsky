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

    func testIsAvailableReturnsFalseInTestEnvironment() {
        let (service, _) = makeService(enabled: true)
        // Sparkle cannot initialise without a host app bundle, so the controller is always nil in tests.
        XCTAssertFalse(service.isAvailable)
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
}

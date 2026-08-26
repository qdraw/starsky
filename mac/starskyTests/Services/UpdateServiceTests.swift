import XCTest
@testable import starsky

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
}

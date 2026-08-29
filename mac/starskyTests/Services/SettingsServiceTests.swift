import XCTest


final class SettingsServiceTests: XCTestCase {
    private static let remoteBaseUrl = "https://example.com"
    private static let roundtripBaseUrl = "https://roundtrip.example.com"

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

    func testMissingFileReturnsDefaults() {
        let service = SettingsService(settingsFile: tempDir.appendingPathComponent("nonexistent.json"))
        service.load()
        XCTAssertEqual(service.current.mode, .local)
        XCTAssertTrue(service.current.updateCheckEnabled)
    }

    func testValidJsonLoads() throws {
        let file = tempDir.appendingPathComponent("settings.json")
        let json = """
        {"mode":1,"remoteBaseUrl":"\(Self.remoteBaseUrl)","updateCheckEnabled":false,"windows":[]}
        """
        try json.data(using: .utf8)!.write(to: file)
        let service = SettingsService(settingsFile: file)
        service.load()
        XCTAssertEqual(service.current.mode, .remote)
        XCTAssertEqual(service.current.remoteBaseUrl, Self.remoteBaseUrl)
        XCTAssertFalse(service.current.updateCheckEnabled)
    }

    func testCorruptJsonFallsBackToDefaults() throws {
        let file = tempDir.appendingPathComponent("settings.json")
        try "not valid json {{{".data(using: .utf8)!.write(to: file)
        let service = SettingsService(settingsFile: file)
        service.load()
        XCTAssertEqual(service.current.mode, .local)
    }

    func testSaveThenLoadRoundTrip() throws {
        let file = tempDir.appendingPathComponent("settings.json")
        let service = SettingsService(settingsFile: file)
        service.load()
        var settings = service.current
        settings.remoteBaseUrl = Self.roundtripBaseUrl
        settings.mode = .remote
        service.save(settings)

        let service2 = SettingsService(settingsFile: file)
        service2.load()
        XCTAssertEqual(service2.current.remoteBaseUrl, Self.roundtripBaseUrl)
        XCTAssertEqual(service2.current.mode, .remote)
    }

    func testSaveNoArgPersistsCurrentSettings() throws {
        let file = tempDir.appendingPathComponent("settings.json")
        let service = SettingsService(settingsFile: file)
        service.load()
        var settings = service.current
        settings.remoteBaseUrl = "https://noarg.example.com"
        service.save(settings)
        service.save()

        let service2 = SettingsService(settingsFile: file)
        service2.load()
        XCTAssertEqual(service2.current.remoteBaseUrl, "https://noarg.example.com")
    }

    func testSaveToUnwritablePathDoesNotCrash() {
        let service = SettingsService(settingsFile: URL(fileURLWithPath: "/nonexistent/dir/settings.json"))
        var settings = service.current
        settings.remoteBaseUrl = "https://example.com"
        service.save(settings)
    }
}

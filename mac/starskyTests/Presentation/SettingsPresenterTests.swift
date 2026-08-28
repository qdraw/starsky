import XCTest
@testable import starsky

@MainActor
final class SettingsPresenterTests: XCTestCase {
    private var tempDir: URL!
    private var settingsService: SettingsService!
    private var mockWindowManager: MockWindowManager!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try? FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
        settingsService = SettingsService(settingsFile: tempDir.appendingPathComponent("settings.json"))
        settingsService.load()
        mockWindowManager = MockWindowManager()
        FakeURLProtocol.reset()
    }

    override func tearDown() {
        FakeURLProtocol.reset()
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    private func makePresenter(validator: RemoteUrlValidator? = nil) -> SettingsPresenter {
        let v = validator ?? RemoteUrlValidator(session: FakeURLProtocol.makeSession())
        return SettingsPresenter(
            settingsService: settingsService,
            remoteUrlValidator: v,
            windowManager: mockWindowManager
        )
    }

    // MARK: - modeChanged

    func testModeChangedToRemoteSavesSettings() {
        let presenter = makePresenter()
        presenter.modeChanged(to: .remote)
        XCTAssertEqual(settingsService.current.mode, .remote)
    }

    func testModeChangedToLocalSavesSettings() {
        var s = settingsService.current
        s.mode = .remote
        settingsService.save(s)

        let presenter = makePresenter()
        presenter.modeChanged(to: .local)
        XCTAssertEqual(settingsService.current.mode, .local)
    }

    func testModeChangedToRemoteFiresOnSwitchToRemote() {
        let presenter = makePresenter()
        var fired = false
        presenter.onSwitchToRemote = { fired = true }
        presenter.modeChanged(to: .remote)
        XCTAssertTrue(fired)
    }

    func testModeChangedToLocalFiresOnSwitchToLocal() {
        var s = settingsService.current
        s.mode = .remote
        settingsService.save(s)

        let presenter = makePresenter()
        var fired = false
        presenter.onSwitchToLocal = { fired = true }
        presenter.modeChanged(to: .local)
        XCTAssertTrue(fired)
    }

    func testModeChangedToSameValueDoesNotFireCallbacks() {
        let presenter = makePresenter()
        var localFired = false
        var remoteFired = false
        presenter.onSwitchToLocal = { localFired = true }
        presenter.onSwitchToRemote = { remoteFired = true }
        presenter.modeChanged(to: .local)
        XCTAssertFalse(localFired)
        XCTAssertFalse(remoteFired)
    }

    func testModeChangedNotifiesViewOfFieldState() {
        let presenter = makePresenter()
        let mockView = MockSettingsView()
        presenter.view = mockView
        presenter.modeChanged(to: .remote)
        XCTAssertTrue(mockView.urlFieldEnabled)
        XCTAssertTrue(mockView.saveEnabled)
    }

    func testModeChangedToLocalDisablesUrlField() {
        var s = settingsService.current
        s.mode = .remote
        settingsService.save(s)

        let presenter = makePresenter()
        let mockView = MockSettingsView()
        presenter.view = mockView
        presenter.modeChanged(to: .local)
        XCTAssertFalse(mockView.urlFieldEnabled)
        XCTAssertFalse(mockView.saveEnabled)
    }

    // MARK: - updateCheckChanged

    func testUpdateCheckChangedSavesEnabledState() {
        let presenter = makePresenter()
        presenter.updateCheckChanged(enabled: false)
        XCTAssertFalse(settingsService.current.updateCheckEnabled)
        presenter.updateCheckChanged(enabled: true)
        XCTAssertTrue(settingsService.current.updateCheckEnabled)
    }

    // MARK: - saveUrl

    func testSaveUrlEmptyStringShowsError() async {
        let presenter = makePresenter()
        let mockView = MockSettingsView()
        presenter.view = mockView

        await presenter.saveUrl("")

        XCTAssertFalse(mockView.lastResultSuccess ?? true)
    }

    func testSaveUrlInvalidSchemeShowsError() async {
        let presenter = makePresenter()
        let mockView = MockSettingsView()
        presenter.view = mockView

        await presenter.saveUrl("ftp://example.com")

        XCTAssertFalse(mockView.lastResultSuccess ?? true)
    }

    func testSaveUrlSuccessfulValidationSavesAndReopens() async {
        let url = URL(string: "https://myserver.com")!
        FakeURLProtocol.enqueue(statusCode: 200, url: url.appendingPathComponent("api/health"))

        let presenter = makePresenter()
        let mockView = MockSettingsView()
        presenter.view = mockView

        await presenter.saveUrl("https://myserver.com")

        XCTAssertTrue(mockView.lastResultSuccess ?? false)
        XCTAssertEqual(settingsService.current.remoteBaseUrl, "https://myserver.com")
        XCTAssertTrue(mockWindowManager.reopenAllCalled)
    }

    func testSaveUrlStripsTrailingSlash() async {
        let url = URL(string: "https://myserver.com")!
        FakeURLProtocol.enqueue(statusCode: 200, url: url.appendingPathComponent("api/health"))

        let presenter = makePresenter()
        await presenter.saveUrl("https://myserver.com/")

        XCTAssertEqual(settingsService.current.remoteBaseUrl, "https://myserver.com")
    }

    func testSaveUrlServerErrorShowsError() async {
        let url = URL(string: "https://badserver.com")!
        FakeURLProtocol.enqueue(statusCode: 404, url: url.appendingPathComponent("api/health"))

        let presenter = makePresenter()
        let mockView = MockSettingsView()
        presenter.view = mockView

        await presenter.saveUrl("https://badserver.com")

        XCTAssertFalse(mockView.lastResultSuccess ?? true)
        XCTAssertFalse(mockWindowManager.reopenAllCalled)
    }

    func testSaveUrlShowsValidatingWhileInProgress() async {
        let presenter = makePresenter()
        let mockView = MockSettingsView()
        presenter.view = mockView

        await presenter.saveUrl("")

        XCTAssertTrue(mockView.validatingCalled)
    }
}

// MARK: - Test doubles

@MainActor
private class MockWindowManager: WindowManagerProtocol {
    var reopenAllCalled = false
    func openMainWindow(route: String?) {
        // Intentionally no-op: these tests assert settings-save/reopen behavior only.
        // openMainWindow is required by the protocol but not part of this scenario.
        _ = route
    }
    func reopenAll() { reopenAllCalled = true }
}

private class MockSettingsView: SettingsView {
    var validatingCalled = false
    var lastResultSuccess: Bool?
    var lastResultMessage: String?
    var urlFieldEnabled = true
    var saveEnabled = true

    func setValidating() { validatingCalled = true }
    func setResult(success: Bool, message: String) {
        lastResultSuccess = success
        lastResultMessage = message
    }
    func setUrlFieldEnabled(_ enabled: Bool) { urlFieldEnabled = enabled }
    func setSaveEnabled(_ enabled: Bool) { saveEnabled = enabled }
}

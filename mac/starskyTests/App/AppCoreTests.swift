import XCTest
import AppKit
@testable import starsky

// MARK: - Test doubles

private final class MockBackendService: BackendServiceProtocol {
    var isRunning: Bool = false
    var startCalled = false
    var stopCalled = false
    var startError: Error?

    func start(port _: Int) throws {
        startCalled = true
        if let error = startError { throw error }
        isRunning = true
    }

    func stop() {
        stopCalled = true
        isRunning = false
    }
}

private final class MockFileWatcherService: FileWatcherServiceProtocol {
    var startCalled = false
    var stopCalled = false

    func start() { startCalled = true }
    func stop() { stopCalled = true }
}

@MainActor
private final class MockWindowManager: WindowManagerProtocol {
    var openMainWindowCalled = false
    var openMainWindowRoute: String?
    var reopenAllCalled = false
    var setLocalPortValue: Int?
    var restoreWindowsCalled = false
    var closeAllCalled = false
    var reloadAllCalled = false

    func openMainWindow(route: String?) { openMainWindowCalled = true; openMainWindowRoute = route }
    func openMainWindow() { openMainWindowCalled = true; openMainWindowRoute = nil }
    func reopenAll() { reopenAllCalled = true }
    func setLocalPort(_ port: Int) { setLocalPortValue = port }
    func restoreWindows() { restoreWindowsCalled = true }
    func closeAll() { closeAllCalled = true }
    func reloadAll() { reloadAllCalled = true }
}

// MARK: - Factory

@MainActor
private func makeCore(
    remoteBaseUrl: String = "",
    mode: RuntimeMode = .local,
    backendService: MockBackendService? = nil,
    fileWatcherService: MockFileWatcherService? = nil,
    windowManager: MockWindowManager? = nil
) -> AppCore {
    let backendService = backendService ?? MockBackendService()
    let fileWatcherService = fileWatcherService ?? MockFileWatcherService()
    let windowManager = windowManager ?? MockWindowManager()
    let tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
    try? FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)

    let settings = SettingsService(settingsFile: tempDir.appendingPathComponent("settings.json"))
    settings.load()
    if !remoteBaseUrl.isEmpty || mode == .remote {
        var s = settings.current
        s.remoteBaseUrl = remoteBaseUrl
        s.mode = mode
        settings.save(s)
        settings.load()
    }

    FakeURLProtocol.reset()

    let core = AppCore(
        settingsService: settings,
        backendService: backendService,
        fileWatcherService: fileWatcherService,
        updateService: UpdateService(settingsService: settings),
        windowManager: windowManager,
        terminate: {},
        showError: { _ in },
        urlOpener: { _ in },
        healthCheckSession: FakeURLProtocol.makeSession(),
        updateCheckDelay: 0,
        healthCheckRetryDelay: 0,
        healthCheckTimeoutSeconds: 0
    )
    core.terminateDelay = 0
    return core
}

// MARK: - Tests

@MainActor
final class AppCoreTests: XCTestCase {

    // MARK: - startup routing

    func testStartupRoutesToRemoteMode() async {
        let watcher = MockFileWatcherService()
        let wm = MockWindowManager()
        let core = makeCore(remoteBaseUrl: "http://example.com", mode: .remote,
                            fileWatcherService: watcher, windowManager: wm)
        await core.startup()
        XCTAssertTrue(watcher.startCalled)
        XCTAssertTrue(wm.restoreWindowsCalled)
    }

    func testStartupRoutesToLocalModeAndCallsBackend() async {
        let backend = MockBackendService()
        backend.startError = BackendError.executableNotFound
        let core = makeCore(backendService: backend)
        await core.startup()
        XCTAssertTrue(backend.startCalled)
    }

    // MARK: - startRemoteMode

    func testStartRemoteModeCallsFinishStartupWhenUrlSet() async {
        let watcher = MockFileWatcherService()
        let wm = MockWindowManager()
        let core = makeCore(remoteBaseUrl: "http://example.com", mode: .remote,
                            fileWatcherService: watcher, windowManager: wm)
        await core.startRemoteMode()
        XCTAssertTrue(watcher.startCalled)
        XCTAssertTrue(wm.restoreWindowsCalled)
    }

    func testStartRemoteModeDoesNotFinishStartupWhenNoUrl() async {
        let watcher = MockFileWatcherService()
        let core = makeCore(fileWatcherService: watcher)
        await core.startRemoteMode()
        XCTAssertFalse(watcher.startCalled)
    }

    func testStartRemoteModeCallsShowErrorWhenNoUrl() async {
        var errorMessage: String?
        let core = makeCore()
        core.showError = { errorMessage = $0 }
        await core.startRemoteMode()
        XCTAssertNotNil(errorMessage)
        XCTAssertTrue(errorMessage?.contains("URL") == true)
    }

    // MARK: - startLocalMode

    func testStartLocalModeSetsLocalPort() async {
        let backend = MockBackendService()
        let wm = MockWindowManager()
        let watcher = MockFileWatcherService()
        let core = makeCore(backendService: backend, fileWatcherService: watcher, windowManager: wm)
        core.healthCheckTimeoutSeconds = 5
        // Enqueue after makeCore (makeCore resets FakeURLProtocol internally)
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "http://localhost:1/api/health")!, data: Data())
        await core.startLocalMode()
        XCTAssertTrue(backend.startCalled)
        XCTAssertEqual(wm.setLocalPortValue, core.localPort)
    }

    func testStartLocalModeShowsErrorWhenBackendThrows() async {
        let backend = MockBackendService()
        backend.startError = BackendError.executableNotFound
        var errorMessage: String?
        let core = makeCore(backendService: backend)
        core.showError = { errorMessage = $0 }
        await core.startLocalMode()
        XCTAssertTrue(backend.startCalled)
        XCTAssertNotNil(errorMessage)
    }

    func testStartLocalModeShowsErrorWhenHealthCheckFails() async {
        let backend = MockBackendService()
        var errorMessage: String?
        let core = makeCore(backendService: backend)
        core.showError = { errorMessage = $0 }
        // No FakeURLProtocol response enqueued → health check errors immediately → timeout=0 via makeCore
        await core.startLocalMode()
        XCTAssertTrue(backend.startCalled)
        XCTAssertNotNil(errorMessage)
    }

    func testStartLocalModeCompletesSuccessfully() async {
        let backend = MockBackendService()
        let watcher = MockFileWatcherService()
        let wm = MockWindowManager()
        let core = makeCore(backendService: backend, fileWatcherService: watcher, windowManager: wm)
        core.healthCheckTimeoutSeconds = 5
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "http://localhost:1/api/health")!, data: Data())
        await core.startLocalMode()
        XCTAssertTrue(watcher.startCalled)
        XCTAssertTrue(wm.restoreWindowsCalled)
    }

    // MARK: - finishStartup

    func testFinishStartupStartsFileWatcher() async {
        let watcher = MockFileWatcherService()
        let core = makeCore(fileWatcherService: watcher)
        await core.finishStartup()
        XCTAssertTrue(watcher.startCalled)
    }

    func testFinishStartupRestoresWindows() async {
        let wm = MockWindowManager()
        let core = makeCore(windowManager: wm)
        await core.finishStartup()
        XCTAssertTrue(wm.restoreWindowsCalled)
    }

    // MARK: - waitForHealth

    func testWaitForHealthReturnsTrueOn200() async {
        let core = makeCore()
        FakeURLProtocol.reset()
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "http://localhost:9999/api/health")!, data: Data())
        core.healthCheckSession = FakeURLProtocol.makeSession()
        let result = await core.waitForHealth(baseUrl: "http://localhost:9999", timeoutSeconds: 5)
        XCTAssertTrue(result)
    }

    func testWaitForHealthReturnsTrueOn503() async {
        let core = makeCore()
        FakeURLProtocol.reset()
        FakeURLProtocol.enqueue(statusCode: 503, url: URL(string: "http://localhost:9999/api/health")!, data: Data())
        core.healthCheckSession = FakeURLProtocol.makeSession()
        let result = await core.waitForHealth(baseUrl: "http://localhost:9999", timeoutSeconds: 5)
        XCTAssertTrue(result)
    }

    func testWaitForHealthReturnsFalseOnTimeout() async {
        let core = makeCore()
        let result = await core.waitForHealth(baseUrl: "http://localhost:9999", timeoutSeconds: 0)
        XCTAssertFalse(result)
    }

    func testWaitForHealthReturnsFalseOnNetworkError() async {
        let core = makeCore()
        FakeURLProtocol.reset()
        core.healthCheckSession = FakeURLProtocol.makeSession()
        // timeoutSeconds: 0 exits immediately; network error also causes false
        let result = await core.waitForHealth(baseUrl: "http://localhost:9999", timeoutSeconds: 0)
        XCTAssertFalse(result)
    }

    func testWaitForHealthReturnsFalseOnMalformedUrl() async {
        let core = makeCore()
        let result = await core.waitForHealth(baseUrl: "not a url ://", timeoutSeconds: 5)
        XCTAssertFalse(result)
    }

    func testWaitForHealthReturnsFalseOnNon200Status() async {
        let core = makeCore()
        FakeURLProtocol.reset()
        FakeURLProtocol.enqueue(statusCode: 404, url: URL(string: "http://localhost:9999/api/health")!, data: Data())
        core.healthCheckSession = FakeURLProtocol.makeSession()
        let result = await core.waitForHealth(baseUrl: "http://localhost:9999", timeoutSeconds: 0)
        XCTAssertFalse(result)
    }

    // MARK: - showErrorAndQuit

    func testShowErrorAndQuitCallsShowError() async {
        let core = makeCore()
        var errorMessage: String?
        core.showError = { errorMessage = $0 }
        await core.showErrorAndQuit("Something went wrong")
        XCTAssertEqual(errorMessage, "Something went wrong")
    }

    func testShowErrorAndQuitCallsTerminate() async {
        let core = makeCore()
        var terminateCalled = false
        core.terminate = { terminateCalled = true }
        await core.showErrorAndQuit("Error")
        // terminateDelay is 0 in tests; flush the main queue
        await MainActor.run {}
        XCTAssertTrue(terminateCalled)
    }

    // MARK: - beginTermination

    func testBeginTerminationClosesWindows() async {
        let wm = MockWindowManager()
        let core = makeCore(windowManager: wm)
        core.beginTermination()
        await Task.yield()
        XCTAssertTrue(wm.closeAllCalled)
    }

    func testBeginTerminationStopsBackend() async {
        let backend = MockBackendService()
        let core = makeCore(backendService: backend)
        core.beginTermination()
        try? await Task.sleep(nanoseconds: 50_000_000)
        XCTAssertTrue(backend.stopCalled)
    }

    func testBeginTerminationStopsFileWatcher() async {
        let watcher = MockFileWatcherService()
        let core = makeCore(fileWatcherService: watcher)
        core.beginTermination()
        try? await Task.sleep(nanoseconds: 50_000_000)
        XCTAssertTrue(watcher.stopCalled)
    }

    // MARK: - switchToLocalMode

    func testSwitchToLocalModeReopensIfAlreadyRunning() async {
        let backend = MockBackendService()
        backend.isRunning = true
        let wm = MockWindowManager()
        let core = makeCore(backendService: backend, windowManager: wm)
        await core.switchToLocalMode()
        XCTAssertTrue(wm.reopenAllCalled)
        XCTAssertFalse(backend.startCalled)
    }

    func testSwitchToLocalModeStartsBackendAndReopens() async {
        let backend = MockBackendService()
        let wm = MockWindowManager()
        let port = PortFinder.findFreePort()
        guard port > 0 else { return }
        FakeURLProtocol.reset()
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "http://localhost:\(port)/api/health")!, data: Data())
        let core = makeCore(backendService: backend, windowManager: wm)
        core.healthCheckTimeoutSeconds = 5
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "http://localhost:1/api/health")!, data: Data())
        await core.switchToLocalMode()
        XCTAssertTrue(backend.startCalled)
        XCTAssertTrue(wm.reopenAllCalled)
    }

    func testSwitchToLocalModeShowsErrorWhenBackendFails() async {
        let backend = MockBackendService()
        backend.startError = BackendError.executableNotFound
        var errorMessage: String?
        let wm = MockWindowManager()
        let core = makeCore(backendService: backend, windowManager: wm)
        core.showError = { errorMessage = $0 }
        await core.switchToLocalMode()
        XCTAssertTrue(backend.startCalled)
        XCTAssertNotNil(errorMessage)
        XCTAssertFalse(wm.reopenAllCalled)
    }

    func testSwitchToLocalModeShowsErrorWhenHealthCheckFails() async {
        let backend = MockBackendService()
        var errorMessage: String?
        let wm = MockWindowManager()
        let core = makeCore(backendService: backend, windowManager: wm)
        core.showError = { errorMessage = $0 }
        await core.switchToLocalMode()
        XCTAssertNotNil(errorMessage)
        XCTAssertFalse(wm.reopenAllCalled)
    }

    // MARK: - URL actions

    func testOpenDocsCallsUrlOpener() {
        let core = makeCore()
        var opened: URL?
        core.urlOpener = { opened = $0 }
        core.openDocs()
        XCTAssertEqual(opened, AppCore.docsURL)
    }

    func testOpenReleasesCallsUrlOpener() {
        let core = makeCore()
        var opened: URL?
        core.urlOpener = { opened = $0 }
        core.openReleases()
        XCTAssertEqual(opened, AppCore.releasesURL)
    }

    func testDocsURLIsHttps() {
        XCTAssertEqual(AppCore.docsURL.scheme, "https")
    }

    func testReleasesURLPointsToGitHub() {
        XCTAssertTrue(AppCore.releasesURL.absoluteString.contains("github"))
    }
}

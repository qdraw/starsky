import XCTest


final class BackendServiceTests: XCTestCase {
    private static let localhostUrl = "http://localhost"
    private var tempDir: URL!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    func testStopOnUnstartedServiceDoesNotCrash() {
        let service = BackendService(fileLogger: DailyFileLogger())
        service.stop()
    }

    func testDeinitOnUnstartedServiceDoesNotCrash() {
        var service: BackendService? = BackendService(fileLogger: DailyFileLogger())
        service = nil
        XCTAssertNil(service)
    }

    func testEnvironmentContainsAspNetCoreUrls() {
        let env = BackendService.buildEnvironment(port: 5432)
        XCTAssertEqual(env["ASPNETCORE_URLS"], "\(Self.localhostUrl):5432")
    }

    func testEnvironmentContainsAllRequiredKeys() {
        let env = BackendService.buildEnvironment(port: 5000)
        let requiredKeys = [
            "ASPNETCORE_URLS",
            "app__appsettingspath",
            "app__appsettingslocalpath",
            "app__databaseConnection",
            "app__tempFolder",
            "app__thumbnailTempFolder",
            "app__NoAccountLocalhost",
            "app__UseLocalDesktop",
            "app__AccountRegisterDefaultRole",
            "app__ThumbnailGenerationIntervalInMinutes",
            "app__Verbose"
        ]
        for key in requiredKeys {
            XCTAssertNotNil(env[key], "Missing env var: \(key)")
        }
    }

    func testFindBackendExeReturnsUrlOrNil() throws {
        let service = BackendService(fileLogger: DailyFileLogger())
        let result = service.findBackendExe()
        // Result depends on whether the runtime was copied at build time; both outcomes are valid
        if let url = result {
            XCTAssertTrue(FileManager.default.fileExists(atPath: url.path))
            XCTAssertEqual(url.lastPathComponent, "starsky")
        }
    }

    func testNoAccountLocalhostIsTrue() {
        let env = BackendService.buildEnvironment(port: 5000)
        XCTAssertEqual(env["app__NoAccountLocalhost"], "true")
    }

    func testIsRunningReturnsFalseWhenNotStarted() {
        let service = BackendService(fileLogger: DailyFileLogger())
        XCTAssertFalse(service.isRunning)
    }

    func testStartThrowsWhenExecutableMissing() {
        let service = BackendService(fileLogger: DailyFileLogger())
        // findBackendExe() returns nil in the test environment (no app bundle runtime dir)
        if service.findBackendExe() == nil {
            XCTAssertThrowsError(try service.start(port: 19990)) { error in
                XCTAssertTrue(error is BackendError)
                XCTAssertEqual((error as? BackendError)?.errorDescription, "The Starsky backend executable was not found in the application bundle.")
            }
        }
    }

    func testStartAndStopWithFakeProcess() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtime")
        try FakeStarskyBin.create(in: runtimeDir)

        let service = TestableBackendService(
            fileLogger: DailyFileLogger(),
            xattrPath: "/usr/bin/true",
            codesignPath: "/usr/bin/true"
        )
        service.fakeExeURL = runtimeDir.appendingPathComponent("starsky")

        try service.start(port: 19991)
        XCTAssertTrue(service.isRunning)
        service.stop()
        XCTAssertFalse(service.isRunning)
    }

    func testClearQuarantineWithBogusToolsDoesNotCrash() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtime2")
        try FakeStarskyBin.create(in: runtimeDir)

        let service = TestableBackendService(
            fileLogger: DailyFileLogger(),
            xattrPath: "/nonexistent/xattr",
            codesignPath: "/nonexistent/codesign"
        )
        service.fakeExeURL = runtimeDir.appendingPathComponent("starsky")

        XCTAssertNoThrow(try service.start(port: 19992))
        service.stop()
    }

    func testStopOnAlreadyStoppedServiceAfterStartDoesNotCrash() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtime3")
        try FakeStarskyBin.create(in: runtimeDir)

        let service = TestableBackendService(
            fileLogger: DailyFileLogger(),
            xattrPath: "/usr/bin/true",
            codesignPath: "/usr/bin/true"
        )
        service.fakeExeURL = runtimeDir.appendingPathComponent("starsky")

        try service.start(port: 19993)
        service.stop()
        service.stop()
    }

    func testUnexpectedExitTriggersRestart() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtimeRestart")
        try FileManager.default.createDirectory(at: runtimeDir, withIntermediateDirectories: true)
        let binary = runtimeDir.appendingPathComponent("starsky")
        try "#!/bin/sh\nexit 0\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger(), xattrPath: "/usr/bin/true", codesignPath: "/usr/bin/true")
        service.fakeExeURL = binary
        service.restartDelay = 0.1

        try service.start(port: 19996)
        Thread.sleep(forTimeInterval: 0.5)

        XCTAssertGreaterThanOrEqual(service.launchCount, 2, "Backend should have been relaunched after unexpected exit")
    }

    func testRestartIsBlockedWhenShuttingDown() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtimeNoRestart")
        try FileManager.default.createDirectory(at: runtimeDir, withIntermediateDirectories: true)
        let binary = runtimeDir.appendingPathComponent("starsky")
        try "#!/bin/sh\nexit 0\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger(), xattrPath: "/usr/bin/true", codesignPath: "/usr/bin/true")
        service.fakeExeURL = binary
        service.restartDelay = 0.3

        try service.start(port: 19997)
        Thread.sleep(forTimeInterval: 0.1)  // let the crash be detected
        service.stop()                        // mark isShuttingDown before restart fires
        Thread.sleep(forTimeInterval: 0.5)  // wait past the restart deadline

        XCTAssertEqual(service.launchCount, 1, "Restart should have been blocked after stop()")
    }

    func testStopSendsInterruptWhenProcessIgnoresTerm() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtime4")
        try FileManager.default.createDirectory(at: runtimeDir, withIntermediateDirectories: true)
        let binary = runtimeDir.appendingPathComponent("starsky")
        let readyMarker = runtimeDir.appendingPathComponent("ready")
        // ready marker guarantees the trap is installed before stop() sends SIGTERM;
        // while loop prevents exec-optimisation of the last command, keeping the shell
        // as the tracked PID so it genuinely ignores SIGTERM via the trap.
        try "#!/bin/sh\ntrap '' TERM\ntouch '\(readyMarker.path)'\nwhile true; do sleep 0.05; done\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger(), xattrPath: "/usr/bin/true", codesignPath: "/usr/bin/true")
        service.fakeExeURL = binary
        service.sigtermTimeout = 0.1

        try service.start(port: 19994)
        XCTAssertTrue(service.isRunning)
        try waitForReadyMarker(readyMarker)
        service.stop()
        XCTAssertFalse(service.isRunning)
    }

    func testStopSendsKillWhenProcessIgnoresTermAndInt() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtime5")
        try FileManager.default.createDirectory(at: runtimeDir, withIntermediateDirectories: true)
        let binary = runtimeDir.appendingPathComponent("starsky")
        let readyMarker = runtimeDir.appendingPathComponent("ready")
        // ready marker guarantees both traps are installed before stop() sends SIGTERM/SIGINT.
        try "#!/bin/sh\ntrap '' TERM\ntrap '' INT\ntouch '\(readyMarker.path)'\nwhile true; do sleep 0.05; done\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger(), xattrPath: "/usr/bin/true", codesignPath: "/usr/bin/true")
        service.fakeExeURL = binary
        service.sigtermTimeout = 0.1
        service.sigintTimeout = 0.1

        try service.start(port: 19995)
        XCTAssertTrue(service.isRunning)
        try waitForReadyMarker(readyMarker)
        service.stop()
        XCTAssertFalse(service.isRunning)
    }

    func testBeginShutdownSendsTermAndSetsShuttingDown() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtimeBeginShutdown")
        try FileManager.default.createDirectory(at: runtimeDir, withIntermediateDirectories: true)
        let binary = runtimeDir.appendingPathComponent("starsky")
        let readyMarker = runtimeDir.appendingPathComponent("ready")
        try "#!/bin/sh\ntouch '\(readyMarker.path)'\nwhile true; do sleep 0.05; done\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger(), xattrPath: "/usr/bin/true", codesignPath: "/usr/bin/true")
        service.fakeExeURL = binary

        try service.start(port: 19998)
        XCTAssertTrue(service.isRunning)
        try waitForReadyMarker(readyMarker)

        service.beginShutdown()
        // Process should exit from SIGTERM; wait briefly
        let deadline = Date().addingTimeInterval(1.0)
        while service.isRunning && Date() < deadline { Thread.sleep(forTimeInterval: 0.05) }
        XCTAssertFalse(service.isRunning)
    }

    func testForceStopKillsRunningProcess() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtimeForceStop")
        try FileManager.default.createDirectory(at: runtimeDir, withIntermediateDirectories: true)
        let binary = runtimeDir.appendingPathComponent("starsky")
        let readyMarker = runtimeDir.appendingPathComponent("ready")
        // Ignores SIGTERM so only SIGKILL (from forceStop) will end it
        try "#!/bin/sh\ntrap '' TERM\ntouch '\(readyMarker.path)'\nwhile true; do sleep 0.05; done\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger(), xattrPath: "/usr/bin/true", codesignPath: "/usr/bin/true")
        service.fakeExeURL = binary

        try service.start(port: 19999)
        XCTAssertTrue(service.isRunning)
        try waitForReadyMarker(readyMarker)

        service.beginShutdown()               // SIGTERM, ignored by trap
        Thread.sleep(forTimeInterval: 0.1)   // confirm it's still alive
        XCTAssertTrue(service.isRunning)

        service.forceStop()                   // SIGKILL
        XCTAssertFalse(service.isRunning)
    }

    func testClearQuarantineNotCalledWhenBackendStaysRunning() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtimeStable")
        try FakeStarskyBin.create(in: runtimeDir)

        let service = TestableBackendService(fileLogger: DailyFileLogger())
        service.fakeExeURL = runtimeDir.appendingPathComponent("starsky")

        try service.start(port: 20001)
        Thread.sleep(forTimeInterval: 0.1)
        service.stop()

        XCTAssertEqual(service.quarantineClearCount, 0,
                       "clearQuarantine should not be called when the backend stays running")
    }

    func testClearQuarantineCalledOnFirstUnexpectedExit() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtimeCrash")
        try FileManager.default.createDirectory(at: runtimeDir, withIntermediateDirectories: true)
        let binary = runtimeDir.appendingPathComponent("starsky")
        try "#!/bin/sh\nexit 0\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger())
        service.fakeExeURL = binary
        service.restartDelay = 0.1

        try service.start(port: 20002)
        Thread.sleep(forTimeInterval: 0.5)
        service.stop()

        XCTAssertGreaterThanOrEqual(service.launchCount, 2,
            "Restart should have fired (confirms onProcessExited ran)")
        XCTAssertEqual(service.quarantineClearCount, 1,
                       "clearQuarantine should be called exactly once after the first unexpected exit")
    }

    private func waitForReadyMarker(_ marker: URL, timeout: TimeInterval = 2) throws {
        let deadline = Date().addingTimeInterval(timeout)
        while !FileManager.default.fileExists(atPath: marker.path) {
            XCTAssertTrue(Date() < deadline, "Process never signalled readiness at \(marker.path)")
            Thread.sleep(forTimeInterval: 0.02)
        }
    }
}

private class TestableBackendService: BackendService {
    var fakeExeURL: URL?
    private(set) var launchCount = 0
    private(set) var quarantineClearCount = 0

    override func findBackendExe() -> URL? {
        launchCount += 1
        return fakeExeURL
    }

    override func quarantineDidClear(path: String) {
        quarantineClearCount += 1
    }
}

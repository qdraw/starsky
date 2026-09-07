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
        // while loop prevents exec-optimisation of the last command, keeping the shell
        // as the tracked PID so it genuinely ignores SIGTERM via the trap.
        try "#!/bin/sh\ntrap '' TERM\nwhile true; do sleep 0.05; done\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger(), xattrPath: "/usr/bin/true", codesignPath: "/usr/bin/true")
        service.fakeExeURL = binary
        service.sigtermTimeout = 0.1

        try service.start(port: 19994)
        XCTAssertTrue(service.isRunning)
        service.stop()
        XCTAssertFalse(service.isRunning)
    }

    func testStopSendsKillWhenProcessIgnoresTermAndInt() throws {
        let runtimeDir = tempDir.appendingPathComponent("runtime5")
        try FileManager.default.createDirectory(at: runtimeDir, withIntermediateDirectories: true)
        let binary = runtimeDir.appendingPathComponent("starsky")
        try "#!/bin/sh\ntrap '' TERM\ntrap '' INT\nwhile true; do sleep 0.05; done\n".write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes([.posixPermissions: 0o755], ofItemAtPath: binary.path)

        let service = TestableBackendService(fileLogger: DailyFileLogger(), xattrPath: "/usr/bin/true", codesignPath: "/usr/bin/true")
        service.fakeExeURL = binary
        service.sigtermTimeout = 0.1
        service.sigintTimeout = 0.1

        try service.start(port: 19995)
        XCTAssertTrue(service.isRunning)
        service.stop()
        XCTAssertFalse(service.isRunning)
    }
}

private class TestableBackendService: BackendService {
    var fakeExeURL: URL?
    private(set) var launchCount = 0
    override func findBackendExe() -> URL? {
        launchCount += 1
        return fakeExeURL
    }
}

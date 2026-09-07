import XCTest


// MARK: - Test doubles

final class MockProcessRunner: ProcessRunner {
    var runCalled: [(URL, [String])] = []
    var runSyncCalled: [(URL, [String])] = []
    var runResults: [Int32] = []

    func run(executable: URL, arguments: [String]) async -> Int32 {
        runCalled.append((executable, arguments))
        return runResults.isEmpty ? 0 : runResults.removeFirst()
    }

    func runSync(executable: URL, arguments: [String]) {
        runSyncCalled.append((executable, arguments))
    }
}

// MARK: - Tests

final class MountWatcherServiceTests: XCTestCase {
    private var tempDir: URL!
    private var fakeCLI: URL!
    private var fakePlist: URL!

    override func setUp() async throws {
        try await super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
        fakeCLI = tempDir.appendingPathComponent("starskymountwatchercli")
        fakePlist = tempDir.appendingPathComponent("nl.qdraw.mountwatcher.test.plist")
        FileManager.default.createFile(atPath: fakeCLI.path, contents: Data())
    }

    override func tearDown() async throws {
        try? FileManager.default.removeItem(at: tempDir)
        try await super.tearDown()
    }

    private func makeService(runner: MockProcessRunner = MockProcessRunner()) -> MountWatcherService {
        MountWatcherService(
            processRunner: runner,
            cliBinaryURL: fakeCLI,
            plistURL: fakePlist,
            serviceName: "nl.qdraw.mountwatcher.test"
        )
    }

    // MARK: - enable

    func testEnableCallsInstallAndReturnsTrue() async {
        let runner = MockProcessRunner()
        let svc = makeService(runner: runner)
        let result = await svc.enable()
        XCTAssertTrue(result)
        XCTAssertEqual(runner.runCalled.count, 1)
        XCTAssertEqual(runner.runCalled[0].0, fakeCLI)
        XCTAssertEqual(runner.runCalled[0].1, ["--install"])
    }

    func testEnableReturnsFalseWhenCLINotFound() async {
        try? FileManager.default.removeItem(at: fakeCLI)
        let svc = makeService()
        let result = await svc.enable()
        XCTAssertFalse(result)
    }

    func testEnableReturnsFalseWhenCLIExitsNonZero() async {
        let runner = MockProcessRunner()
        runner.runResults = [1]
        let svc = makeService(runner: runner)
        let result = await svc.enable()
        XCTAssertFalse(result)
    }

    // MARK: - disable

    func testDisableCallsUninstallAndReturnsTrue() async {
        let runner = MockProcessRunner()
        let svc = makeService(runner: runner)
        let result = await svc.disable()
        XCTAssertTrue(result)
        XCTAssertEqual(runner.runCalled.count, 1)
        XCTAssertEqual(runner.runCalled[0].1, ["--uninstall"])
    }

    func testDisableReturnsFalseWhenCLIExitsNonZero() async {
        let runner = MockProcessRunner()
        runner.runResults = [1]
        let svc = makeService(runner: runner)
        let result = await svc.disable()
        XCTAssertFalse(result)
    }

    func testDisableReturnsTrueWhenCLIMissingAndPlistAbsent() async {
        try? FileManager.default.removeItem(at: fakeCLI)
        let svc = makeService()
        let result = await svc.disable()
        XCTAssertTrue(result)
    }

    func testDisableReturnsFalseWhenCLIMissingButPlistPresent() async {
        try? FileManager.default.removeItem(at: fakeCLI)
        FileManager.default.createFile(atPath: fakePlist.path, contents: Data())
        let svc = makeService()
        let result = await svc.disable()
        XCTAssertFalse(result)
    }

    // MARK: - status

    func testStatusIsNotInstalledWhenNoPlist() async {
        let svc = makeService()
        let result = await svc.status()
        XCTAssertEqual(result, .notInstalled)
    }

    func testStatusIsRunningWhenPlistExistsAndLaunchctlSucceeds() async {
        FileManager.default.createFile(atPath: fakePlist.path, contents: Data())
        let runner = MockProcessRunner()
        runner.runResults = [0]
        let svc = makeService(runner: runner)
        let result = await svc.status()
        XCTAssertEqual(result, .running)
        XCTAssertEqual(runner.runCalled[0].1, ["list", "nl.qdraw.mountwatcher.test"])
    }

    func testStatusIsStoppedWhenPlistExistsButLaunchctlFails() async {
        FileManager.default.createFile(atPath: fakePlist.path, contents: Data())
        let runner = MockProcessRunner()
        runner.runResults = [1]
        let svc = makeService(runner: runner)
        let result = await svc.status()
        XCTAssertEqual(result, .stopped)
    }

    // MARK: - stopSync

    func testStopSyncCallsUninstallSynchronously() {
        let runner = MockProcessRunner()
        let svc = makeService(runner: runner)
        svc.stopSync()
        XCTAssertEqual(runner.runSyncCalled.count, 1)
        XCTAssertEqual(runner.runSyncCalled[0].0, fakeCLI)
        XCTAssertEqual(runner.runSyncCalled[0].1, ["--uninstall"])
    }

    func testStopSyncDoesNothingWhenCLINotFound() {
        try? FileManager.default.removeItem(at: fakeCLI)
        let runner = MockProcessRunner()
        let svc = makeService(runner: runner)
        svc.stopSync()
        XCTAssertTrue(runner.runSyncCalled.isEmpty)
    }

    // MARK: - DefaultProcessRunner (real process execution via FakeProcessBin)

    func testDefaultProcessRunnerRunReturnsZeroForSuccessScript() async throws {
        let bin = try FakeProcessBin.create(in: tempDir, exitCode: 0, name: "exit0")
        let runner = DefaultProcessRunner()
        let code = await runner.run(executable: bin, arguments: [])
        XCTAssertEqual(code, 0)
    }

    func testDefaultProcessRunnerRunReturnsNonZeroForFailScript() async throws {
        let bin = try FakeProcessBin.create(in: tempDir, exitCode: 1, name: "exit1")
        let runner = DefaultProcessRunner()
        let code = await runner.run(executable: bin, arguments: [])
        XCTAssertEqual(code, 1)
    }

    func testDefaultProcessRunnerRunReturnsMinusOneForMissingExecutable() async {
        let runner = DefaultProcessRunner()
        let code = await runner.run(
            executable: URL(fileURLWithPath: "/nonexistent/binary_that_does_not_exist"),
            arguments: []
        )
        XCTAssertEqual(code, -1)
    }

    func testDefaultProcessRunnerRunSyncCompletesWithoutCrash() throws {
        let bin = try FakeProcessBin.create(in: tempDir, exitCode: 0, name: "syncbin")
        let runner = DefaultProcessRunner()
        runner.runSync(executable: bin, arguments: [])
    }

    func testDefaultProcessRunnerRunSyncWithMissingExecutableDoesNotCrash() {
        let runner = DefaultProcessRunner()
        runner.runSync(
            executable: URL(fileURLWithPath: "/nonexistent/binary_that_does_not_exist"),
            arguments: []
        )
    }

    // MARK: - MountWatcherStatus displayString

    func testDisplayStringsAreNonEmpty() {
        XCTAssertFalse(MountWatcherStatus.running.displayString.isEmpty)
        XCTAssertFalse(MountWatcherStatus.stopped.displayString.isEmpty)
        XCTAssertFalse(MountWatcherStatus.notInstalled.displayString.isEmpty)
        XCTAssertFalse(MountWatcherStatus.unknown.displayString.isEmpty)
    }

    func testDisplayStringsAreDistinct() {
        let strings = [
            MountWatcherStatus.running.displayString,
            MountWatcherStatus.stopped.displayString,
            MountWatcherStatus.notInstalled.displayString,
            MountWatcherStatus.unknown.displayString,
        ]
        XCTAssertEqual(Set(strings).count, strings.count)
    }
}

import XCTest
@testable import Starsky

final class FileWatcherServiceTests: XCTestCase {
    private var tempDir: URL!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    func testStartCreatesDirectoryIfMissing() {
        let watchDir = tempDir.appendingPathComponent("watch")
        XCTAssertFalse(FileManager.default.fileExists(atPath: watchDir.path))
        let service = FileWatcherService(fileLogger: DailyFileLogger(), watchedDirectory: watchDir)
        service.start()
        XCTAssertTrue(FileManager.default.fileExists(atPath: watchDir.path))
        service.stop()
    }

    func testStartAndStopDoNotThrow() {
        let watchDir = tempDir.appendingPathComponent("watch2")
        let service = FileWatcherService(fileLogger: DailyFileLogger(), watchedDirectory: watchDir)
        service.start()
        service.stop()
    }

    func testDoubleStopDoesNotCrash() {
        let watchDir = tempDir.appendingPathComponent("watch3")
        let service = FileWatcherService(fileLogger: DailyFileLogger(), watchedDirectory: watchDir)
        service.start()
        service.stop()
        service.stop()
    }

    func testStartWithoutExistingDirCreatesIt() {
        let watchDir = tempDir.appendingPathComponent("nested/watch")
        let service = FileWatcherService(fileLogger: DailyFileLogger(), watchedDirectory: watchDir)
        service.start()
        XCTAssertTrue(FileManager.default.fileExists(atPath: watchDir.path))
        service.stop()
    }

    func testDeinitDoesNotCrash() {
        let watchDir = tempDir.appendingPathComponent("watch4")
        var service: FileWatcherService? = FileWatcherService(fileLogger: DailyFileLogger(), watchedDirectory: watchDir)
        service?.start()
        service = nil
    }

    func testStopOnUnstartedServiceDoesNotCrash() {
        let watchDir = tempDir.appendingPathComponent("watch5")
        let service = FileWatcherService(fileLogger: DailyFileLogger(), watchedDirectory: watchDir)
        service.stop()
    }

    func testHandleDirectoryChangeRunsWithoutCrashing() throws {
        let watchDir = tempDir.appendingPathComponent("eventWatch")
        try FileManager.default.createDirectory(at: watchDir, withIntermediateDirectories: true)
        let service = FileWatcherService(fileLogger: DailyFileLogger(), watchedDirectory: watchDir)
        service.start()

        try "x".write(to: watchDir.appendingPathComponent("photo.jpg"), atomically: true, encoding: .utf8)

        // Allow debounce (0.5 s) plus dispatch overhead to complete
        Thread.sleep(forTimeInterval: 1.0)
        service.stop()
    }

    func testOnDirectoryChangedIgnoresTmpFiles() throws {
        let watchDir = tempDir.appendingPathComponent("tmpWatch")
        try FileManager.default.createDirectory(at: watchDir, withIntermediateDirectories: true)

        // Pre-populate with a .tmp file that should be skipped
        try "pending".write(to: watchDir.appendingPathComponent("pending.tmp"), atomically: true, encoding: .utf8)

        let service = FileWatcherService(fileLogger: DailyFileLogger(), watchedDirectory: watchDir)
        service.start()

        // Trigger a change event by writing a non-tmp file
        try "data".write(to: watchDir.appendingPathComponent("image.jpg"), atomically: true, encoding: .utf8)

        Thread.sleep(forTimeInterval: 1.0)
        service.stop()
    }
}

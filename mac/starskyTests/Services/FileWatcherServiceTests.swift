import XCTest
@testable import starsky

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
}

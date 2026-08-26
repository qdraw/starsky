import XCTest
@testable import starsky

final class ApplicationPathsTests: XCTestCase {
    func testAppSupportContainsStarsky() {
        XCTAssertTrue(ApplicationPaths.appSupport.path.contains("starsky"))
    }

    func testCachesContainsStarsky() {
        XCTAssertTrue(ApplicationPaths.caches.path.contains("starsky"))
    }

    func testSettingsFileUnderAppSupport() {
        XCTAssertTrue(ApplicationPaths.settingsFile.path.hasPrefix(ApplicationPaths.appSupport.path))
        XCTAssertEqual(ApplicationPaths.settingsFile.lastPathComponent, "settings.json")
    }

    func testLogsDirectoryUnderAppSupport() {
        XCTAssertTrue(ApplicationPaths.logsDirectory.path.hasPrefix(ApplicationPaths.appSupport.path))
        XCTAssertEqual(ApplicationPaths.logsDirectory.lastPathComponent, "logs")
    }

    func testTempFolderUnderCaches() {
        XCTAssertTrue(ApplicationPaths.tempFolder.path.hasPrefix(ApplicationPaths.caches.path))
        XCTAssertEqual(ApplicationPaths.tempFolder.lastPathComponent, "tempFolder")
    }
}

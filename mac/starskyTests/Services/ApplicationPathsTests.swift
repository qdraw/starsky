import XCTest


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

    func testAppSettingsFileUnderAppSupport() {
        XCTAssertTrue(ApplicationPaths.appSettingsFile.path.hasPrefix(ApplicationPaths.appSupport.path))
        XCTAssertEqual(ApplicationPaths.appSettingsFile.lastPathComponent, "appsettings.json")
    }

    func testAppSettingsLocalFileUnderAppSupport() {
        XCTAssertTrue(ApplicationPaths.appSettingsLocalFile.path.hasPrefix(ApplicationPaths.appSupport.path))
        XCTAssertEqual(ApplicationPaths.appSettingsLocalFile.lastPathComponent, "appsettings.local.json")
    }

    func testDatabaseFileUnderAppSupport() {
        XCTAssertTrue(ApplicationPaths.databaseFile.path.hasPrefix(ApplicationPaths.appSupport.path))
        XCTAssertEqual(ApplicationPaths.databaseFile.lastPathComponent, "starsky.db")
    }

    func testThumbnailTempFolderUnderAppSupport() {
        XCTAssertTrue(ApplicationPaths.thumbnailTempFolder.path.hasPrefix(ApplicationPaths.appSupport.path))
        XCTAssertEqual(ApplicationPaths.thumbnailTempFolder.lastPathComponent, "thumbnailTempFolder")
    }

    func testRuntimeDirectoryUnderBundleContents() {
        let runtimeDir = ApplicationPaths.runtimeDirectory
        XCTAssertTrue(runtimeDir.path.contains("Contents/MacOS"))
        #if arch(arm64)
        XCTAssertEqual(runtimeDir.lastPathComponent, "runtime-starsky-osx-arm64")
        #else
        XCTAssertEqual(runtimeDir.lastPathComponent, "runtime-starsky-osx-x64")
        #endif
    }

    func testEnsureDirectoriesCreatesRequiredDirs() throws {
        try ApplicationPaths.ensureDirectories()
        let fm = FileManager.default
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.appSupport.path))
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.caches.path))
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.logsDirectory.path))
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.thumbnailTempFolder.path))
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.tempFolder.path))
    }
}

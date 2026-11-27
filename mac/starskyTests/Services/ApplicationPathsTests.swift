import XCTest

final class ApplicationPathsTests: XCTestCase {
    func testAppSupportContainsStarsky() {
        XCTAssertTrue(ApplicationPaths.appSupport.path.contains("starsky"))
    }

    func testGroupContainerContainsStarsky() {
        // In test builds the entitlement isn't provisioned, so groupContainer falls
        // back to appSupport — either way the path must contain "starsky".
        let path = ApplicationPaths.groupContainer.path
        XCTAssertTrue(path.contains("starsky") || path.contains("group.nl.qdraw.starsky"))
    }

    func testSettingsFileUnderAppSupport() {
        XCTAssertTrue(ApplicationPaths.settingsFile.path.hasPrefix(ApplicationPaths.appSupport.path))
        #if DEBUG
        XCTAssertEqual(ApplicationPaths.settingsFile.lastPathComponent, "settings-debug.json")
        #else
        XCTAssertEqual(ApplicationPaths.settingsFile.lastPathComponent, "settings.json")
        #endif
    }

    func testLogsDirectoryUnderGroupContainer() {
        XCTAssertTrue(ApplicationPaths.logsDirectory.path.hasPrefix(ApplicationPaths.groupContainer.path))
        XCTAssertEqual(ApplicationPaths.logsDirectory.lastPathComponent, "logs")
    }

    func testTempFolderUnderGroupContainer() {
        XCTAssertTrue(ApplicationPaths.tempFolder.path.hasPrefix(ApplicationPaths.groupContainer.path))
        XCTAssertEqual(ApplicationPaths.tempFolder.lastPathComponent, "tmp")
    }

    func testAppSettingsFileUnderGroupContainer() {
        XCTAssertTrue(ApplicationPaths.appSettingsFile.path.hasPrefix(ApplicationPaths.groupContainer.path))
        XCTAssertEqual(ApplicationPaths.appSettingsFile.lastPathComponent, "appsettings.json")
    }

    func testAppSettingsLocalFileUnderGroupContainer() {
        XCTAssertTrue(ApplicationPaths.appSettingsLocalFile.path.hasPrefix(ApplicationPaths.groupContainer.path))
        XCTAssertEqual(ApplicationPaths.appSettingsLocalFile.lastPathComponent, "appsettings.local.json")
    }

    func testDatabaseFileUnderGroupContainer() {
        XCTAssertTrue(ApplicationPaths.databaseFile.path.hasPrefix(ApplicationPaths.groupContainer.path))
        XCTAssertEqual(ApplicationPaths.databaseFile.lastPathComponent, "starsky.db")
    }

    func testThumbnailTempFolderUnderGroupContainer() {
        XCTAssertTrue(ApplicationPaths.thumbnailTempFolder.path.hasPrefix(ApplicationPaths.groupContainer.path))
        XCTAssertEqual(ApplicationPaths.thumbnailTempFolder.lastPathComponent, "thumbnailTempFolder")
    }

    func testBookmarksDirectoryUnderGroupContainer() {
        XCTAssertTrue(ApplicationPaths.bookmarksDirectory.path.hasPrefix(ApplicationPaths.groupContainer.path))
        XCTAssertEqual(ApplicationPaths.bookmarksDirectory.lastPathComponent, "bookmarks")
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
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.groupContainer.path))
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.logsDirectory.path))
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.thumbnailTempFolder.path))
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.tempFolder.path))
        XCTAssertTrue(fm.fileExists(atPath: ApplicationPaths.bookmarksDirectory.path))
    }
}

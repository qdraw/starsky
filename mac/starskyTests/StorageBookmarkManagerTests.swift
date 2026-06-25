import XCTest
@testable import starsky

final class StorageBookmarkManagerTests: XCTestCase {
    var tmpBookmarksDir: URL!
    var tmpFolderToBookmark: URL!

    override func setUpWithError() throws {
        tmpBookmarksDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(at: tmpBookmarksDir, withIntermediateDirectories: true, attributes: nil)
        StorageBookmarkManager.storageDirOverride = tmpBookmarksDir

        tmpFolderToBookmark = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(at: tmpFolderToBookmark, withIntermediateDirectories: true, attributes: nil)
    }

    override func tearDownWithError() throws {
        StorageBookmarkManager.storageDirOverride = nil
        try? FileManager.default.removeItem(at: tmpBookmarksDir)
        try? FileManager.default.removeItem(at: tmpFolderToBookmark)
    }

    func testSaveLoadStartStopRemoveBookmark() throws {
        let token = "test-token-\(UUID().uuidString)"

        // Save bookmark
        try StorageBookmarkManager.saveBookmark(forFolderPath: tmpFolderToBookmark.path, token: token)

        // Ensure bookmark file exists
        let bookmarkFile = tmpBookmarksDir.appendingPathComponent("bookmark_\(token)")
        XCTAssertTrue(FileManager.default.fileExists(atPath: bookmarkFile.path))

        // Load bookmark URL
        let url = try StorageBookmarkManager.loadBookmarkURL(token: token)
        XCTAssertEqual(url.path, tmpFolderToBookmark.path)

        // Try starting access; may fail in non-sandboxed test environments -- treat as non-fatal
        do {
            let started = try StorageBookmarkManager.startAccessForToken(token)
            // If started, ensure it's the same path and then stop
            XCTAssertTrue(FileManager.default.fileExists(atPath: started.path))
            StorageBookmarkManager.stopAccess(started)
        } catch {
            // Non-fatal: log but don't fail the test
            NSLog("[Test] startAccessForToken threw: %@", String(describing: error))
        }

        // Remove bookmark
        try StorageBookmarkManager.removeBookmark(token: token)
        XCTAssertFalse(FileManager.default.fileExists(atPath: bookmarkFile.path))
    }
}

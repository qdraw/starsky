import XCTest
@testable import starsky

final class AppSettingsReaderTests: XCTestCase {
    override func setUpWithError() throws {
        AppSettingsReader.storageFileOverride = nil
    }

    override func tearDownWithError() throws {
        AppSettingsReader.storageFileOverride = nil
    }

    func writeJSON(_ obj: Any, to url: URL) throws {
        let data = try JSONSerialization.data(withJSONObject: obj, options: [])
        try data.write(to: url)
    }

    func testValidStorageFolderAndToken() throws {
        let tmpDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(at: tmpDir, withIntermediateDirectories: true, attributes: nil)
        defer { try? FileManager.default.removeItem(at: tmpDir) }

        let appsettings: [String: Any] = [
            "app": [
                "StorageFolder": tmpDir.path,
                "StorageFolderToken": "token-123"
            ]
        ]

        let file = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        try writeJSON(appsettings, to: file)
        AppSettingsReader.storageFileOverride = file.path

        let result = AppSettingsReader.getStorageFolderAndTokenFromAppSettings()
        XCTAssertEqual(result.storageFolder, tmpDir.path)
        XCTAssertEqual(result.storageFolderToken, "token-123")
    }

    func testTildeExpansionAndValidation() throws {
        // Create a real directory at ~/tmp_test_starsky
        let home = NSHomeDirectory()
        let dir = URL(fileURLWithPath: home).appendingPathComponent("tmp_test_starsky_\(UUID().uuidString)")
        try FileManager.default.createDirectory(at: dir, withIntermediateDirectories: true, attributes: nil)
        defer { try? FileManager.default.removeItem(at: dir) }

        let tildePath = "~\(dir.path.replacingOccurrences(of: home, with: ""))"

        let appsettings: [String: Any] = [
            "app": [
                "StorageFolder": tildePath,
                "StorageFolderToken": "tok"
            ]
        ]

        let file = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        try writeJSON(appsettings, to: file)
        AppSettingsReader.storageFileOverride = file.path

        let result = AppSettingsReader.getStorageFolderAndTokenFromAppSettings()
        XCTAssertEqual(result.storageFolder, dir.path)
        XCTAssertEqual(result.storageFolderToken, "tok")
    }

    func testMissingFolderReturnsNilFolderButTokenStillRead() throws {
        let appsettings: [String: Any] = [
            "app": [
                "StorageFolder": "/path/that/does/not/exist/\(UUID().uuidString)",
                "StorageFolderToken": "token-only"
            ]
        ]
        let file = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        try writeJSON(appsettings, to: file)
        AppSettingsReader.storageFileOverride = file.path

        let result = AppSettingsReader.getStorageFolderAndTokenFromAppSettings()
        XCTAssertNil(result.storageFolder)
        XCTAssertEqual(result.storageFolderToken, "token-only")
    }

    func testNoAppSectionReturnsNil() throws {
        let obj: [String: Any] = ["notapp": ["StorageFolder": "/tmp"]]
        let file = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        try writeJSON(obj, to: file)
        AppSettingsReader.storageFileOverride = file.path

        let result = AppSettingsReader.getStorageFolderAndTokenFromAppSettings()
        XCTAssertNil(result.storageFolder)
        XCTAssertNil(result.storageFolderToken)
    }
}

import XCTest

final class AppSettingsReaderTests: XCTestCase {
    private var tempDir: URL!
    private var mainFile: URL!
    private var localFile: URL!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try? FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
        mainFile  = tempDir.appendingPathComponent("appsettings.json")
        localFile = tempDir.appendingPathComponent("appsettings.local.json")
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    // MARK: - Missing / corrupt files

    func testMissingBothFilesReturnsNil() {
        XCTAssertNil(AppSettingsReader.read(mainFile: mainFile, localFile: localFile))
    }

    func testCorruptMainFileReturnsNil() throws {
        try "not json {{{".data(using: .utf8)!.write(to: mainFile)
        XCTAssertNil(AppSettingsReader.read(mainFile: mainFile, localFile: localFile))
    }

    func testEmptyStorageFolderReturnsNil() throws {
        try write(to: mainFile, storageFolder: "")
        XCTAssertNil(AppSettingsReader.read(mainFile: mainFile, localFile: localFile))
    }

    // MARK: - Basic reading

    func testReadsStorageFolderFromMainFile() throws {
        try write(to: mainFile, storageFolder: "/photos/")
        let result = AppSettingsReader.read(mainFile: mainFile, localFile: localFile)
        XCTAssertEqual(result?.storageFolder, "/photos/")
    }

    func testReadsStorageFolderMappingsFromMainFile() throws {
        try write(to: mainFile, storageFolder: "/photos/", mappings: ["/archive": "/mnt/archive"])
        let result = AppSettingsReader.read(mainFile: mainFile, localFile: localFile)
        XCTAssertEqual(result?.storageFolderMappings, ["/archive": "/mnt/archive"])
    }

    func testMissingMappingsKeyDefaultsToEmptyDictionary() throws {
        try write(to: mainFile, storageFolder: "/photos/")
        let result = AppSettingsReader.read(mainFile: mainFile, localFile: localFile)
        XCTAssertEqual(result?.storageFolderMappings, [:])
    }

    // MARK: - Local file override

    func testLocalFileStorageFolderOverridesMain() throws {
        try write(to: mainFile,  storageFolder: "/main/")
        try write(to: localFile, storageFolder: "/local/")
        let result = AppSettingsReader.read(mainFile: mainFile, localFile: localFile)
        XCTAssertEqual(result?.storageFolder, "/local/")
    }

    func testLocalFileMappingsOverrideMain() throws {
        try write(to: mainFile,  storageFolder: "/photos/", mappings: ["/a": "/main-a"])
        try write(to: localFile, storageFolder: "/photos/", mappings: ["/a": "/local-a"])
        let result = AppSettingsReader.read(mainFile: mainFile, localFile: localFile)
        XCTAssertEqual(result?.storageFolderMappings["/a"], "/local-a")
    }

    func testMissingLocalFileFallsBackToMain() throws {
        try write(to: mainFile, storageFolder: "/photos/")
        // localFile does not exist
        let result = AppSettingsReader.read(mainFile: mainFile, localFile: localFile)
        XCTAssertEqual(result?.storageFolder, "/photos/")
    }

    func testCorruptLocalFileFallsBackToMain() throws {
        try write(to: mainFile, storageFolder: "/photos/")
        try "bad json".data(using: .utf8)!.write(to: localFile)
        let result = AppSettingsReader.read(mainFile: mainFile, localFile: localFile)
        XCTAssertEqual(result?.storageFolder, "/photos/")
    }

    // MARK: - Helpers

    private func write(to url: URL, storageFolder: String, mappings: [String: String] = [:]) throws {
        var app: [String: Any] = ["StorageFolder": storageFolder]
        if !mappings.isEmpty { app["StorageFolderMappings"] = mappings }
        let data = try JSONSerialization.data(withJSONObject: ["app": app])
        try data.write(to: url)
    }
}

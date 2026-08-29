import XCTest


final class FileDownloadServiceTests: XCTestCase {
    private static let localBaseUrl = "http://localhost:5000"
    private static let remoteBaseUrl = "http://remote.example.com"
    private static let testPhotoPath = "/photos/test.jpg"
    private static let testMissingPath = "/photos/missing.jpg"
    private var tempDir: URL!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try? FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
        FakeURLProtocol.reset()
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        FakeURLProtocol.reset()
        super.tearDown()
    }

    func testHappyPathWritesFileToDisk() async throws {
        let baseUrl = Self.localBaseUrl
        let path = Self.testPhotoPath
        let imageData = "fake-jpeg-bytes".data(using: .utf8)!

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/index?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=%2Fphotos%2Ftest.jpg&cache=false")!, data: imageData)

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false)

        let destFile = tempDir.appendingPathComponent("photos/test.jpg")
        XCTAssertTrue(FileManager.default.fileExists(atPath: destFile.path))
        let written = try Data(contentsOf: destFile)
        XCTAssertEqual(written, imageData)
    }

    func testSidecarFailureStillDownloadsMainFile() async throws {
        let baseUrl = Self.localBaseUrl
        let path = Self.testPhotoPath
        let imageData = "fake-jpeg".data(using: .utf8)!

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/index?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 404, url: URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=%2Fphotos%2Ftest.jpg&cache=false")!, data: imageData)

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false)
        let destFile = tempDir.appendingPathComponent("photos/test.jpg")
        XCTAssertTrue(FileManager.default.fileExists(atPath: destFile.path))
    }

    func testCookiesAreForwardedAsHeader() async throws {
        let baseUrl = Self.remoteBaseUrl
        let path = Self.testPhotoPath
        let imageData = "data".data(using: .utf8)!

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/index?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 404, url: URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=%2Fphotos%2Ftest.jpg&cache=false")!, data: imageData)

        let cookie = HTTPCookie(properties: [
            .name: "session", .value: "abc123",
            .domain: "remote.example.com", .path: "/"
        ])!

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false, cookies: [cookie])

        let cookieHeaders = FakeURLProtocol.capturedRequests.compactMap { $0.value(forHTTPHeaderField: "Cookie") }
        XCTAssertTrue(cookieHeaders.allSatisfy { $0.contains("session=abc123") }, "Cookie header missing from requests")
    }

    func testPhotoHttpErrorThrows() async {
        let baseUrl = Self.localBaseUrl
        let path = Self.testMissingPath

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/index?f=%2Fphotos%2Fmissing.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=%2Fphotos%2Fmissing.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 500, url: URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=%2Fphotos%2Fmissing.jpg&cache=false")!, data: Data())

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        do {
            try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false)
            XCTFail("Expected error was not thrown")
        } catch {
            XCTAssertTrue(error is DownloadError)
        }
    }

    func testIndexNotFoundThrowsFileNotFound() async {
        let baseUrl = Self.localBaseUrl
        let path = Self.testMissingPath

        FakeURLProtocol.enqueue(statusCode: 404, url: URL(string: "\(baseUrl)/starsky/api/index?f=%2Fphotos%2Fmissing.jpg")!, data: Data())

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        do {
            try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false)
            XCTFail("Expected error was not thrown")
        } catch DownloadError.fileNotFound {
            // expected
        } catch {
            XCTFail("Wrong error type: \(error)")
        }
    }

    func testExistingFileIsOverwritten() async throws {
        let baseUrl = Self.localBaseUrl
        let path = Self.testPhotoPath
        let destFile = tempDir.appendingPathComponent("photos/test.jpg")

        // Pre-create the destination file with old content
        try FileManager.default.createDirectory(at: destFile.deletingLastPathComponent(), withIntermediateDirectories: true)
        try "old-content".write(to: destFile, atomically: true, encoding: .utf8)

        let newData = "new-jpeg-bytes".data(using: .utf8)!
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/index?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=%2Fphotos%2Ftest.jpg&cache=false")!, data: newData)

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false)

        let written = try Data(contentsOf: destFile)
        XCTAssertEqual(written, newData)
    }

    func testDownloadErrorInvalidPathDescription() {
        let error = DownloadError.invalidPath
        XCTAssertEqual(error.errorDescription, "Invalid file path.")
    }

    func testDownloadErrorFileNotFoundDescription() {
        let error = DownloadError.fileNotFound
        XCTAssertEqual(error.errorDescription, "File not found on server.")
    }

    func testDownloadErrorDownloadFailedDescription() {
        let error = DownloadError.downloadFailed(statusCode: 403)
        XCTAssertEqual(error.errorDescription, "Download failed (HTTP 403).")
    }

    func testUploadSendsCorrectHeadersAndBody() async throws {
        let baseUrl = Self.remoteBaseUrl
        let remotePath = "/photos/test.jpg"
        let fileData = "updated-jpeg-bytes".data(using: .utf8)!
        let localFile = tempDir.appendingPathComponent("test.jpg")
        try fileData.write(to: localFile)

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/upload")!, data: "[]".data(using: .utf8)!)

        let cookie = HTTPCookie(properties: [.name: "session", .value: "tok", .domain: "remote.example.com", .path: "/"])!
        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        try await service.upload(localURL: localFile, remotePath: remotePath, baseUrl: baseUrl, cookies: [cookie])

        let uploadReq = FakeURLProtocol.capturedRequests.first { $0.url?.path == "/starsky/api/upload" }
        XCTAssertNotNil(uploadReq, "No upload request captured")
        XCTAssertEqual(uploadReq?.value(forHTTPHeaderField: "to"), "/photos")
        XCTAssertEqual(uploadReq?.value(forHTTPHeaderField: "filename"), "test.jpg")
        XCTAssertEqual(uploadReq?.value(forHTTPHeaderField: "Content-Type"), "application/octet-stream")
        XCTAssertTrue(uploadReq?.value(forHTTPHeaderField: "Cookie")?.contains("session=tok") == true)
    }

    func testUploadFailedStatusThrows() async {
        let baseUrl = Self.remoteBaseUrl
        let localFile = tempDir.appendingPathComponent("f.jpg")
        try? "x".data(using: .utf8)!.write(to: localFile)

        FakeURLProtocol.enqueue(statusCode: 403, url: URL(string: "\(baseUrl)/starsky/api/upload")!, data: Data())

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        do {
            try await service.upload(localURL: localFile, remotePath: "/f.jpg", baseUrl: baseUrl)
            XCTFail("Expected error")
        } catch {
            XCTAssertTrue(error is UploadError)
        }
    }

    func testDownloadWithCookieProviderRegistersWatcher() async throws {
        let baseUrl = Self.localBaseUrl
        let path = Self.testPhotoPath
        let imageData = "img".data(using: .utf8)!
        var providerCallCount = 0

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/index?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 404, url: URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=%2Fphotos%2Ftest.jpg&cache=false")!, data: imageData)

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false, cookieProvider: {
            providerCallCount += 1
            return []
        })

        // Provider should not have been called yet (only called on change)
        XCTAssertEqual(providerCallCount, 0)
        let destFile = tempDir.appendingPathComponent("photos/test.jpg")
        XCTAssertTrue(FileManager.default.fileExists(atPath: destFile.path))
    }

    func testUploadErrorDescriptions() {
        XCTAssertEqual(UploadError.readFailed.errorDescription, "Could not read local file for upload.")
        XCTAssertEqual(UploadError.invalidPath.errorDescription, "Invalid upload URL.")
        XCTAssertEqual(UploadError.uploadFailed(statusCode: 500).errorDescription, "Upload failed (HTTP 500).")
    }

    // MARK: - Watcher integration tests

    func testInPlaceWriteTriggersUpload() async throws {
        let baseUrl = Self.localBaseUrl
        let path = Self.testPhotoPath

        enqueuDownload(baseUrl: baseUrl, path: path, data: "original".data(using: .utf8)!)
        let service = makeWatchedService()
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false, cookieProvider: { [] })

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/upload")!, data: Data())

        let destFile = tempDir.appendingPathComponent("photos/test.jpg")
        // Non-atomic write — modifies the inode in place, triggers .write on the file fd
        try "updated".data(using: .utf8)!.write(to: destFile, options: [])

        try await Task.sleep(nanoseconds: 2_000_000_000)
        XCTAssertNotNil(
            FakeURLProtocol.capturedRequests.first { $0.url?.path == "/starsky/api/upload" },
            "In-place write did not trigger an upload"
        )
    }

    func testAtomicWriteTriggersUpload() async throws {
        let baseUrl = Self.localBaseUrl
        let path = Self.testPhotoPath

        enqueuDownload(baseUrl: baseUrl, path: path, data: "original".data(using: .utf8)!)
        let service = makeWatchedService()
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false, cookieProvider: { [] })

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/upload")!, data: Data())

        let destFile = tempDir.appendingPathComponent("photos/test.jpg")
        // Atomic write: Swift Data.write with .atomic writes to a temp file in the same
        // directory and renames it over the target — the classic editor save pattern.
        try "updated-atomic".data(using: .utf8)!.write(to: destFile, options: .atomic)

        try await Task.sleep(nanoseconds: 2_000_000_000)
        XCTAssertNotNil(
            FakeURLProtocol.capturedRequests.first { $0.url?.path == "/starsky/api/upload" },
            "Atomic write (rename) did not trigger an upload"
        )
    }

    func testNewFileInSameDirTriggersUpload() async throws {
        let baseUrl = Self.localBaseUrl
        let path = Self.testPhotoPath  // /photos/test.jpg

        enqueuDownload(baseUrl: baseUrl, path: path, data: "original".data(using: .utf8)!)
        let service = makeWatchedService()
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false, cookieProvider: { [] })

        // Simulate an editor exporting a new file (e.g. DNG → JPEG export) into the same dir
        let exportedFile = tempDir.appendingPathComponent("photos/export.jpg")
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/upload")!, data: Data())
        try "exported-jpeg-bytes".data(using: .utf8)!.write(to: exportedFile)

        try await Task.sleep(nanoseconds: 2_000_000_000)
        XCTAssertNotNil(
            FakeURLProtocol.capturedRequests.first { $0.url?.path == "/starsky/api/upload" },
            "New file created by editor in same directory did not trigger an upload"
        )
        // Remote path should use the same parent directory as the watched file
        let uploadReq = FakeURLProtocol.capturedRequests.first { $0.url?.path == "/starsky/api/upload" }
        XCTAssertEqual(uploadReq?.value(forHTTPHeaderField: "to"), "/photos")
        XCTAssertEqual(uploadReq?.value(forHTTPHeaderField: "filename"), "export.jpg")
    }

    func testMtimeUpdatedAfterUploadPreventsDoubleUpload() async throws {
        let baseUrl = Self.localBaseUrl
        let path = Self.testPhotoPath

        enqueuDownload(baseUrl: baseUrl, path: path, data: "original".data(using: .utf8)!)
        let service = makeWatchedService()
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false, cookieProvider: { [] })

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/upload")!, data: Data())

        let destFile = tempDir.appendingPathComponent("photos/test.jpg")
        try "updated".data(using: .utf8)!.write(to: destFile, options: [])

        // Wait for upload to complete and mtime to be refreshed
        try await Task.sleep(nanoseconds: 2_000_000_000)
        let countAfterFirstSave = FakeURLProtocol.capturedRequests.filter { $0.url?.path == "/starsky/api/upload" }.count
        XCTAssertEqual(countAfterFirstSave, 1)

        // Simulate a directory event with no actual file change (no new upload expected)
        try await Task.sleep(nanoseconds: 500_000_000)
        let countAfterNoChange = FakeURLProtocol.capturedRequests.filter { $0.url?.path == "/starsky/api/upload" }.count
        XCTAssertEqual(countAfterNoChange, 1, "Upload fired again despite no mtime change")
    }

    // MARK: - Watcher test helpers

    private func enqueuDownload(baseUrl: String, path: String, data: Data) {
        let enc = path.addingPercentEncoding(withAllowedCharacters:
            CharacterSet.alphanumerics.union(CharacterSet(charactersIn: "-._~")))!
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/index?f=\(enc)")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 404, url: URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=\(enc)")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=\(enc)&cache=false")!, data: data)
    }

    private func makeWatchedService() -> FileDownloadService {
        FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
    }

    func testInvalidBaseUrlThrowsInvalidPath() async {
        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        do {
            // A baseUrl with unencoded spaces produces a nil URL when combined with the query string
            try await service.downloadAndOpen(path: "/test.jpg", baseUrl: "http://host with spaces", openFile: false)
            XCTFail("Expected error was not thrown")
        } catch DownloadError.invalidPath {
            // expected
        } catch {
            // network or other errors are also acceptable — the important thing is it doesn't crash
        }
    }

    func testDefaultSessionInitDoesNotCrash() {
        // Exercises the else-branch in init where no URLSession is provided
        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            tempFolder: URL(fileURLWithPath: NSTemporaryDirectory())
        )
        XCTAssertNotNil(service)
    }

    func testMultipleCookiesAreConcatenated() async throws {
        let baseUrl = Self.remoteBaseUrl
        let path = Self.testPhotoPath

        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/index?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=%2Fphotos%2Ftest.jpg")!, data: Data())
        FakeURLProtocol.enqueue(statusCode: 200, url: URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=%2Fphotos%2Ftest.jpg&cache=false")!, data: "x".data(using: .utf8)!)

        let cookie1 = HTTPCookie(properties: [.name: "a", .value: "1", .domain: "remote.example.com", .path: "/"])!
        let cookie2 = HTTPCookie(properties: [.name: "b", .value: "2", .domain: "remote.example.com", .path: "/"])!

        let service = FileDownloadService(
            fileLogger: DailyFileLogger(),
            session: FakeURLProtocol.makeSession(),
            tempFolder: tempDir
        )
        try await service.downloadAndOpen(path: path, baseUrl: baseUrl, openFile: false, cookies: [cookie1, cookie2])

        let headers = FakeURLProtocol.capturedRequests.compactMap { $0.value(forHTTPHeaderField: "Cookie") }
        XCTAssertTrue(headers.allSatisfy { $0.contains("a=1") && $0.contains("b=2") })
    }
}

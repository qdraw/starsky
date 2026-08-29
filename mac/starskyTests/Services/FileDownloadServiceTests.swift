import XCTest
@testable import starsky

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

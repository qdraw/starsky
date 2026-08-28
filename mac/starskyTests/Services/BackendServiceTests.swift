import XCTest
@testable import starsky

final class BackendServiceTests: XCTestCase {
    private static let localhostUrl = "http://localhost"
    private var tempDir: URL!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    func testStopOnUnstartedServiceDoesNotCrash() {
        let service = BackendService(fileLogger: DailyFileLogger())
        service.stop()
    }

    func testDeinitOnUnstartedServiceDoesNotCrash() {
        var service: BackendService? = BackendService(fileLogger: DailyFileLogger())
        service = nil
        XCTAssertNil(service)
    }

    func testEnvironmentContainsAspNetCoreUrls() {
        let env = BackendService.buildEnvironment(port: 5432)
        XCTAssertEqual(env["ASPNETCORE_URLS"], "\(Self.localhostUrl):5432")
    }

    func testEnvironmentContainsAllRequiredKeys() {
        let env = BackendService.buildEnvironment(port: 5000)
        let requiredKeys = [
            "ASPNETCORE_URLS",
            "app__appsettingspath",
            "app__appsettingslocalpath",
            "app__databaseConnection",
            "app__tempFolder",
            "app__thumbnailTempFolder",
            "app__NoAccountLocalhost",
            "app__UseLocalDesktop",
            "app__AccountRegisterDefaultRole",
            "app__ThumbnailGenerationIntervalInMinutes",
            "app__Verbose"
        ]
        for key in requiredKeys {
            XCTAssertNotNil(env[key], "Missing env var: \(key)")
        }
    }

    func testFindBackendExeReturnsUrlOrNil() throws {
        let service = BackendService(fileLogger: DailyFileLogger())
        let result = service.findBackendExe()
        // Result depends on whether the runtime was copied at build time; both outcomes are valid
        if let url = result {
            XCTAssertTrue(FileManager.default.fileExists(atPath: url.path))
            XCTAssertEqual(url.lastPathComponent, "starsky")
        }
    }

    func testNoAccountLocalhostIsTrue() {
        let env = BackendService.buildEnvironment(port: 5000)
        XCTAssertEqual(env["app__NoAccountLocalhost"], "true")
    }
}

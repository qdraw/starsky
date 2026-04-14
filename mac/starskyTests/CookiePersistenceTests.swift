import XCTest
import WebKit
@testable import starsky

final class CookiePersistenceTests: XCTestCase {
    override func setUpWithError() throws {
        // ensure override is nil before each test
        CookiePersistence.storageURLOverride = nil
    }

    override func tearDownWithError() throws {
        CookiePersistence.storageURLOverride = nil
    }

    func testSaveAndLoadCookies_roundtrip() throws {
        let tempFile = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        CookiePersistence.storageURLOverride = tempFile
        defer {
            try? FileManager.default.removeItem(at: tempFile)
            CookiePersistence.storageURLOverride = nil
        }

        // Create cookie
        let expires = Date().addingTimeInterval(3600)
        let props: [HTTPCookiePropertyKey: Any] = [
            .name: "testCookie",
            .value: "hello",
            .domain: "localhost",
            .path: "/",
            .expires: expires
        ]
        guard let cookie = HTTPCookie(properties: props) else {
            XCTFail("Failed to create cookie")
            return
        }

        try CookiePersistence.saveCookiesArray([cookie])

        let loaded = try CookiePersistence.loadCookiesArray()
        XCTAssertEqual(loaded.count, 1)
        let lc = loaded[0]
        XCTAssertEqual(lc.name, "testCookie")
        XCTAssertEqual(lc.value, "hello")
        XCTAssertEqual(lc.domain, "localhost")
        XCTAssertEqual(lc.path, "/")
        // expires may differ slightly due to serialization -> allow small delta
        if let d = lc.expiresDate {
            XCTAssertEqual(d.timeIntervalSince1970, expires.timeIntervalSince1970, accuracy: 1.0)
        } else {
            XCTFail("expiresDate missing")
        }
    }

    func testRestoreCookiesIntoWKHTTPCookieStore_integration() throws {
        let tempFile = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        CookiePersistence.storageURLOverride = tempFile
        defer {
            try? FileManager.default.removeItem(at: tempFile)
            CookiePersistence.storageURLOverride = nil
        }

        // Create cookie with a unique name so we can detect it later
        let cookieName = "starsky_test_cookie_\(UUID().uuidString)"
        let props: [HTTPCookiePropertyKey: Any] = [
            .name: cookieName,
            .value: "v1",
            .domain: "localhost",
            .path: "/"
        ]
        guard let cookie = HTTPCookie(properties: props) else {
            XCTFail("Failed to create cookie")
            return
        }

        try CookiePersistence.saveCookiesArray([cookie])

        let cookieStore = WKWebsiteDataStore.default().httpCookieStore

        let exp = expectation(description: "restore cookies into cookie store")

        // Remove any existing cookie with the same name first (best-effort)
        cookieStore.getAllCookies { existing in
            for c in existing where c.name == cookieName {
                // No direct delete API per cookie; to keep test simple, we will ignore existing ones
            }

            // Now call restore
            CookiePersistence.restoreCookies(into: cookieStore) {
                cookieStore.getAllCookies { all in
                    let found = all.first { $0.name == cookieName && $0.value == "v1" }
                    XCTAssertNotNil(found, "Restored cookie should exist in WKHTTPCookieStore")
                    exp.fulfill()
                }
            }
        }

        waitForExpectations(timeout: 5)
    }

    func testExpiredCookie_roundtrip() throws {
        let tempFile = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        CookiePersistence.storageURLOverride = tempFile
        defer {
            try? FileManager.default.removeItem(at: tempFile)
            CookiePersistence.storageURLOverride = nil
        }

        // Create an expired cookie (expiry in the past)
        let expires = Date().addingTimeInterval(-3600)
        let props: [HTTPCookiePropertyKey: Any] = [
            .name: "expiredCookie",
            .value: "x",
            .domain: "localhost",
            .path: "/",
            .expires: expires
        ]
        guard let cookie = HTTPCookie(properties: props) else { XCTFail("Failed to create cookie"); return }

        try CookiePersistence.saveCookiesArray([cookie])
        let loaded = try CookiePersistence.loadCookiesArray()
        XCTAssertEqual(loaded.count, 1)
        let lc = loaded[0]
        XCTAssertEqual(lc.name, "expiredCookie")
        // expiry preserved
        XCTAssertNotNil(lc.expiresDate)
        if let d = lc.expiresDate {
            XCTAssertEqual(d.timeIntervalSince1970, expires.timeIntervalSince1970, accuracy: 1.0)
        }
    }

    func testMultipleAndSpecialCharacterCookies_roundtrip() throws {
        let tempFile = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        CookiePersistence.storageURLOverride = tempFile
        defer {
            try? FileManager.default.removeItem(at: tempFile)
            CookiePersistence.storageURLOverride = nil
        }

        let cookie1Props: [HTTPCookiePropertyKey: Any] = [
            .name: "c1",
            .value: "v1",
            .domain: "localhost",
            .path: "/"
        ]
        let cookie2Props: [HTTPCookiePropertyKey: Any] = [
            .name: "special_©_name",
            .value: "päss=;:,\"<>",
            .domain: "localhost",
            .path: "/"
        ]
        guard let c1 = HTTPCookie(properties: cookie1Props), let c2 = HTTPCookie(properties: cookie2Props) else { XCTFail("Failed to create cookies"); return }

        try CookiePersistence.saveCookiesArray([c1, c2])
        let loaded = try CookiePersistence.loadCookiesArray()
        XCTAssertEqual(loaded.count, 2)
        // Find by name
        let found1 = loaded.first { $0.name == "c1" }
        let found2 = loaded.first { $0.name == "special_©_name" }
        XCTAssertNotNil(found1)
        XCTAssertNotNil(found2)
        XCTAssertEqual(found1?.value, "v1")
        XCTAssertEqual(found2?.value, "päss=;:,\"<>")
    }

    func testClearStoredCookies_removesFile() throws {
        let tempFile = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString).appendingPathExtension("json")
        CookiePersistence.storageURLOverride = tempFile
        defer {
            try? FileManager.default.removeItem(at: tempFile)
            CookiePersistence.storageURLOverride = nil
        }

        // create file
        let props: [HTTPCookiePropertyKey: Any] = [
            .name: "t1",
            .value: "v",
            .domain: "localhost",
            .path: "/"
        ]
        guard let cookie = HTTPCookie(properties: props) else { XCTFail("Failed to create cookie"); return }
        try CookiePersistence.saveCookiesArray([cookie])
        XCTAssertTrue(FileManager.default.fileExists(atPath: tempFile.path))

        try CookiePersistence.clearStoredCookies()
        XCTAssertFalse(FileManager.default.fileExists(atPath: tempFile.path))
    }
}

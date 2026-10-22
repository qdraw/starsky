import XCTest


final class ApplicationInfoTests: XCTestCase {

    func testVersionIsNonEmpty() {
        XCTAssertFalse(ApplicationInfo.version.isEmpty)
    }

    func testUserAgentSuffixContainsAppName() {
        XCTAssertTrue(ApplicationInfo.userAgentSuffix.hasPrefix("starsky/"))
    }

    func testUserAgentSuffixContainsVersion() {
        let suffix = ApplicationInfo.userAgentSuffix
        let version = ApplicationInfo.version
        XCTAssertTrue(suffix.contains(version))
    }

    func testUserAgentSuffixFormat() {
        let parts = ApplicationInfo.userAgentSuffix.split(separator: "/")
        XCTAssertEqual(parts.count, 2)
        XCTAssertEqual(String(parts[0]), "starsky")
    }

    func testVersionFallsBackWhenBundleMissing() {
        // In test bundles CFBundleShortVersionString may be absent; version must still be a non-empty string
        XCTAssertFalse(ApplicationInfo.version.isEmpty)
    }
}

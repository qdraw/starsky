import XCTest
@testable import Starsky
import Darwin

final class PortFinderTests: XCTestCase {
    func testReturnsPositivePort() {
        let port = PortFinder.findFreePort()
        XCTAssertGreaterThan(port, 0)
    }

    func testPortIsBindable() throws {
        let port = PortFinder.findFreePort()
        XCTAssertGreaterThan(port, 0)

        let sock = socket(AF_INET, SOCK_STREAM, 0)
        XCTAssertGreaterThanOrEqual(sock, 0)
        defer { close(sock) }

        var addr = sockaddr_in()
        addr.sin_family = sa_family_t(AF_INET)
        addr.sin_port = in_port_t(port).bigEndian
        addr.sin_addr.s_addr = INADDR_LOOPBACK.bigEndian

        let result = withUnsafePointer(to: &addr) {
            $0.withMemoryRebound(to: sockaddr.self, capacity: 1) {
                Darwin.bind(sock, $0, socklen_t(MemoryLayout<sockaddr_in>.size))
            }
        }
        XCTAssertEqual(result, 0, "Port \(port) should be bindable")
    }

    func testSuccessiveCalls() {
        let p1 = PortFinder.findFreePort()
        let p2 = PortFinder.findFreePort()
        XCTAssertGreaterThan(p1, 0)
        XCTAssertGreaterThan(p2, 0)
    }
}

import XCTest
@testable import starsky
import Darwin

private func htons(_ value: UInt16) -> UInt16 {
    return (value << 8) | (value >> 8)
}

final class GetFreePortTests: XCTestCase {
    func testPortIsNonNilAndInRange() throws {
        guard let port = getFreePort() else {
            XCTFail("getFreePort() returned nil")
            return
        }
        XCTAssertGreaterThan(port, 0, "Port should be > 0")
        XCTAssertLessThanOrEqual(port, 65535, "Port should be <= 65535")
    }

    func testPortIsBindable() throws {
        guard let port = getFreePort() else {
            XCTFail("getFreePort() returned nil")
            return
        }

        // Try to bind to the returned port to ensure it was free at the moment of discovery
        let fd = socket(AF_INET, SOCK_STREAM, 0)
        XCTAssertTrue(fd >= 0, "Failed to create socket: \(errno)")
        defer { if fd >= 0 { close(fd) } }

        var addr = sockaddr_in()
        addr.sin_len = __uint8_t(MemoryLayout<sockaddr_in>.size)
        addr.sin_family = sa_family_t(AF_INET)
        addr.sin_port = in_port_t(htons(UInt16(port)))
        addr.sin_addr = in_addr(s_addr: inet_addr("127.0.0.1"))

        let bindResult = withUnsafePointer(to: &addr) {
            $0.withMemoryRebound(to: sockaddr.self, capacity: 1) {
                Darwin.bind(fd, $0, socklen_t(MemoryLayout<sockaddr_in>.size))
            }
        }

        XCTAssertEqual(bindResult, 0, "Failed to bind to port \(port): errno=\(errno)")
    }

    func testRepeatedCallsAreBindable() throws {
        for _ in 0..<5 {
            guard let port = getFreePort() else {
                XCTFail("getFreePort() returned nil")
                return
            }

            let fd = socket(AF_INET, SOCK_STREAM, 0)
            XCTAssertTrue(fd >= 0, "Failed to create socket: \(errno)")
            defer { if fd >= 0 { close(fd) } }

            var addr = sockaddr_in()
            addr.sin_len = __uint8_t(MemoryLayout<sockaddr_in>.size)
            addr.sin_family = sa_family_t(AF_INET)
            addr.sin_port = in_port_t(htons(UInt16(port)))
            addr.sin_addr = in_addr(s_addr: inet_addr("127.0.0.1"))

            let bindResult = withUnsafePointer(to: &addr) {
                $0.withMemoryRebound(to: sockaddr.self, capacity: 1) {
                    Darwin.bind(fd, $0, socklen_t(MemoryLayout<sockaddr_in>.size))
                }
            }

            XCTAssertEqual(bindResult, 0, "Failed to bind to port \(port) on iteration: errno=\(errno)")

            // close socket to free the port for next iteration
            close(fd)
        }
    }
}


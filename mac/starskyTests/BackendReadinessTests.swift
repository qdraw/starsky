import XCTest
@testable import starsky

private func htons(_ value: UInt16) -> UInt16 {
    return (value << 8) | (value >> 8)
}

final class BackendReadinessTests: XCTestCase {

    // Start a minimal HTTP server that responds 200 to any request on loopback.
    private func startSimpleHTTPServer() throws -> (port: UInt16, stop: () -> Void) {
        let fd = socket(AF_INET, SOCK_STREAM, 0)
        if fd < 0 { throw NSError(domain: "socket", code: Int(errno), userInfo: nil) }

        var addr = sockaddr_in()
        addr.sin_len = __uint8_t(MemoryLayout<sockaddr_in>.size)
        addr.sin_family = sa_family_t(AF_INET)
        addr.sin_port = in_port_t(0).bigEndian // 0 means let OS choose
        addr.sin_addr = in_addr(s_addr: inet_addr("127.0.0.1"))

        let bindResult = withUnsafePointer(to: &addr) {
            $0.withMemoryRebound(to: sockaddr.self, capacity: 1) {
                Darwin.bind(fd, $0, socklen_t(MemoryLayout<sockaddr_in>.size))
            }
        }
        guard bindResult == 0 else { close(fd); throw NSError(domain: "bind", code: Int(errno), userInfo: nil) }

        // Get assigned port
        var len = socklen_t(MemoryLayout<sockaddr_in>.size)
        let getsocknameResult = withUnsafeMutablePointer(to: &addr) {
            $0.withMemoryRebound(to: sockaddr.self, capacity: 1) {
                getsockname(fd, $0, &len)
            }
        }
        guard getsocknameResult == 0 else { close(fd); throw NSError(domain: "getsockname", code: Int(errno), userInfo: nil) }

        let portNetwork = addr.sin_port
        let port = UInt16(bigEndian: UInt16(portNetwork))

        guard listen(fd, 10) == 0 else { close(fd); throw NSError(domain: "listen", code: Int(errno), userInfo: nil) }

        var shouldStop = false
        let q = DispatchQueue(label: "test.simple.http.server")
        q.async {
            while !shouldStop {
                var clientAddr = sockaddr_storage()
                var clientLen = socklen_t(MemoryLayout<sockaddr_storage>.size)
                let client = withUnsafeMutablePointer(to: &clientAddr) {
                    $0.withMemoryRebound(to: sockaddr.self, capacity: 1) { ptr in
                        accept(fd, ptr, &clientLen)
                    }
                }
                if client < 0 { continue }

                // Read request (not fully robust, but ok for tests)
                var buffer = [UInt8](repeating: 0, count: 1024)
                _ = read(client, &buffer, 1024)

                let response = "HTTP/1.1 200 OK\r\nContent-Length: 2\r\n\r\nOK"
                _ = response.withCString { ptr in
                    write(client, ptr, strlen(ptr))
                }
                close(client)
            }
            close(fd)
        }

        let stop = {
            shouldStop = true
            // make a dummy connection to unblock accept
            let s = socket(AF_INET, SOCK_STREAM, 0)
            if s >= 0 {
                var a = sockaddr_in()
                a.sin_len = __uint8_t(MemoryLayout<sockaddr_in>.size)
                a.sin_family = sa_family_t(AF_INET)
                a.sin_port = in_port_t(htons(port))
                a.sin_addr = in_addr(s_addr: inet_addr("127.0.0.1"))
                withUnsafePointer(to: &a) {
                    $0.withMemoryRebound(to: sockaddr.self, capacity: 1) {
                        _ = connect(s, $0, socklen_t(MemoryLayout<sockaddr_in>.size))
                    }
                }
                close(s)
            }
        }

        return (port, stop)
    }

    func testWaitUntilReadyDetectsServer() throws {
        let (port, stop) = try startSimpleHTTPServer()
        defer { stop() }

        let url = URL(string: "http://localhost:\(port)/")!
        let ready = waitUntilReady(localUrl: url, timeout: 5)
        XCTAssertTrue(ready, "waitUntilReady should have detected the server")
    }

}

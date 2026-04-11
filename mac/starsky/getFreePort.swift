import Foundation
import Darwin

func getFreePort() -> UInt16? {
    // Create a TCP socket
    let fd = socket(AF_INET, SOCK_STREAM, 0)
    if fd < 0 { return nil }

    // Ensure we close the socket before returning
    defer { close(fd) }

    // Prepare address structure (IPv4 loopback)
    var addr = sockaddr_in()
    addr.sin_len = __uint8_t(MemoryLayout<sockaddr_in>.size)
    addr.sin_family = sa_family_t(AF_INET)
    addr.sin_port = in_port_t(0) // port 0 => ask OS for an ephemeral port
    addr.sin_addr = in_addr(s_addr: inet_addr("127.0.0.1"))

    // Bind the socket to port 0
    let bindResult = withUnsafePointer(to: &addr) {
        $0.withMemoryRebound(to: sockaddr.self, capacity: 1) {
            bind(fd, $0, socklen_t(MemoryLayout<sockaddr_in>.size))
        }
    }
    if bindResult != 0 { return nil }

    // Retrieve the assigned port
    var len = socklen_t(MemoryLayout<sockaddr_in>.size)
    let getsocknameResult = withUnsafeMutablePointer(to: &addr) {
        $0.withMemoryRebound(to: sockaddr.self, capacity: 1) {
            getsockname(fd, $0, &len)
        }
    }
    if getsocknameResult != 0 { return nil }

    // sin_port is in network byte order (big-endian); convert to host order
    let networkPort = UInt16(addr.sin_port)
    let port = UInt16(bigEndian: networkPort)
    if port == 0 { return nil }
    return port
}

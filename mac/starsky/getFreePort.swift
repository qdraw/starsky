import Foundation
import Network

func getFreePort() -> UInt16? {
    let listener = try? NWListener(using: .tcp, on: 0)
    guard let listener = listener else { return nil }
    let port = listener.port?.rawValue
    listener.cancel()
    return port
}

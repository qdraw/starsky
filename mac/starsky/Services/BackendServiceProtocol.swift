import Foundation

// @unchecked Sendable: stop() is called from a background queue during termination by design.
protocol BackendServiceProtocol: AnyObject, Sendable {
    var isRunning: Bool { get }
    func start(port: Int) throws
    func stop()
}

extension BackendService: BackendServiceProtocol {}

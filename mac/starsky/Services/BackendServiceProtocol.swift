import Foundation

protocol BackendServiceProtocol: AnyObject {
    var isRunning: Bool { get }
    func start(port: Int) throws
    func stop()
}

extension BackendService: BackendServiceProtocol {}

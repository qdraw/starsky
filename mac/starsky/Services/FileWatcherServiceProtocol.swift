import Foundation

// @unchecked Sendable: stop() is called from a background queue during termination by design.
protocol FileWatcherServiceProtocol: AnyObject, Sendable {
    func start()
    func stop()
}

extension FileWatcherService: FileWatcherServiceProtocol {}

import Foundation

protocol FileWatcherServiceProtocol: AnyObject {
    func start()
    func stop()
}

extension FileWatcherService: FileWatcherServiceProtocol {}

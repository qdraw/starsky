import Foundation

enum MountWatcherStatus: Equatable {
    case running
    case stopped
    case notInstalled
    case unknown

    var displayString: String {
        switch self {
        case .running: return NSLocalizedString("menu.mountwatcher.status.running", comment: "")
        case .stopped: return NSLocalizedString("menu.mountwatcher.status.stopped", comment: "")
        case .notInstalled: return NSLocalizedString("menu.mountwatcher.status.notInstalled", comment: "")
        case .unknown: return NSLocalizedString("menu.mountwatcher.status.unknown", comment: "")
        }
    }
}

// @unchecked Sendable: stopSync() is documented safe to call from any thread during termination.
protocol MountWatcherServiceProtocol: AnyObject, Sendable {
    /// Install and start the MountWatcher launchd agent. Returns true on success.
    func enable() async -> Bool
    /// Stop and remove the MountWatcher launchd agent. Returns true on success.
    func disable() async -> Bool
    /// Query the actual service status from launchd.
    func status() async -> MountWatcherStatus
    /// Synchronously stop the service; safe to call from any thread during app termination.
    func stopSync()
}

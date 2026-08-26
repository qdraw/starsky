import Foundation

struct DesktopSettings: Codable {
    var mode: RuntimeMode = .local
    var remoteBaseUrl: String = ""
    var updateCheckEnabled: Bool = true
    var lastUpdateWarningShown: Date? = nil
    var windows: [SavedWindowState] = []
}

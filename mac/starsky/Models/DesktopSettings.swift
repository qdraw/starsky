import Foundation

struct DesktopSettings: Codable {
    var mode: RuntimeMode = .local
    var remoteBaseUrl: String = ""
    var updateCheckEnabled: Bool = true
    var preReleaseEnabled: Bool = false
    var lastUpdateWarningShown: Date? = nil
    var windows: [SavedWindowState] = []

    init() {}

    init(from decoder: Decoder) throws {
        let c = try decoder.container(keyedBy: CodingKeys.self)
        mode = try c.decode(RuntimeMode.self, forKey: .mode)
        remoteBaseUrl = try c.decode(String.self, forKey: .remoteBaseUrl)
        updateCheckEnabled = try c.decode(Bool.self, forKey: .updateCheckEnabled)
        preReleaseEnabled = try c.decodeIfPresent(Bool.self, forKey: .preReleaseEnabled) ?? false
        lastUpdateWarningShown = try c.decodeIfPresent(Date.self, forKey: .lastUpdateWarningShown)
        windows = try c.decode([SavedWindowState].self, forKey: .windows)
    }
}

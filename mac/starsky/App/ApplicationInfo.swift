import Foundation

enum ApplicationInfo {
    static let version: String = {
        Bundle.main.infoDictionary?["CFBundleShortVersionString"] as? String ?? "0.0.0"
    }()

    static let userAgentSuffix: String = "starsky/\(version)"
}

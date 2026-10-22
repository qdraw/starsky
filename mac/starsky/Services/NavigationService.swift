import Foundation

class NavigationService {
    private let settings: SettingsService

    init(settings: SettingsService) {
        self.settings = settings
    }

    func isAllowedOrigin(_ url: URL, baseUrl: String) -> Bool {
        guard let host = url.host,
              let base = URL(string: baseUrl),
              let baseHost = base.host else { return false }
        let sameScheme = url.scheme?.lowercased() == base.scheme?.lowercased()
        let sameHost = host.lowercased() == baseHost.lowercased()
        let samePort = url.port == base.port
        return sameScheme && sameHost && samePort
    }

    func buildStartUrl(baseUrl: String, route: String? = nil) -> String {
        let trimmed = baseUrl.hasSuffix("/") ? String(baseUrl.dropLast()) : baseUrl
        let r = route ?? "?f=/"
        let suffix = r.hasPrefix("/") || r.hasPrefix("?") ? r : "/\(r)"
        return trimmed + suffix
    }

    func getEffectiveBaseUrl(localPort: Int? = nil) -> String {
        switch settings.current.mode {
        case .local:
            let port = localPort ?? 0
            return "http://localhost:\(port)"
        case .remote:
            return settings.current.remoteBaseUrl
        }
    }
}

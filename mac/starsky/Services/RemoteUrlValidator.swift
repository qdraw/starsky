import Foundation

class RemoteUrlValidator {
    private static let healthPath = "/api/health"
    private let session: URLSession

    init(session: URLSession = .shared) {
        self.session = session
    }

    func validate(urlString: String) async -> UrlValidationResult {
        var trimmed = urlString.trimmingCharacters(in: .whitespacesAndNewlines)
        if trimmed.hasSuffix("/") { trimmed = String(trimmed.dropLast()) }

        guard !trimmed.isEmpty else {
            return UrlValidationResult(success: false, error: "URL cannot be empty.")
        }
        guard let url = URL(string: trimmed) else {
            return UrlValidationResult(success: false, error: "Invalid URL format.")
        }
        guard let scheme = url.scheme?.lowercased(), scheme == "http" || scheme == "https" else {
            return UrlValidationResult(success: false, error: "URL scheme must be http or https.")
        }

        guard let healthURL = URL(string: Self.healthPath, relativeTo: url)?.absoluteURL else {
            return UrlValidationResult(success: false, error: "Could not construct health URL.")
        }
        var request = URLRequest(url: healthURL, timeoutInterval: 10)
        request.httpMethod = "GET"

        do {
            let (_, response) = try await session.data(for: request)
            guard let http = response as? HTTPURLResponse else {
                return UrlValidationResult(success: false, error: "Unexpected response from server.")
            }
            if http.statusCode == 200 || http.statusCode == 503 {
                return UrlValidationResult(success: true, error: nil)
            }
            return UrlValidationResult(
                success: false,
                error: "Server returned HTTP \(http.statusCode)."
            )
        } catch {
            return UrlValidationResult(success: false, error: error.localizedDescription)
        }
    }
}

import Foundation
import AppKit
import OSLog

class FileDownloadService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "FileDownloadService")
    private let fileLogger: DailyFileLogger
    private let session: URLSession
    private let tempFolder: URL

    init(
        fileLogger: DailyFileLogger,
        session: URLSession? = nil,
        tempFolder: URL = ApplicationPaths.tempFolder
    ) {
        self.fileLogger = fileLogger
        self.tempFolder = tempFolder
        if let session = session {
            self.session = session
        } else {
            let config = URLSessionConfiguration.default
            config.timeoutIntervalForRequest = 60
            self.session = URLSession(configuration: config)
        }
    }

    func downloadAndOpen(path: String, baseUrl: String, openFile: Bool = true, cookies: [HTTPCookie] = []) async throws {
        // Encode like Uri.EscapeDataString: only unreserved chars (RFC 3986) left bare,
        // so slashes and other reserved chars in the value don't confuse the server.
        let unreserved = CharacterSet.alphanumerics.union(CharacterSet(charactersIn: "-._~"))
        guard let encodedPath = path.addingPercentEncoding(withAllowedCharacters: unreserved) else {
            throw DownloadError.invalidPath
        }

        let cookieHeader: String? = cookies.isEmpty ? nil
            : cookies.map { "\($0.name)=\($0.value)" }.joined(separator: "; ")

        guard let indexURL = URL(string: "\(baseUrl)/starsky/api/index?f=\(encodedPath)") else {
            throw DownloadError.invalidPath
        }
        let (_, indexResponse) = try await session.data(for: request(indexURL, cookieHeader: cookieHeader))
        guard let http = indexResponse as? HTTPURLResponse, http.statusCode == 200 else {
            throw DownloadError.fileNotFound
        }

        if let sidecarURL = URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=\(encodedPath)") {
            _ = try? await session.data(for: request(sidecarURL, cookieHeader: cookieHeader))
        }

        guard let photoURL = URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=\(encodedPath)&cache=false") else {
            throw DownloadError.invalidPath
        }
        let (photoData, photoResponse) = try await session.data(for: request(photoURL, cookieHeader: cookieHeader))
        guard let photoHTTP = photoResponse as? HTTPURLResponse, photoHTTP.statusCode == 200 else {
            let status = (photoResponse as? HTTPURLResponse)?.statusCode ?? -1
            logger.error("download-photo returned HTTP \(status) for \(encodedPath)")
            throw DownloadError.downloadFailed(statusCode: status)
        }

        let filename = URL(fileURLWithPath: path).lastPathComponent
        let parentDir = URL(fileURLWithPath: path).deletingLastPathComponent().path
            .trimmingCharacters(in: CharacterSet(charactersIn: "/"))
        let destDir = tempFolder.appendingPathComponent(parentDir, isDirectory: true)
        try FileManager.default.createDirectory(at: destDir, withIntermediateDirectories: true)

        let tmpURL = destDir.appendingPathComponent("\(filename).tmp")
        let finalURL = destDir.appendingPathComponent(filename)
        try photoData.write(to: tmpURL)
        if FileManager.default.fileExists(atPath: finalURL.path) {
            try FileManager.default.removeItem(at: finalURL)
        }
        try FileManager.default.moveItem(at: tmpURL, to: finalURL)

        if openFile {
            _ = await MainActor.run {
                NSWorkspace.shared.open(finalURL)
            }
        }

        fileLogger.info("Downloaded and opened \(filename)", category: "FileDownloadService")
    }

    private func request(_ url: URL, cookieHeader: String?) -> URLRequest {
        var req = URLRequest(url: url)
        if let cookieHeader {
            req.setValue(cookieHeader, forHTTPHeaderField: "Cookie")
        }
        return req
    }
}

enum DownloadError: LocalizedError {
    case invalidPath
    case fileNotFound
    case downloadFailed(statusCode: Int)

    var errorDescription: String? {
        switch self {
        case .invalidPath: return "Invalid file path."
        case .fileNotFound: return "File not found on server."
        case .downloadFailed(let statusCode): return "Download failed (HTTP \(statusCode))."
        }
    }
}

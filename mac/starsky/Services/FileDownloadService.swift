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

    func downloadAndOpen(path: String, baseUrl: String, openFile: Bool = true) async throws {
        guard let encodedPath = path.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) else {
            throw DownloadError.invalidPath
        }

        guard let indexURL = URL(string: "\(baseUrl)/starsky/api/index?f=\(encodedPath)") else {
            throw DownloadError.invalidPath
        }
        let (_, indexResponse) = try await session.data(from: indexURL)
        guard let http = indexResponse as? HTTPURLResponse, http.statusCode == 200 else {
            throw DownloadError.fileNotFound
        }

        if let sidecarURL = URL(string: "\(baseUrl)/starsky/api/download-sidecar?f=\(encodedPath)") {
            _ = try? await session.data(from: sidecarURL)
        }

        guard let photoURL = URL(string: "\(baseUrl)/starsky/api/download-photo?isThumbnail=false&f=\(encodedPath)&cache=false") else {
            throw DownloadError.invalidPath
        }
        let (photoData, photoResponse) = try await session.data(from: photoURL)
        guard let photoHTTP = photoResponse as? HTTPURLResponse, photoHTTP.statusCode == 200 else {
            throw DownloadError.downloadFailed
        }

        let filename = URL(fileURLWithPath: path).lastPathComponent
        let parentDir = URL(fileURLWithPath: path).deletingLastPathComponent().lastPathComponent
        let destDir = tempFolder.appendingPathComponent(parentDir, isDirectory: true)
        try FileManager.default.createDirectory(at: destDir, withIntermediateDirectories: true)

        let tmpURL = destDir.appendingPathComponent("\(filename).tmp")
        let finalURL = destDir.appendingPathComponent(filename)
        try photoData.write(to: tmpURL)
        _ = try? FileManager.default.replaceItemAt(finalURL, withItemAt: tmpURL)

        if openFile {
            await MainActor.run {
                NSWorkspace.shared.open(finalURL)
            }
        }

        fileLogger.info("Downloaded and opened \(filename)", category: "FileDownloadService")
    }
}

enum DownloadError: LocalizedError {
    case invalidPath
    case fileNotFound
    case downloadFailed

    var errorDescription: String? {
        switch self {
        case .invalidPath: return "Invalid file path."
        case .fileNotFound: return "File not found on server."
        case .downloadFailed: return "Download failed."
        }
    }
}

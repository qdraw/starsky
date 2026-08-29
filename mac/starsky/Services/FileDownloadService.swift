import Foundation
import AppKit
import OSLog

class FileDownloadService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "FileDownloadService")
    private let fileLogger: DailyFileLogger
    private let session: URLSession
    private let tempFolder: URL

    private let watcherQueue = DispatchQueue(label: "nl.qdraw.starsky.fileuploadwatcher")
    private var watcherSources: [String: DispatchSourceFileSystemObject] = [:]
    private var debounceItems: [String: DispatchWorkItem] = [:]
    private var remoteContexts: [String: (remotePath: String, baseUrl: String, cookieProvider: () async -> [HTTPCookie])] = [:]

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

    deinit {
        watcherQueue.sync {
            debounceItems.values.forEach { $0.cancel() }
            debounceItems.removeAll()
            watcherSources.values.forEach { $0.cancel() }
            watcherSources.removeAll()
            remoteContexts.removeAll()
        }
    }

    func downloadAndOpen(path: String, baseUrl: String, openFile: Bool = true, cookies: [HTTPCookie] = [], cookieProvider: (() async -> [HTTPCookie])? = nil) async throws {
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

        if let cookieProvider {
            watchFile(localURL: finalURL, remotePath: path, baseUrl: baseUrl, cookieProvider: cookieProvider)
        }

        if openFile {
            _ = await MainActor.run {
                NSWorkspace.shared.open(finalURL)
            }
        }

        fileLogger.info("Downloaded and opened \(filename)", category: "FileDownloadService")
    }

    private func watchFile(localURL: URL, remotePath: String, baseUrl: String, cookieProvider: @escaping () async -> [HTTPCookie]) {
        let key = localURL.path

        watcherQueue.sync {
            watcherSources[key]?.cancel()
            watcherSources.removeValue(forKey: key)
            debounceItems[key]?.cancel()

            let fd = open(localURL.path, O_EVTONLY)
            guard fd >= 0 else {
                logger.warning("Could not open file fd for watching: \(localURL.path)")
                return
            }

            remoteContexts[key] = (remotePath, baseUrl, cookieProvider)

            let src = DispatchSource.makeFileSystemObjectSource(
                fileDescriptor: fd,
                eventMask: .write,
                queue: watcherQueue
            )
            src.setEventHandler { [weak self] in self?.handleFileChange(key: key, localURL: localURL) }
            src.setCancelHandler { close(fd) }
            src.resume()
            watcherSources[key] = src

            logger.info("Watching for changes: \(localURL.lastPathComponent)")
        }
    }

    private func handleFileChange(key: String, localURL: URL) {
        debounceItems[key]?.cancel()
        let item = DispatchWorkItem { [weak self] in
            guard let self, let ctx = self.remoteContexts[key] else { return }
            Task {
                let cookies = await ctx.cookieProvider()
                do {
                    try await self.upload(localURL: localURL, remotePath: ctx.remotePath, baseUrl: ctx.baseUrl, cookies: cookies)
                } catch {
                    self.logger.error("Upload failed for \(localURL.lastPathComponent): \(error.localizedDescription)")
                    self.fileLogger.info("Upload failed for \(localURL.lastPathComponent): \(error.localizedDescription)", category: "FileDownloadService")
                }
            }
        }
        debounceItems[key] = item
        watcherQueue.asyncAfter(deadline: .now() + 1.0, execute: item)
    }

    func upload(localURL: URL, remotePath: String, baseUrl: String, cookies: [HTTPCookie] = []) async throws {
        guard let data = try? Data(contentsOf: localURL) else {
            throw UploadError.readFailed
        }

        let parentDir = URL(fileURLWithPath: remotePath).deletingLastPathComponent().path
        let filename = URL(fileURLWithPath: remotePath).lastPathComponent

        guard let uploadURL = URL(string: "\(baseUrl)/starsky/api/upload") else {
            throw UploadError.invalidPath
        }

        var req = URLRequest(url: uploadURL)
        req.httpMethod = "POST"
        req.setValue("application/octet-stream", forHTTPHeaderField: "Content-Type")
        req.setValue(parentDir, forHTTPHeaderField: "to")
        req.setValue(filename, forHTTPHeaderField: "filename")
        if !cookies.isEmpty {
            req.setValue(cookies.map { "\($0.name)=\($0.value)" }.joined(separator: "; "), forHTTPHeaderField: "Cookie")
        }
        req.httpBody = data

        let (_, response) = try await session.data(for: req)
        guard let http = response as? HTTPURLResponse, (200...299).contains(http.statusCode) else {
            let status = (response as? HTTPURLResponse)?.statusCode ?? -1
            logger.error("Upload returned HTTP \(status) for \(remotePath)")
            throw UploadError.uploadFailed(statusCode: status)
        }

        fileLogger.info("Uploaded \(filename) to \(remotePath)", category: "FileDownloadService")
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

enum UploadError: LocalizedError {
    case readFailed
    case invalidPath
    case uploadFailed(statusCode: Int)

    var errorDescription: String? {
        switch self {
        case .readFailed: return "Could not read local file for upload."
        case .invalidPath: return "Invalid upload URL."
        case .uploadFailed(let statusCode): return "Upload failed (HTTP \(statusCode))."
        }
    }
}

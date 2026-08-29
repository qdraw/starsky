import Foundation
import AppKit
import OSLog

class FileDownloadService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "FileDownloadService")
    private let fileLogger: DailyFileLogger
    private let session: URLSession
    private let tempFolder: URL

    private let watcherQueue = DispatchQueue(label: "nl.qdraw.starsky.fileuploadwatcher")
    // Per-file fd sources (catch in-place writes; key = file path)
    private var fileSources: [String: DispatchSourceFileSystemObject] = [:]
    // Per-directory fd sources (catch atomic renames and new files; key = dir path)
    private var dirSources: [String: DispatchSourceFileSystemObject] = [:]
    // Dir path → set of tracked file paths within it (files we're actively monitoring)
    private var dirFiles: [String: Set<String>] = [:]
    // Dir path → set of file paths that existed when we first started watching the directory.
    // Files NOT in this set that appear later are treated as new editor exports and uploaded.
    private var dirKnownFiles: [String: Set<String>] = [:]
    // Last mtime we saw when the file was downloaded or last uploaded
    private var lastMtimes: [String: Date] = [:]

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
            fileSources.values.forEach { $0.cancel() }
            fileSources.removeAll()
            dirSources.values.forEach { $0.cancel() }
            dirSources.removeAll()
            remoteContexts.removeAll()
            lastMtimes.removeAll()
            dirFiles.removeAll()
            dirKnownFiles.removeAll()
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
        let dirPath = localURL.deletingLastPathComponent().path

        watcherQueue.sync {
            debounceItems[key]?.cancel()
            remoteContexts[key] = (remotePath, baseUrl, cookieProvider)
            lastMtimes[key] = mtime(of: localURL)

            registerFileSource(key: key, localURL: localURL)

            if dirSources[dirPath] == nil {
                // Snapshot files already present so new editor exports can be distinguished
                let existing = (try? FileManager.default.contentsOfDirectory(
                    at: URL(fileURLWithPath: dirPath),
                    includingPropertiesForKeys: nil
                ))?.map(\.path) ?? []
                dirKnownFiles[dirPath] = Set(existing)
                registerDirSource(dirPath: dirPath)
            }
            dirFiles[dirPath, default: []].insert(key)
        }
    }

    // MARK: - Source registration (must be called on watcherQueue)

    private func registerFileSource(key: String, localURL: URL) {
        fileSources[key]?.cancel()
        fileSources.removeValue(forKey: key)

        let fd = open(localURL.path, O_EVTONLY)
        guard fd >= 0 else {
            logger.warning("Could not open file fd for watching: \(localURL.lastPathComponent)")
            return
        }

        let src = DispatchSource.makeFileSystemObjectSource(
            fileDescriptor: fd,
            eventMask: [.write, .delete, .rename],
            queue: watcherQueue
        )
        src.setEventHandler { [weak self, weak src] in
            let events = src?.data ?? []
            self?.handleFileEvent(key: key, localURL: localURL, events: events)
        }
        src.setCancelHandler { close(fd) }
        src.resume()
        fileSources[key] = src

        logger.info("Watching for changes: \(localURL.lastPathComponent)")
        fileLogger.info("Watching for changes: \(localURL.lastPathComponent)", category: "FileDownloadService")
    }

    private func registerDirSource(dirPath: String) {
        let fd = open(dirPath, O_EVTONLY)
        guard fd >= 0 else { return }

        let src = DispatchSource.makeFileSystemObjectSource(
            fileDescriptor: fd,
            eventMask: [.write, .link, .rename],
            queue: watcherQueue
        )
        src.setEventHandler { [weak self] in self?.handleDirEvent(dirPath: dirPath) }
        src.setCancelHandler { close(fd) }
        src.resume()
        dirSources[dirPath] = src
    }

    // MARK: - Event handlers (run on watcherQueue)

    private func handleFileEvent(key: String, localURL: URL, events: DispatchSource.FileSystemEvent) {
        logger.info("File event for: \(localURL.lastPathComponent)")
        fileLogger.info("File event for: \(localURL.lastPathComponent)", category: "FileDownloadService")
        // Only re-register after delete/rename (atomic write replaced the inode).
        // Plain .write events keep the same inode, so the existing source stays valid.
        if events.contains(.delete) || events.contains(.rename) {
            watcherQueue.asyncAfter(deadline: .now() + 0.1) { [weak self] in
                guard let self, self.remoteContexts[key] != nil else { return }
                self.registerFileSource(key: key, localURL: localURL)
            }
        }
        scheduleUpload(key: key, localURL: localURL)
    }

    private func handleDirEvent(dirPath: String) {
        guard let fileKeys = dirFiles[dirPath], !fileKeys.isEmpty else { return }

        // Part 1: tracked files whose mtime changed (in-place or atomic overwrite)
        for key in fileKeys {
            guard remoteContexts[key] != nil else { continue }
            let localURL = URL(fileURLWithPath: key)
            guard let currentMtime = mtime(of: localURL),
                  currentMtime > (lastMtimes[key] ?? .distantPast) else { continue }
            logger.info("Dir event detected change for: \(localURL.lastPathComponent)")
            fileLogger.info("Dir event detected change for: \(localURL.lastPathComponent)", category: "FileDownloadService")
            registerFileSource(key: key, localURL: localURL)
            scheduleUpload(key: key, localURL: localURL)
        }

        // Part 2: files that weren't present when we started watching — editor exports
        guard let ctxKey = fileKeys.first(where: { remoteContexts[$0] != nil }),
              let ctx = remoteContexts[ctxKey] else { return }
        let remoteParentDir = URL(fileURLWithPath: ctx.remotePath).deletingLastPathComponent().path
        let known = dirKnownFiles[dirPath] ?? []

        guard let contents = try? FileManager.default.contentsOfDirectory(
            at: URL(fileURLWithPath: dirPath),
            includingPropertiesForKeys: nil
        ) else { return }

        for fileURL in contents where fileURL.pathExtension != "tmp" {
            let newKey = fileURL.path
            guard !known.contains(newKey), remoteContexts[newKey] == nil else { continue }
            let remotePath = remoteParentDir + "/" + fileURL.lastPathComponent
            remoteContexts[newKey] = (remotePath, ctx.baseUrl, ctx.cookieProvider)
            dirFiles[dirPath, default: []].insert(newKey)
            dirKnownFiles[dirPath, default: []].insert(newKey)
            registerFileSource(key: newKey, localURL: fileURL)
            scheduleUpload(key: newKey, localURL: fileURL)
            logger.info("New file in dir, will upload: \(fileURL.lastPathComponent)")
            fileLogger.info("New file in dir, will upload: \(fileURL.lastPathComponent)", category: "FileDownloadService")
        }
    }

    // MARK: - Upload scheduling

    private func scheduleUpload(key: String, localURL: URL) {
        debounceItems[key]?.cancel()
        let item = DispatchWorkItem { [weak self] in
            guard let self, let ctx = self.remoteContexts[key] else {
                self?.fileLogger.info("Upload skipped (no context): \(localURL.lastPathComponent)", category: "FileDownloadService")
                return
            }
            self.fileLogger.info("Starting upload for: \(localURL.lastPathComponent)", category: "FileDownloadService")
            Task {
                let cookies = await ctx.cookieProvider()
                self.fileLogger.info("Got \(cookies.count) cookies, uploading: \(localURL.lastPathComponent)", category: "FileDownloadService")
                do {
                    try await self.upload(localURL: localURL, remotePath: ctx.remotePath, baseUrl: ctx.baseUrl, cookies: cookies)
                    self.watcherQueue.async { self.lastMtimes[key] = self.mtime(of: localURL) }
                } catch {
                    self.logger.error("Upload failed for \(localURL.lastPathComponent): \(error.localizedDescription)")
                    self.fileLogger.info("Upload failed for \(localURL.lastPathComponent): \(error.localizedDescription)", category: "FileDownloadService")
                }
            }
        }
        debounceItems[key] = item
        watcherQueue.asyncAfter(deadline: .now() + 1.0, execute: item)
    }

    // MARK: - Upload

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

    // MARK: - Helpers

    private func mtime(of url: URL) -> Date? {
        (try? FileManager.default.attributesOfItem(atPath: url.path))?[FileAttributeKey.modificationDate] as? Date
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

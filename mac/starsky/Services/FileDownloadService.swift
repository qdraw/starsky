import Foundation
import AppKit
import OSLog
import CoreServices

// File-level function ensures correct @convention(c) compilation (no Swift closure thunk).
private func fsEventsCallback(
    _ stream: ConstFSEventStreamRef,
    _ info: UnsafeMutableRawPointer?,
    _ numEvents: Int,
    _ eventPaths: UnsafeMutableRawPointer,
    _ eventFlags: UnsafePointer<FSEventStreamEventFlags>,
    _ eventIds: UnsafePointer<FSEventStreamEventId>
) {
    guard let info = info else { return }
    let svc = Unmanaged<FileDownloadService>.fromOpaque(info).takeUnretainedValue()
    var paths: [String] = []
    let cPaths = eventPaths.assumingMemoryBound(to: UnsafePointer<CChar>.self)
    for i in 0..<numEvents { paths.append(String(cString: cPaths[i])) }
    let flags = Array(UnsafeBufferPointer(start: eventFlags, count: numEvents))
    svc.handleFSEvents(paths: paths, flags: flags)
}

class FileDownloadService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "FileDownloadService")
    private let fileLogger: DailyFileLogger
    private let session: URLSession
    private let tempFolder: URL

    private let watcherQueue = DispatchQueue(label: "nl.qdraw.starsky.fileuploadwatcher")
    // localPath → remote context for files we downloaded and are watching
    private var remoteContexts: [String: (remotePath: String, baseUrl: String, cookieProvider: () async -> [HTTPCookie])] = [:]
    // dirPath → context — new files in this dir inherit this for upload
    private var dirContexts: [String: (baseUrl: String, cookieProvider: () async -> [HTTPCookie])] = [:]
    // Last mtime at download or last upload — prevents re-uploading unchanged files
    private var lastMtimes: [String: Date] = [:]
    // Per-file upload debounce
    private var debounceItems: [String: DispatchWorkItem] = [:]
    // FSEvents stream watching tempFolder recursively
    private var streamRef: FSEventStreamRef?

    init(
        fileLogger: DailyFileLogger,
        session: URLSession? = nil,
        tempFolder: URL = ApplicationPaths.tempFolder
    ) {
        self.fileLogger = fileLogger
        // Resolve symlinks once so FSEvents canonical paths and our stored keys always match.
        // FileManager.default.temporaryDirectory returns /var/folders/... (a symlink);
        // FSEvents delivers /private/var/folders/... (the real path).
        self.tempFolder = tempFolder.resolvingSymlinksInPath()
        if let session = session {
            self.session = session
        } else {
            let config = URLSessionConfiguration.default
            config.timeoutIntervalForRequest = 60
            self.session = URLSession(configuration: config)
        }
    }

    deinit {
        if let stream = streamRef {
            FSEventStreamStop(stream)
            FSEventStreamInvalidate(stream)
            FSEventStreamRelease(stream)
        }
        watcherQueue.sync {
            debounceItems.values.forEach { $0.cancel() }
            debounceItems.removeAll()
            remoteContexts.removeAll()
            dirContexts.removeAll()
            lastMtimes.removeAll()
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
            remoteContexts[key] = (remotePath, baseUrl, cookieProvider)
            dirContexts[dirPath] = (baseUrl, cookieProvider)
            lastMtimes[key] = mtime(of: localURL)
            startFSEventsWatcherIfNeeded()
        }
    }

    // MARK: - FSEvents (callback fires on watcherQueue)

    private func startFSEventsWatcherIfNeeded() {
        guard streamRef == nil else { return }
        let watchPath = tempFolder.path
        try? FileManager.default.createDirectory(atPath: watchPath, withIntermediateDirectories: true, attributes: nil)

        var ctx = FSEventStreamContext(
            version: 0,
            info: Unmanaged.passUnretained(self).toOpaque(),
            retain: nil, release: nil, copyDescription: nil
        )

        let createFlags = UInt32(
            kFSEventStreamCreateFlagFileEvents |
            kFSEventStreamCreateFlagNoDefer
        )

        guard let stream = FSEventStreamCreate(
            kCFAllocatorDefault,
            fsEventsCallback,
            &ctx,
            [watchPath] as CFArray,
            FSEventStreamEventId(kFSEventStreamEventIdSinceNow),
            0.5,
            createFlags
        ) else {
            logger.error("Failed to create FSEvents stream")
            return
        }

        FSEventStreamSetDispatchQueue(stream, watcherQueue)
        guard FSEventStreamStart(stream) else {
            FSEventStreamRelease(stream)
            logger.error("Failed to start FSEvents stream")
            return
        }
        streamRef = stream
        logger.info("Watching temp folder: \(watchPath)")
        fileLogger.info("Watching temp folder for changes", category: "FileDownloadService")
    }

    func handleFSEvents(paths: [String], flags: [FSEventStreamEventFlags]) {
        let isFile    = UInt32(kFSEventStreamEventFlagItemIsFile)
        let isRemoved = UInt32(kFSEventStreamEventFlagItemRemoved)
        let isChange  = UInt32(
            kFSEventStreamEventFlagItemCreated |
            kFSEventStreamEventFlagItemModified |
            kFSEventStreamEventFlagItemRenamed
        )
        let tempPrefix = tempFolder.path + "/"

        for (path, flag) in zip(paths, flags) {
            guard flag & isFile != 0 else { continue }
            guard flag & isRemoved == 0 else { continue }
            guard flag & isChange != 0 else { continue }
            guard !path.hasSuffix(".tmp") else { continue }
            guard path.hasPrefix(tempPrefix) else { continue }

            let localURL = URL(fileURLWithPath: path)

            // Register new file in a watched directory
            if remoteContexts[path] == nil {
                let dirPath = localURL.deletingLastPathComponent().path
                guard let dirCtx = dirContexts[dirPath] else { continue }
                let remotePath = String(path.dropFirst(tempFolder.path.count))
                remoteContexts[path] = (remotePath, dirCtx.baseUrl, dirCtx.cookieProvider)
                logger.info("New file detected, will upload: \(localURL.lastPathComponent)")
                fileLogger.info("New file detected, will upload: \(localURL.lastPathComponent)", category: "FileDownloadService")
            }

            guard remoteContexts[path] != nil else { continue }

            // Skip if mtime hasn't changed
            guard let currentMtime = mtime(of: localURL),
                  currentMtime != lastMtimes[path] else { continue }

            logger.info("File changed, scheduling upload: \(localURL.lastPathComponent)")
            fileLogger.info("File changed, scheduling upload: \(localURL.lastPathComponent)", category: "FileDownloadService")
            scheduleUpload(key: path, localURL: localURL)
        }
    }

    // MARK: - Upload scheduling

    private func scheduleUpload(key: String, localURL: URL) {
        debounceItems[key]?.cancel()
        let fl = fileLogger
        let item = DispatchWorkItem { [weak self] in
            fl.info("Starting upload for: \(localURL.lastPathComponent)", category: "FileDownloadService")
            guard let self, let ctx = self.remoteContexts[key] else {
                fl.info("Upload skipped (no context): \(localURL.lastPathComponent)", category: "FileDownloadService")
                return
            }
            Task {
                let cookies = await ctx.cookieProvider()
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

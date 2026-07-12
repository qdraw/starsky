import Foundation
import CryptoKit

/// Manage security-scoped bookmarks for persistent folder access.
/// Bookmarks are stored as files named `bookmark_<token>` inside Application Support/<bundle>/bookmarks
enum StorageBookmarkManager {
    /// Override storage directory (for tests)
    static var storageDirOverride: URL? = nil

    private static var bookmarksDirectoryURL: URL? {
        if let override = storageDirOverride { return override }
        let fm = FileManager.default
        guard let appSupport = try? fm.url(for: .applicationSupportDirectory, in: .userDomainMask, appropriateFor: nil, create: true) else { return nil }
        let bundleId = Bundle.main.bundleIdentifier ?? "starsky"
        let dir = appSupport.appendingPathComponent(bundleId, isDirectory: true).appendingPathComponent("bookmarks", isDirectory: true)
        if !fm.fileExists(atPath: dir.path) {
            try? fm.createDirectory(at: dir, withIntermediateDirectories: true, attributes: nil)
        }
        return dir
    }

    private static func sha256Hex(_ input: String) -> String {
        let data = Data(input.utf8)
        let hash = SHA256.hash(data: data)
        return hash.map { String(format: "%02x", $0) }.joined()
    }

    /// Returns either an existing bookmark file URL (when existingOnly=true)
    /// or a target URL for saving (hashed filename) when existingOnly=false.
    private static func bookmarkFileURL(for token: String, existingOnly: Bool = false) -> URL? {
        guard let dir = bookmarksDirectoryURL else { return nil }
        let originalFile = dir.appendingPathComponent("bookmark_\(token)")
        let hashedFile = dir.appendingPathComponent("bookmark_\(sha256Hex(token))")

        if existingOnly {
            let fm = FileManager.default
            if fm.fileExists(atPath: originalFile.path) { return originalFile }
            if fm.fileExists(atPath: hashedFile.path) { return hashedFile }
            return nil
        }
        // When saving, always use the hashed filename to avoid problematic characters.
        return hashedFile
    }

    /// Try several base64 normalization strategies and return decoded data if any succeeds.
    private static func tryDecodeBase64(_ token: String) -> Data? {
        let attempts = [
            token,
            token.trimmingCharacters(in: .whitespacesAndNewlines),
            token.replacingOccurrences(of: "\n", with: ""),
            token.replacingOccurrences(of: "\r", with: ""),
        ]

        // Also try URL-safe base64 conversions
        var extra: [String] = []
        if token.contains("-") || token.contains("_") {
            var s = token
            s = s.replacingOccurrences(of: "-", with: "+")
            s = s.replacingOccurrences(of: "_", with: "/")
            extra.append(s)
        }

        // Percent decode if it looks URL encoded
        if let percentDecoded = token.removingPercentEncoding { extra.append(percentDecoded) }

        let all = attempts + extra
        for attempt in all {
            if let d = Data(base64Encoded: attempt) { return d }
        }
        return nil
    }

    /// Try to interpret `token` as base64-encoded bookmark data. If it decodes and resolves,
    /// persist the bookmark data to the canonical bookmark file and return the resolved URL.
    private static func tryPersistBookmarkDataFromToken(_ token: String) throws -> URL? {
        // Heuristic: short tokens are unlikely to be base64 bookmark data. Try decode anyway.
        guard let decoded = tryDecodeBase64(token) else {
            NSLog("[StorageBookmarkManager] token is not decodable base64 (len=\(token.count))")
            return nil
        }

        NSLog("[StorageBookmarkManager] token decoded to \(decoded.count) bytes; attempting to resolve bookmark data")

        // Try resolve the bookmark data without starting access.
        var isStale = false
        do {
            let resolved = try URL(resolvingBookmarkData: decoded, options: [.withSecurityScope], relativeTo: nil, bookmarkDataIsStale: &isStale)
            // Persist decoded bookmark data to canonical file so future loads use the file.
            if let file = bookmarkFileURL(for: token, existingOnly: false) {
                NSLog("[StorageBookmarkManager] Persisting decoded bookmark data to: %@", file.path)
                try decoded.write(to: file, options: [.atomic])
                // Log saved file size
                if let attr = try? FileManager.default.attributesOfItem(atPath: file.path), let size = attr[.size] as? NSNumber {
                    NSLog("[StorageBookmarkManager] Persisted bookmark file size: %d", size.intValue)
                }
            }
            return resolved
        } catch let err as NSError {
            NSLog("[StorageBookmarkManager] Failed to resolve bookmark data decoded from token: %@", err.localizedDescription)
            return nil
        }
    }

    /// Save a security-scoped bookmark for the given folder path using the provided token.
    /// Overwrites existing bookmark file for the token.
    static func saveBookmark(forFolderPath folderPath: String, token: String) throws {
        let url = URL(fileURLWithPath: folderPath, isDirectory: true)
        let bookmarkData = try url.bookmarkData(options: [.withSecurityScope], includingResourceValuesForKeys: nil, relativeTo: nil)
        guard let file = bookmarkFileURL(for: token, existingOnly: false) else { throw NSError(domain: "StorageBookmarkManager", code: 1, userInfo: [NSLocalizedDescriptionKey: "No bookmarks directory available"]) }
        NSLog("[StorageBookmarkManager] Saving bookmark for token to: %@", file.path)
        try bookmarkData.write(to: file, options: [.atomic])
    }

    /// Load a bookmark for token and resolve it to a URL (without starting access).
    static func loadBookmarkURL(token: String) throws -> URL {
        guard let file = bookmarkFileURL(for: token, existingOnly: true) else { throw NSError(domain: "StorageBookmarkManager", code: 2, userInfo: [NSLocalizedDescriptionKey: "No bookmarks directory available or bookmark not found"]) }
        NSLog("[StorageBookmarkManager] Loading bookmark from: %@", file.path)
        let data = try Data(contentsOf: file)
        NSLog("[StorageBookmarkManager] Read bookmark file size: %d", data.count)
        var isStale = false
        do {
            let resolved = try URL(resolvingBookmarkData: data, options: [.withSecurityScope], relativeTo: nil, bookmarkDataIsStale: &isStale)
            return resolved
        } catch let err as NSError {
            // Provide more detail in the thrown error
            var info = err.userInfo
            info["bookmarkFilePath"] = file.path
            info["bookmarkFileSize"] = data.count
            let enhanced = NSError(domain: err.domain, code: err.code, userInfo: info)
            NSLog("[StorageBookmarkManager] resolving bookmark failed: %@; file=%@; size=%d", String(describing: err), file.path, data.count)
            throw enhanced
        }
    }

    /// Start security-scoped access for bookmark identified by token. Returns the resolved URL if success.
    /// Caller must call `stopAccess(_:)` when done.
    static func startAccessForToken(_ token: String) throws -> URL {
        let url = try loadBookmarkURL(token: token)
        let ok = url.startAccessingSecurityScopedResource()
        if !ok {
            // Even if false, we return the URL so caller can attempt to use it, but indicate failure via error
            throw NSError(domain: "StorageBookmarkManager", code: 3, userInfo: [NSLocalizedDescriptionKey: "Failed to start accessing security scoped resource for \(url.path)"]) }
        NSLog("[StorageBookmarkManager] Started access for URL: %@", url.path)
        return url
    }

    /// Stop security-scoped access for the provided URL.
    static func stopAccess(_ url: URL) {
        NSLog("[StorageBookmarkManager] Stopping access for URL: %@", url.path)
        url.stopAccessingSecurityScopedResource()
    }

    /// Convenience: ensure bookmark exists for folder/token, creating it if missing, and start access.
    /// Returns the resolved URL (and starts access). Caller must call `stopAccess(_:)`.
    static func ensureBookmarkAndStartAccess(folderPath: String, token: String) throws -> URL {
        // If bookmark file exists, try starting access; if fails, recreate
        if let existing = bookmarkFileURL(for: token, existingOnly: true) {
            NSLog("[StorageBookmarkManager] Found existing bookmark file: %@", existing.path)
            do {
                let url = try startAccessForToken(token)
                return url
            } catch {
                NSLog("[StorageBookmarkManager] Existing bookmark failed to start; will recreate. Error: %@", String(describing: error))
                // fallthrough to recreate
            }
        }

        // If the token itself contains base64 bookmark data, persist it and start access
        if let resolvedFromToken = try tryPersistBookmarkDataFromToken(token) {
            NSLog("[StorageBookmarkManager] Resolved bookmark from token data to: %@", resolvedFromToken.path)
            let ok = resolvedFromToken.startAccessingSecurityScopedResource()
            if !ok {
                throw NSError(domain: "StorageBookmarkManager", code: 3, userInfo: [NSLocalizedDescriptionKey: "Failed to start accessing security scoped resource for \(resolvedFromToken.path)"])
            }
            NSLog("[StorageBookmarkManager] Started access for URL (from token data): %@", resolvedFromToken.path)
            return resolvedFromToken
        }

        // create bookmark then start (saved to hashed filename)
        // Note: creating a bookmark from folderPath requires the process to have access to folderPath.
        // If we reached here it means token didn't help; attempting to create a bookmark may fail.
        NSLog("[StorageBookmarkManager] Attempting to create bookmark from folderPath: %@", folderPath)
        try saveBookmark(forFolderPath: folderPath, token: token)
        let url = try startAccessForToken(token)
        return url
    }

    /// Remove stored bookmark file for token
    static func removeBookmark(token: String) throws {
        guard let dir = bookmarksDirectoryURL else { return }
        let fm = FileManager.default
        let originalFile = dir.appendingPathComponent("bookmark_\(token)")
        let hashedFile = dir.appendingPathComponent("bookmark_\(sha256Hex(token))")
        if fm.fileExists(atPath: originalFile.path) {
            try fm.removeItem(at: originalFile)
        }
        if fm.fileExists(atPath: hashedFile.path) {
            try fm.removeItem(at: hashedFile)
        }
    }
}

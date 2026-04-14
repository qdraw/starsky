import Foundation

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

	private static func bookmarkFileURL(for token: String) -> URL? {
		guard let dir = bookmarksDirectoryURL else { return nil }
		let file = "bookmark_\(token)"
		return dir.appendingPathComponent(file)
	}

	/// Save a security-scoped bookmark for the given folder path using the provided token.
	/// Overwrites existing bookmark file for the token.
	static func saveBookmark(forFolderPath folderPath: String, token: String) throws {
		let url = URL(fileURLWithPath: folderPath, isDirectory: true)
		let bookmarkData = try url.bookmarkData(options: [.withSecurityScope], includingResourceValuesForKeys: nil, relativeTo: nil)
		guard let file = bookmarkFileURL(for: token) else { throw NSError(domain: "StorageBookmarkManager", code: 1, userInfo: [NSLocalizedDescriptionKey: "No bookmarks directory available"]) }
		try bookmarkData.write(to: file, options: [.atomic])
	}

	/// Load a bookmark for token and resolve it to a URL (without starting access).
	static func loadBookmarkURL(token: String) throws -> URL {
		guard let file = bookmarkFileURL(for: token) else { throw NSError(domain: "StorageBookmarkManager", code: 2, userInfo: [NSLocalizedDescriptionKey: "No bookmarks directory available"]) }
		let data = try Data(contentsOf: file)
		var isStale = false
		let resolved = try URL(resolvingBookmarkData: data, options: [.withSecurityScope], relativeTo: nil, bookmarkDataIsStale: &isStale)
		return resolved
	}

	/// Start security-scoped access for bookmark identified by token. Returns the resolved URL if success.
	/// Caller must call `stopAccess(_:)` when done.
	static func startAccessForToken(_ token: String) throws -> URL {
		let url = try loadBookmarkURL(token: token)
		let ok = url.startAccessingSecurityScopedResource()
		if !ok {
			// Even if false, we return the URL so caller can attempt to use it, but indicate failure via error
			throw NSError(domain: "StorageBookmarkManager", code: 3, userInfo: [NSLocalizedDescriptionKey: "Failed to start accessing security scoped resource for \(url.path)"]) }
		return url
	}

	/// Stop security-scoped access for the provided URL.
	static func stopAccess(_ url: URL) {
		url.stopAccessingSecurityScopedResource()
	}

	/// Convenience: ensure bookmark exists for folder/token, creating it if missing, and start access.
	/// Returns the resolved URL (and starts access). Caller must call `stopAccess(_:)`.
	static func ensureBookmarkAndStartAccess(folderPath: String, token: String) throws -> URL {
		// If bookmark file exists, try starting access; if fails, recreate
		if let file = bookmarkFileURL(for: token), FileManager.default.fileExists(atPath: file.path) {
			// try start
			do {
				let url = try startAccessForToken(token)
				return url
			} catch {
				// fallthrough to recreate
			}
		}
		// create bookmark then start
		try saveBookmark(forFolderPath: folderPath, token: token)
		let url = try startAccessForToken(token)
		return url
	}

	/// Remove stored bookmark file for token
	static func removeBookmark(token: String) throws {
		guard let file = bookmarkFileURL(for: token) else { return }
		if FileManager.default.fileExists(atPath: file.path) {
			try FileManager.default.removeItem(at: file)
		}
	}
}

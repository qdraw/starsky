import AppKit
import WebKit
import OSLog

// Handles the "storageFolderPicker" message posted from the web layer.
// Presents an NSOpenPanel so the user can choose a storage folder, then:
//   - Saves a security-scoped bookmark to the App Group container's bookmarks/
//     directory. In MAS builds this is required to regain access after relaunch;
//     in Developer ID builds it is harmless and keeps SecurityScopedBookmarkService
//     (.NET) consistent across distribution channels.
//   - Evaluates window.__starskyStorageFolderSelected("<path>") in the web view
//     so React can call ChangeSetting and update the displayed path.
final class StorageFolderPickerHandler: NSObject, WKScriptMessageHandler, @unchecked Sendable {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "StorageFolderPicker")
    private weak var webView: WKWebView?
    private var accessedURLs: [URL] = []

    init(webView: WKWebView) {
        self.webView = webView
    }

    func userContentController(_: WKUserContentController, didReceive _: WKScriptMessage) {
        DispatchQueue.main.async { [weak self] in
            self?.showPanel()
        }
    }

    private func showPanel() {
        let panel = NSOpenPanel()
        panel.canChooseDirectories = true
        panel.canChooseFiles = false
        panel.allowsMultipleSelection = false
        panel.prompt = "Select"
        panel.message = "Choose the Starsky photo library folder"

        guard panel.runModal() == .OK, let url = panel.url else { return }

        saveBookmark(for: url)

        let escaped = url.path
            .replacingOccurrences(of: "\\", with: "\\\\")
            .replacingOccurrences(of: "\"", with: "\\\"")
        webView?.evaluateJavaScript("window.__starskyStorageFolderSelected(\"\(escaped)\")")
    }

    private func saveBookmark(for url: URL) {
        do {
            let bookmarkData = try url.bookmarkData(
                options: [.withSecurityScope],
                includingResourceValuesForKeys: nil,
                relativeTo: nil
            )
            // Use a stable filename derived from the path so re-selecting the same folder
            // overwrites the existing bookmark rather than accumulating duplicates.
            let id = Data(url.path.utf8).base64EncodedString()
                .replacingOccurrences(of: "/", with: "_")
                .replacingOccurrences(of: "=", with: "")
            let dest = ApplicationPaths.bookmarksDirectory
                .appendingPathComponent("\(id).bookmark")
            try bookmarkData.write(to: dest, options: .atomic)

            // Start accessing for the main app's own process.
            if url.startAccessingSecurityScopedResource() {
                accessedURLs.append(url)
            }
            logger.info("Bookmark saved and access started for \(url.path, privacy: .public)")
        } catch {
            logger.error("Failed to save bookmark for \(url.path, privacy: .public): \(error.localizedDescription, privacy: .public)")
        }
    }

    deinit {
        for url in accessedURLs {
            url.stopAccessingSecurityScopedResource()
        }
    }
}

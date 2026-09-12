import AppKit
import WebKit
import OSLog

// Handles WKDownloadDelegate callbacks for a single window.
// panelFactory and internal methods are injectable/overridable so tests can
// drive save-panel behavior without showing UI.
@MainActor
class DownloadCoordinator: NSObject, WKDownloadDelegate {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "DownloadCoordinator")

    weak var window: NSWindow?

    // Injectable for tests — default constructs a real NSSavePanel.
    var panelFactory: () -> NSSavePanel = { NSSavePanel() }

    init(window: NSWindow?) {
        self.window = window
        super.init()
    }

    // MARK: - WKDownloadDelegate

    func download(_: WKDownload, decideDestinationUsing _: URLResponse,
                  suggestedFilename: String,
                  completionHandler: @escaping @MainActor (URL?) -> Void) {
        decideDownloadDestination(suggestedFilename: suggestedFilename,
                                  completionHandler: completionHandler)
    }

    func download(_: WKDownload, didFailWithError error: Error, resumeData _: Data?) {
        handleDownloadFailure(error: error)
    }

    // MARK: - Internal (overridable in tests)

    func decideDownloadDestination(suggestedFilename: String,
                                   completionHandler: @escaping @MainActor (URL?) -> Void) {
        let panel = panelFactory()
        panel.nameFieldStringValue = suggestedFilename
        panel.canCreateDirectories = true
        guard let win = window else { completionHandler(nil); return }
        panel.beginSheetModal(for: win) { response in
            completionHandler(response == .OK ? panel.url : nil)
        }
    }

    func handleDownloadFailure(error: Error) {
        logger.error("Download failed: \(error.localizedDescription)")
    }
}

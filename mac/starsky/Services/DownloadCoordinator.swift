import AppKit
import WebKit
import OSLog

// Handles WKDownloadDelegate callbacks for a single window.
// savePanelPresenter is injectable so tests can drive it without UI.
@MainActor
class DownloadCoordinator: NSObject, WKDownloadDelegate {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "DownloadCoordinator")

    weak var window: NSWindow?

    var savePanelPresenter: (_ suggestedFilename: String, _ window: NSWindow?,
                             _ completion: @escaping @MainActor (URL?) -> Void) -> Void

    init(window: NSWindow?) {
        self.window = window
        self.savePanelPresenter = { filename, win, completion in
            let panel = NSSavePanel()
            panel.nameFieldStringValue = filename
            panel.canCreateDirectories = true
            guard let win else { completion(nil); return }
            panel.beginSheet(win) { response in
                completion(response == .OK ? panel.url : nil)
            }
        }
        super.init()
    }

    func download(_: WKDownload, decideDestinationUsing _: URLResponse,
                  suggestedFilename: String,
                  completionHandler: @escaping @MainActor (URL?) -> Void) {
        savePanelPresenter(suggestedFilename, window, completionHandler)
    }

    func download(_: WKDownload, didFailWithError error: Error, resumeData _: Data?) {
        logger.error("Download failed: \(error.localizedDescription)")
    }
}

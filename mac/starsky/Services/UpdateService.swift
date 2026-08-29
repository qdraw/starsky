import Foundation
import Sparkle
import OSLog

class UpdateService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "UpdateService")
    private let settingsService: SettingsService
    private var updaterController: SPUStandardUpdaterController?
    static let suppressMinutes: Double = 5760

    init(settingsService: SettingsService) {
        self.settingsService = settingsService
        do {
            updaterController = try makeUpdaterController()
        } catch {
            logger.warning("Sparkle updater unavailable: \(error.localizedDescription)")
            updaterController = nil
        }
    }

    private func makeUpdaterController() throws -> SPUStandardUpdaterController {
        SPUStandardUpdaterController(
            startingUpdater: false,
            updaterDelegate: nil,
            userDriverDelegate: nil
        )
    }

    var isAvailable: Bool { updaterController != nil }

    func checkAsync() async -> Bool {
        guard settingsService.current.updateCheckEnabled else { return false }

        if let last = settingsService.current.lastUpdateWarningShown {
            let elapsed = Date().timeIntervalSince(last) / 60
            if elapsed < Self.suppressMinutes { return false }
        }

        guard updaterController != nil else { return false }

        let controller = updaterController
        return await MainActor.run {
            controller?.updater.canCheckForUpdates ?? false
        }
    }

    func applyUpdate() {
        guard let controller = updaterController else { return }
        DispatchQueue.main.async {
            controller.updater.checkForUpdates()
        }
    }

    func recordWarningShown() {
        var settings = settingsService.current
        settings.lastUpdateWarningShown = Date()
        settingsService.save(settings)
    }
}

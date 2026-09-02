import Foundation
import Sparkle
import OSLog

private class SparkleUpdaterDelegate: NSObject, SPUUpdaterDelegate {
    private let updateService: UpdateService
    private let baseFeedURLProvider: () -> String?

    init(
        updateService: UpdateService,
        baseFeedURLProvider: @escaping () -> String? = { Bundle.main.infoDictionary?["SUFeedURL"] as? String }
    ) {
        self.updateService = updateService
        self.baseFeedURLProvider = baseFeedURLProvider
    }

    func feedURLString(for _: SPUUpdater) -> String? {
        updateService.feedURLOverride(baseFeedURL: baseFeedURLProvider())
    }
}

class UpdateService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "UpdateService")
    private let settingsService: SettingsService
    private var updaterController: SPUStandardUpdaterController?
    private var sparkleDelegate: SparkleUpdaterDelegate?
    private var isStarted = false
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
        let delegate = SparkleUpdaterDelegate(updateService: self)
        sparkleDelegate = delegate
        return SPUStandardUpdaterController(
            startingUpdater: false,
            updaterDelegate: delegate,
            userDriverDelegate: nil
        )
    }

    var isAvailable: Bool { updaterController != nil }

    // Returns nil (use Info.plist default) when pre-release is off,
    // or the base URL with ?pre-release=1 appended when it is on.
    func feedURLOverride(baseFeedURL: String?) -> String? {
        guard settingsService.current.preReleaseEnabled else { return nil }
        guard let base = baseFeedURL else { return nil }
        return base + "?pre-release=1"
    }

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
        logPublicKeyPrefix()
        DispatchQueue.main.async {
            self.startIfNeeded(controller)
            controller.updater.checkForUpdates()
        }
    }

    private func logPublicKeyPrefix() {
        if let key = Bundle.main.infoDictionary?["SUPublicEDKey"] as? String, !key.isEmpty {
            let prefix = String(key.prefix(15))
            logger.info("SUPublicEDKey prefix: \(prefix)")
        } else {
            logger.warning("SUPublicEDKey is not set")
        }
    }

    private func startIfNeeded(_ controller: SPUStandardUpdaterController) {
        guard !isStarted else { return }
        do {
            try controller.updater.start()
            isStarted = true
        } catch {
            logger.warning("Sparkle startUpdater failed: \(error.localizedDescription)")
        }
    }

    func recordWarningShown() {
        var settings = settingsService.current
        settings.lastUpdateWarningShown = Date()
        settingsService.save(settings)
    }
}

import Foundation
import Sparkle
import OSLog

private class SparkleUpdaterDelegate: NSObject, SPUUpdaterDelegate {
    private let updateService: UpdateService
    private let baseFeedURLProvider: () -> String?
    var mountWatcherProvider: (() -> (any MountWatcherServiceProtocol)?)?

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

    func updater(_: SPUUpdater, willInstallUpdate _: SUAppcastItem) {
        mountWatcherProvider?()?.stopSync()
    }
}

class UpdateService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "UpdateService")
    private let fileLogger: DailyFileLogger?
    private let settingsService: SettingsService
    private var updaterController: SPUStandardUpdaterController?
    private var sparkleDelegate: SparkleUpdaterDelegate?
    private var isStarted = false
    static let suppressMinutes: Double = 5760

    init(settingsService: SettingsService, fileLogger: DailyFileLogger? = nil) {
        self.settingsService = settingsService
        self.fileLogger = fileLogger
        updaterController = nil
        updaterController = makeSparkleController()
    }

    func makeSparkleController() -> SPUStandardUpdaterController? {
        let delegate = SparkleUpdaterDelegate(updateService: self)
        sparkleDelegate = delegate
        return SPUStandardUpdaterController(
            startingUpdater: false,
            updaterDelegate: delegate,
            userDriverDelegate: nil
        )
    }

    var isAvailable: Bool { updaterController != nil }

    var mountWatcherProvider: (() -> (any MountWatcherServiceProtocol)?)? {
        get { sparkleDelegate?.mountWatcherProvider }
        set { sparkleDelegate?.mountWatcherProvider = newValue }
    }

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
        guard let controller = updaterController else {
            logger.error("applyUpdate called but Sparkle updater is unavailable")
            fileLogger?.error("applyUpdate called but Sparkle updater is unavailable", category: "UpdateService")
            return
        }
        logPublicKeyPrefix()
        DispatchQueue.main.async {
            self.startIfNeeded(controller)
            guard controller.updater.canCheckForUpdates else {
                self.logger.warning("applyUpdate: canCheckForUpdates is false, skipping")
                self.fileLogger?.warning("applyUpdate: canCheckForUpdates is false, skipping", category: "UpdateService")
                return
            }
            controller.updater.checkForUpdates()
        }
    }

    private func logPublicKeyPrefix() {
        if let key = Bundle.main.infoDictionary?["SUPublicEDKey"] as? String, !key.isEmpty {
            let prefix = String(key.prefix(15))
            logger.info("SUPublicEDKey prefix: \(prefix)")
            fileLogger?.info("SUPublicEDKey prefix: \(prefix)", category: "UpdateService")
        } else {
            logger.warning("SUPublicEDKey is not set")
            fileLogger?.warning("SUPublicEDKey is not set", category: "UpdateService")
        }
    }

    private func startIfNeeded(_ controller: SPUStandardUpdaterController) {
        guard !isStarted else { return }
        do {
            try controller.updater.start()
            isStarted = true
        } catch {
            logger.warning("Sparkle startUpdater failed: \(error.localizedDescription)")
            fileLogger?.error("Sparkle startUpdater failed: \(error.localizedDescription)", category: "UpdateService")
        }
    }

    func recordWarningShown() {
        var settings = settingsService.current
        settings.lastUpdateWarningShown = Date()
        settingsService.save(settings)
    }
}

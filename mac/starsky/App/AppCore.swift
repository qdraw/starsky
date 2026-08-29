import AppKit
import OSLog

/// Contains all application startup, shutdown, and coordination logic.
/// AppDelegate is a thin NSApplicationDelegate adapter; AppCore holds everything testable.
class AppCore {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "AppCore")

    let settingsService: SettingsService
    let backendService: any BackendServiceProtocol
    let fileWatcherService: any FileWatcherServiceProtocol
    let updateService: UpdateService
    let windowManager: any WindowManagerProtocol

    // Injected side-effectful operations — override in tests
    var terminate: () -> Void
    var showError: @MainActor (String) -> Void
    var urlOpener: (URL) -> Void
    var healthCheckSession: URLSession
    var updateCheckDelay: UInt64
    var healthCheckRetryDelay: UInt64
    var healthCheckTimeoutSeconds: Int
    var splashStatus: @MainActor (String) -> Void
    var versionProvider: () -> String

    private(set) var localPort: Int = 0

    static let docsURL = URL(string: "https://qdraw.nl/special/starsky/docs/")!
    static let releasesURL = URL(string: "https://github.com/qdraw/starsky/releases")!

    // NOSONAR S107
    init(
        settingsService: SettingsService,
        backendService: any BackendServiceProtocol,
        fileWatcherService: any FileWatcherServiceProtocol,
        updateService: UpdateService,
        windowManager: any WindowManagerProtocol,
        terminate: @escaping () -> Void = { NSApplication.shared.terminate(nil) },
        showError: @escaping @MainActor (String) -> Void = { ErrorWindowController.show(message: $0) },
        urlOpener: @escaping (URL) -> Void = { NSWorkspace.shared.open($0) },
        healthCheckSession: URLSession = .shared,
        updateCheckDelay: UInt64 = 5_000_000_000,
        healthCheckRetryDelay: UInt64 = 1_000_000_000,
        healthCheckTimeoutSeconds: Int = 60,
        splashStatus: @escaping @MainActor (String) -> Void = { _ in
            // Intentionally no-op by default: splash updates are optional (e.g. tests/headless startup).
        },
        versionProvider: @escaping () -> String = { ApplicationInfo.version }
    ) {
        self.settingsService = settingsService
        self.backendService = backendService
        self.fileWatcherService = fileWatcherService
        self.updateService = updateService
        self.windowManager = windowManager
        self.terminate = terminate
        self.showError = showError
        self.urlOpener = urlOpener
        self.healthCheckSession = healthCheckSession
        self.updateCheckDelay = updateCheckDelay
        self.healthCheckRetryDelay = healthCheckRetryDelay
        self.healthCheckTimeoutSeconds = healthCheckTimeoutSeconds
        self.splashStatus = splashStatus
        self.versionProvider = versionProvider
    }

    // MARK: - Startup

    func startup() async {
        switch settingsService.current.mode {
        case .local:
            await startLocalMode()
        case .remote:
            await startRemoteMode()
        }
    }

    func startLocalMode() async {
        NSLog("[startup] startLocalMode begin")
        await splashStatus("Finding free port…")
        let port = PortFinder.findFreePort()
        NSLog("[startup] port=\(port)")
        guard port > 0 else {
            await showErrorAndQuit("Could not find a free port to start the backend.")
            return
        }
        localPort = port
        await MainActor.run { windowManager.setLocalPort(port) }

        await splashStatus("Starting backend…")
        NSLog("[startup] launching backend")
        do {
            try backendService.start(port: port)
            NSLog("[startup] backend launched")
        } catch {
            NSLog("[startup] backend launch error: \(error)")
            await showErrorAndQuit("Failed to start the backend: \(error.localizedDescription)")
            return
        }

        await splashStatus("Waiting for backend…")
        let baseUrl = "http://localhost:\(port)"
        NSLog("[startup] waiting for health at \(baseUrl)")
        let ready = await waitForHealth(baseUrl: baseUrl, timeoutSeconds: healthCheckTimeoutSeconds)
        NSLog("[startup] health ready=\(ready)")
        guard ready else {
            await showErrorAndQuit("Backend did not start within 60 seconds.")
            return
        }

        await splashStatus("Checking version compatibility…")
        let compatible = await checkVersionCompatibility(baseUrl: baseUrl)
        NSLog("[startup] version compatible=\(compatible)")
        guard compatible else {
            let v = versionProvider()
            await showErrorAndQuit("This version (\(v)) is incompatible with the server. Please update Starsky.")
            return
        }

        NSLog("[startup] finishStartup")
        await finishStartup()
    }

    func startRemoteMode() async {
        guard !settingsService.current.remoteBaseUrl.isEmpty else {
            await showErrorAndQuit("No remote server URL configured.\nPlease set one in Settings.")
            return
        }
        await finishStartup()
    }

    func finishStartup() async {
        fileWatcherService.start()

        await MainActor.run {
            windowManager.restoreWindows()
            NSApp.activate(ignoringOtherApps: true)
        }

        try? await Task.sleep(nanoseconds: updateCheckDelay)
        let hasUpdate = await updateService.checkAsync()
        if hasUpdate {
            await MainActor.run {
                let updateWindow = UpdateWindowController(updateService: updateService)
                updateWindow.window?.center()
                updateWindow.showWindow(nil)
            }
        }
    }

    func waitForHealth(baseUrl: String, timeoutSeconds: Int) async -> Bool {
        guard let healthURL = URL(string: "\(baseUrl)/api/health") else { return false }
        let deadline = Date().addingTimeInterval(Double(timeoutSeconds))
        while Date() < deadline {
            if let (_, response) = try? await healthCheckSession.data(from: healthURL),
               let http = response as? HTTPURLResponse,
               (200...503).contains(http.statusCode) {
                return true
            }
            try? await Task.sleep(nanoseconds: healthCheckRetryDelay)
        }
        return false
    }

    func checkVersionCompatibility(baseUrl: String) async -> Bool {
        let version = versionProvider()
        guard let url = URL(string: "\(baseUrl)/api/health/version?version=\(version)") else { return true }
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue(version, forHTTPHeaderField: "x-api-version")
        guard let (_, response) = try? await healthCheckSession.data(for: request),
              let http = response as? HTTPURLResponse else { return true }
        return http.statusCode != 400
    }

    var terminateDelay: Double = 0.5

    @MainActor
    func showErrorAndQuit(_ message: String) async {
        showError(message)
        let term = terminate
        let delay = terminateDelay
        DispatchQueue.main.asyncAfter(deadline: .now() + delay) { term() }
    }

    // MARK: - Shutdown

    @MainActor
    func beginTermination() {
        windowManager.closeAll()
        Task.detached { [weak self] in
            self?.fileWatcherService.stop()
            self?.backendService.stop()
            await MainActor.run {
                NSApplication.shared.reply(toApplicationShouldTerminate: true)
            }
        }
    }

    // MARK: - Mode switching

    func switchToLocalMode() async {
        if backendService.isRunning {
            await MainActor.run { windowManager.reopenAll() }
            return
        }
        let port = PortFinder.findFreePort()
        guard port > 0 else {
            await showErrorAndQuit("Could not find a free port to start the backend.")
            return
        }
        localPort = port
        await MainActor.run { windowManager.setLocalPort(port) }
        do {
            try backendService.start(port: port)
        } catch {
            await showErrorAndQuit("Failed to start the backend: \(error.localizedDescription)")
            return
        }
        await splashStatus("Waiting for backend…")
        let ready = await waitForHealth(baseUrl: "http://localhost:\(port)", timeoutSeconds: healthCheckTimeoutSeconds)
        guard ready else {
            await showErrorAndQuit("Backend did not start within 60 seconds.")
            return
        }
        await MainActor.run { windowManager.reopenAll() }
    }

    // MARK: - URL actions

    func openDocs() {
        urlOpener(Self.docsURL)
    }

    func openReleases() {
        urlOpener(Self.releasesURL)
    }
}

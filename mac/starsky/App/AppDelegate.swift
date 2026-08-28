import AppKit
import OSLog

class AppDelegate: NSObject, NSApplicationDelegate {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "AppDelegate")

    private var fileLogger: DailyFileLogger!
    private var settingsService: SettingsService!
    private var navigationService: NavigationService!
    private var routePersistenceService: RoutePersistenceService!
    private var backendService: BackendService!
    private var fileWatcherService: FileWatcherService!
    private var fileDownloadService: FileDownloadService!
    private var remoteUrlValidator: RemoteUrlValidator!
    private var updateService: UpdateService!
    private var windowManager: WindowManager!

    private var splash: SplashWindowController?
    private var settingsWindowController: SettingsWindowController?
    private var localPort: Int = 0

    func applicationDidFinishLaunching(_ notification: Notification) {
        guard ProcessInfo.processInfo.environment["XCTestConfigurationFilePath"] == nil else { return }
        NSLog("[app] applicationDidFinishLaunching")
        try? "[app] start".write(to: URL(fileURLWithPath: "/tmp/starsky-trace.txt"), atomically: false, encoding: .utf8)
        do {
            try ApplicationPaths.ensureDirectories()
        } catch {
            logger.error("Failed to create app directories: \(error.localizedDescription)")
        }

        fileLogger = DailyFileLogger()
        settingsService = SettingsService()
        settingsService.load()

        navigationService = NavigationService(settings: settingsService)
        routePersistenceService = RoutePersistenceService(settingsService: settingsService)
        backendService = BackendService(fileLogger: fileLogger)
        fileWatcherService = FileWatcherService(fileLogger: fileLogger)
        fileDownloadService = FileDownloadService(fileLogger: fileLogger)
        remoteUrlValidator = RemoteUrlValidator()
        updateService = UpdateService(settingsService: settingsService)
        windowManager = WindowManager(
            settingsService: settingsService,
            routePersistenceService: routePersistenceService,
            navigationService: navigationService,
            fileDownloadService: fileDownloadService,
            fileLogger: fileLogger
        )

        buildMenu()

        splash = SplashWindowController()
        splash?.showWindow(nil)
        NSApp.activate(ignoringOtherApps: true)

        Task {
            await startup()
        }
    }

    private func startup() async {
        switch settingsService.current.mode {
        case .local:
            await startLocalMode()
        case .remote:
            await startRemoteMode()
        }
    }

    private func startLocalMode() async {
        NSLog("[startup] startLocalMode begin")
        await MainActor.run { splash?.setStatus("Finding free port…") }
        let port = PortFinder.findFreePort()
        NSLog("[startup] port=\(port)")
        guard port > 0 else {
            await showErrorAndQuit("Could not find a free port to start the backend.")
            return
        }
        localPort = port
        windowManager.setLocalPort(port)

        await MainActor.run { splash?.setStatus("Starting backend…") }
        NSLog("[startup] launching backend")
        do {
            try backendService.start(port: port)
            NSLog("[startup] backend launched")
        } catch {
            NSLog("[startup] backend launch error: \(error)")
            await showErrorAndQuit("Failed to start the backend: \(error.localizedDescription)")
            return
        }

        await MainActor.run { splash?.setStatus("Waiting for backend…") }
        let baseUrl = "http://localhost:\(port)"
        NSLog("[startup] waiting for health at \(baseUrl)")
        let ready = await waitForHealth(baseUrl: baseUrl, timeoutSeconds: 60)
        NSLog("[startup] health ready=\(ready)")
        guard ready else {
            await showErrorAndQuit("Backend did not start within 60 seconds.")
            return
        }

        NSLog("[startup] finishStartup")
        await finishStartup()
    }

    private func startRemoteMode() async {
        guard !settingsService.current.remoteBaseUrl.isEmpty else {
            await showErrorAndQuit("No remote server URL configured.\nPlease set one in Settings.")
            return
        }
        await finishStartup()
    }

    private func finishStartup() async {
        fileWatcherService.start()

        await MainActor.run {
            windowManager.restoreWindows()
            splash?.close()
            splash = nil
            NSApp.activate(ignoringOtherApps: true)
        }

        try? await Task.sleep(nanoseconds: 5_000_000_000)
        let hasUpdate = await updateService.checkAsync()
        if hasUpdate {
            await MainActor.run {
                let updateWindow = UpdateWindowController(updateService: updateService)
                updateWindow.window?.center()
                updateWindow.showWindow(nil)
            }
        }
    }

    private func waitForHealth(baseUrl: String, timeoutSeconds: Int) async -> Bool {
        guard let healthURL = URL(string: "\(baseUrl)/api/health") else { return false }
        let deadline = Date().addingTimeInterval(Double(timeoutSeconds))
        while Date() < deadline {
            if let (_, response) = try? await URLSession.shared.data(from: healthURL),
               let http = response as? HTTPURLResponse,
               (200...503).contains(http.statusCode) {
                return true
            }
            try? await Task.sleep(nanoseconds: 1_000_000_000)
        }
        return false
    }

    @MainActor
    private func showErrorAndQuit(_ message: String) async {
        splash?.close()
        splash = nil
        ErrorWindowController.show(message: message)
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.5) {
            NSApplication.shared.terminate(nil)
        }
    }

    func applicationShouldTerminate(_ sender: NSApplication) -> NSApplication.TerminateReply {
        // Close windows immediately so the UI disappears before the blocking backend shutdown
        windowManager?.closeAll()

        Task.detached {
            self.fileWatcherService?.stop()
            self.backendService?.stop()
            await MainActor.run {
                NSApplication.shared.reply(toApplicationShouldTerminate: true)
            }
        }
        return .terminateLater
    }

    func applicationWillTerminate(_ notification: Notification) {
        // Intentionally empty — cleanup is done in applicationShouldTerminate
    }

    func applicationSupportsSecureRestorableState(_ app: NSApplication) -> Bool {
        true
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        false
    }

    func applicationShouldHandleReopen(_ sender: NSApplication, hasVisibleWindows flag: Bool) -> Bool {
        if !flag {
            Task { @MainActor in windowManager?.openMainWindow() }
        }
        return true
    }

    // MARK: - Menu

    private func buildMenu() {
        let mainMenu = NSMenu()

        let appMenu = NSMenuItem()
        appMenu.submenu = buildAppMenu()
        mainMenu.addItem(appMenu)

        let fileMenu = NSMenuItem()
        fileMenu.submenu = buildFileMenu()
        mainMenu.addItem(fileMenu)

        let viewMenu = NSMenuItem()
        viewMenu.submenu = buildViewMenu()
        mainMenu.addItem(viewMenu)

        let helpMenu = NSMenuItem()
        helpMenu.submenu = buildHelpMenu()
        mainMenu.addItem(helpMenu)

        NSApplication.shared.mainMenu = mainMenu
    }

    private func buildAppMenu() -> NSMenu {
        let menu = NSMenu(title: "Starsky")

        menu.addItem(NSMenuItem(title: "About Starsky", action: #selector(NSApplication.orderFrontStandardAboutPanel(_:)), keyEquivalent: ""))
        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: "Connection Settings…", action: #selector(openSettings), keyEquivalent: ","))
        menu.addItem(withTitle: "Application Settings", action: #selector(openApplicationSettings), keyEquivalent: "k")
            .keyEquivalentModifierMask = [.command, .shift]
        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: "Hide Starsky", action: #selector(NSApplication.hide(_:)), keyEquivalent: "h"))
        menu.addItem(NSMenuItem(title: "Quit Starsky", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q"))
        return menu
    }

    private func buildFileMenu() -> NSMenu {
        let menu = NSMenu(title: "File")
        menu.addItem(NSMenuItem(title: "New Window", action: #selector(newWindow), keyEquivalent: "n"))

        let reloadItem = NSMenuItem(title: "Reload All", action: #selector(reloadAll), keyEquivalent: "r")
        reloadItem.keyEquivalentModifierMask = [.command, .shift]
        menu.addItem(reloadItem)

        menu.addItem(NSMenuItem(title: "Edit File in Editor", action: #selector(editFileInEditor), keyEquivalent: "e"))
        return menu
    }

    private func buildViewMenu() -> NSMenu {
        let menu = NSMenu(title: "View")

        let devToolsItem = NSMenuItem(title: "Developer Tools", action: #selector(openDevTools), keyEquivalent: "i")
        devToolsItem.keyEquivalentModifierMask = [.command, .option]
        menu.addItem(devToolsItem)

        menu.addItem(NSMenuItem(title: "Open in Browser", action: #selector(openInBrowser), keyEquivalent: ""))
        return menu
    }

    private func buildHelpMenu() -> NSMenu {
        let menu = NSMenu(title: "Help")
        menu.addItem(NSMenuItem(title: "Documentation", action: #selector(openDocs), keyEquivalent: ""))
        menu.addItem(NSMenuItem(title: "Release Overview", action: #selector(openReleases), keyEquivalent: ""))
        return menu
    }

    // MARK: - Actions

    @objc private func newWindow() {
        Task { @MainActor in windowManager.openMainWindow() }
    }

    @objc private func reloadAll() {
        Task { @MainActor in windowManager.reloadAll() }
    }

    @objc private func editFileInEditor() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.editFileInEditor()
    }

    @objc private func openSettings() {
        if settingsWindowController == nil {
            settingsWindowController = SettingsWindowController(
                settingsService: settingsService,
                remoteUrlValidator: remoteUrlValidator,
                windowManager: windowManager
            )
            settingsWindowController?.window?.center()
            settingsWindowController?.onSwitchToLocal = { [weak self] in
                Task { await self?.switchToLocalMode() }
            }
            settingsWindowController?.onSwitchToRemote = { [weak self] in
                guard let self, !self.settingsService.current.remoteBaseUrl.isEmpty else { return }
                Task { @MainActor in self.windowManager.reopenAll() }
            }
        }
        settingsWindowController?.showWindow(nil)
        settingsWindowController?.window?.makeKeyAndOrderFront(nil)
    }

    private func switchToLocalMode() async {
        if backendService.isRunning {
            await MainActor.run { windowManager.reopenAll() }
            return
        }
        await MainActor.run {
            splash = SplashWindowController()
            splash?.showWindow(nil)
            NSApp.activate(ignoringOtherApps: true)
        }
        let port = PortFinder.findFreePort()
        guard port > 0 else {
            await showErrorAndQuit("Could not find a free port to start the backend.")
            return
        }
        localPort = port
        windowManager.setLocalPort(port)
        do {
            try backendService.start(port: port)
        } catch {
            await showErrorAndQuit("Failed to start the backend: \(error.localizedDescription)")
            return
        }
        await MainActor.run { splash?.setStatus("Waiting for backend…") }
        let ready = await waitForHealth(baseUrl: "http://localhost:\(port)", timeoutSeconds: 60)
        guard ready else {
            await showErrorAndQuit("Backend did not start within 60 seconds.")
            return
        }
        await MainActor.run {
            windowManager.reopenAll()
            splash?.close()
            splash = nil
        }
    }

    @objc private func openApplicationSettings() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.openApplicationSettings()
    }

    @objc private func openDevTools() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.openDevTools()
    }

    @objc private func openInBrowser() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.openInBrowser()
    }

    @objc private func openDocs() {
        NSWorkspace.shared.open(URL(string: "https://qdraw.nl/special/starsky/docs/")!)
    }

    @objc private func openReleases() {
        NSWorkspace.shared.open(URL(string: "https://github.com/qdraw/starsky/releases")!)
    }
}

extension NSMenu {
    @discardableResult
    func addItem(withTitle title: String, action: Selector?, keyEquivalent: String) -> NSMenuItem {
        let item = NSMenuItem(title: title, action: action, keyEquivalent: keyEquivalent)
        addItem(item)
        return item
    }
}

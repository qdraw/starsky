import AppKit
import OSLog

class AppDelegate: NSObject, NSApplicationDelegate {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "AppDelegate")

    private var core: AppCore?
    private var splash: SplashWindowController?
    private var settingsWindowController: SettingsWindowController?

    func applicationDidFinishLaunching(_: Notification) {
        guard ProcessInfo.processInfo.environment["XCTestConfigurationFilePath"] == nil else { return }
        NSLog("[app] applicationDidFinishLaunching")
        do {
            try ApplicationPaths.ensureDirectories()
        } catch {
            logger.error("Failed to create app directories: \(error.localizedDescription)")
        }

        let fileLogger = DailyFileLogger()
        let settingsService = SettingsService()
        settingsService.load()

        let navigationService = NavigationService(settings: settingsService)
        let routePersistenceService = RoutePersistenceService(settingsService: settingsService)
        let fileDownloadService = FileDownloadService(fileLogger: fileLogger)
        let windowManager = WindowManager(
            settingsService: settingsService,
            routePersistenceService: routePersistenceService,
            navigationService: navigationService,
            fileDownloadService: fileDownloadService,
            fileLogger: fileLogger
        )

        let splashRef = SplashWindowController()
        splash = splashRef

        core = AppCore(
            settingsService: settingsService,
            backendService: BackendService(fileLogger: fileLogger),
            fileWatcherService: FileWatcherService(fileLogger: fileLogger),
            updateService: UpdateService(settingsService: settingsService),
            windowManager: windowManager,
            terminate: { NSApplication.shared.terminate(nil) },
            showError: { ErrorWindowController.show(message: $0) },
            urlOpener: { NSWorkspace.shared.open($0) },
            splashStatus: { [weak splashRef] status in splashRef?.setStatus(status) }
        )

        buildMenu()
        splash?.showWindow(nil)
        NSApp.activate(ignoringOtherApps: true)

        Task {
            await core?.startup()
            await MainActor.run {
                self.splash?.close()
                self.splash = nil
                NSApp.activate(ignoringOtherApps: true)
            }
        }
    }

    func applicationShouldTerminate(_: NSApplication) -> NSApplication.TerminateReply {
        core?.beginTermination()
        return .terminateLater
    }

    func applicationWillTerminate(_: Notification) {
        // Intentionally empty — cleanup is done in applicationShouldTerminate
    }

    func applicationSupportsSecureRestorableState(_: NSApplication) -> Bool {
        true
    }

    func applicationShouldTerminateAfterLastWindowClosed(_: NSApplication) -> Bool {
        false
    }

    func applicationShouldHandleReopen(_: NSApplication, hasVisibleWindows flag: Bool) -> Bool {
        if !flag {
            Task { @MainActor in core?.windowManager.openMainWindow() }
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

        let editMenu = NSMenuItem()
        editMenu.submenu = buildEditMenu()
        mainMenu.addItem(editMenu)

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
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.app.about", comment: ""), action: #selector(NSApplication.orderFrontStandardAboutPanel(_:)), keyEquivalent: ""))
        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.app.connectionSettings", comment: ""), action: #selector(openSettings), keyEquivalent: ","))
        menu.addItem(withTitle: NSLocalizedString("menu.app.applicationSettings", comment: ""), action: #selector(openApplicationSettings), keyEquivalent: "k")
            .keyEquivalentModifierMask = [.command, .shift]
        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.app.hide", comment: ""), action: #selector(NSApplication.hide(_:)), keyEquivalent: "h"))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.app.quit", comment: ""), action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q"))
        return menu
    }

    private func buildFileMenu() -> NSMenu {
        let menu = NSMenu(title: NSLocalizedString("menu.file.title", comment: ""))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.file.newWindow", comment: ""), action: #selector(newWindow), keyEquivalent: "n"))
        let reloadItem = NSMenuItem(title: NSLocalizedString("menu.file.reloadAll", comment: ""), action: #selector(reloadAll), keyEquivalent: "r")
        reloadItem.keyEquivalentModifierMask = [.command, .shift]
        menu.addItem(reloadItem)
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.file.editFileInEditor", comment: ""), action: #selector(editFileInEditor), keyEquivalent: "e"))
        return menu
    }

    private func buildEditMenu() -> NSMenu {
        let menu = NSMenu(title: NSLocalizedString("menu.edit.title", comment: ""))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.edit.undo", comment: ""), action: Selector(("undo:")), keyEquivalent: "z"))
        let redo = NSMenuItem(title: NSLocalizedString("menu.edit.redo", comment: ""), action: Selector(("redo:")), keyEquivalent: "z")
        redo.keyEquivalentModifierMask = [.command, .shift]
        menu.addItem(redo)
        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.edit.cut", comment: ""), action: #selector(NSText.cut(_:)), keyEquivalent: "x"))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.edit.copy", comment: ""), action: #selector(NSText.copy(_:)), keyEquivalent: "c"))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.edit.paste", comment: ""), action: #selector(NSText.paste(_:)), keyEquivalent: "v"))
        menu.addItem(.separator())
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.edit.selectAll", comment: ""), action: #selector(NSText.selectAll(_:)), keyEquivalent: "a"))
        return menu
    }

    private func buildViewMenu() -> NSMenu {
        let menu = NSMenu(title: NSLocalizedString("menu.view.title", comment: ""))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.view.actualSize", comment: ""), action: #selector(actualSize), keyEquivalent: "0"))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.view.zoomIn", comment: ""), action: #selector(zoomIn), keyEquivalent: "="))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.view.zoomOut", comment: ""), action: #selector(zoomOut), keyEquivalent: "-"))
        menu.addItem(.separator())
        let devToolsItem = NSMenuItem(title: NSLocalizedString("menu.view.developerTools", comment: ""), action: #selector(openDevTools), keyEquivalent: "i")
        devToolsItem.keyEquivalentModifierMask = [.command, .option]
        menu.addItem(devToolsItem)
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.view.openInBrowser", comment: ""), action: #selector(openInBrowser), keyEquivalent: ""))
        menu.addItem(.separator())
        let fullScreenItem = NSMenuItem(title: NSLocalizedString("menu.view.fullScreen", comment: ""), action: #selector(toggleFullScreen), keyEquivalent: "f")
        fullScreenItem.keyEquivalentModifierMask = [.command, .control]
        menu.addItem(fullScreenItem)
        return menu
    }

    private func buildHelpMenu() -> NSMenu {
        let menu = NSMenu(title: NSLocalizedString("menu.help.title", comment: ""))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.help.documentation", comment: ""), action: #selector(openDocs), keyEquivalent: ""))
        menu.addItem(NSMenuItem(title: NSLocalizedString("menu.help.releaseOverview", comment: ""), action: #selector(openReleases), keyEquivalent: ""))
        return menu
    }

    // MARK: - Actions

    @objc private func newWindow() {
        Task { @MainActor in core?.windowManager.openMainWindow() }
    }

    @objc private func reloadAll() {
        Task { @MainActor in core?.windowManager.reloadAll() }
    }

    @objc private func editFileInEditor() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.editFileInEditor()
    }

    @objc private func openSettings() {
        guard let core else { return }
        if settingsWindowController == nil {
            settingsWindowController = SettingsWindowController(
                settingsService: core.settingsService,
                remoteUrlValidator: RemoteUrlValidator(),
                windowManager: core.windowManager
            )
            settingsWindowController?.window?.center()
            settingsWindowController?.onSwitchToLocal = { [weak self] in
                Task { await self?.core?.switchToLocalMode() }
            }
            settingsWindowController?.onSwitchToRemote = { [weak core] in
                guard let core, !core.settingsService.current.remoteBaseUrl.isEmpty else { return }
                Task { @MainActor in core.windowManager.reopenAll() }
            }
        }
        settingsWindowController?.showWindow(nil)
        settingsWindowController?.window?.makeKeyAndOrderFront(nil)
    }

    @objc private func openApplicationSettings() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.openApplicationSettings()
    }

    @objc private func zoomIn() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.zoomIn()
    }

    @objc private func zoomOut() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.zoomOut()
    }

    @objc private func actualSize() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.actualSize()
    }

    @objc private func toggleFullScreen() {
        NSApp.keyWindow?.toggleFullScreen(nil)
    }

    @objc private func openDevTools() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.openDevTools()
    }

    @objc private func openInBrowser() {
        (NSApp.keyWindow?.windowController as? MainWindowController)?.openInBrowser()
    }

    @objc private func openDocs() {
        core?.openDocs()
    }

    @objc private func openReleases() {
        core?.openReleases()
    }
}

import AppKit

class WindowManager {
    private var windows: [MainWindowController] = []
    private var localPort: Int = 0
    private var isReopening = false
    private let settingsService: SettingsService
    private let routePersistenceService: RoutePersistenceService
    private let navigationService: NavigationService
    private let fileDownloadService: FileDownloadService
    private let fileLogger: DailyFileLogger

    init(
        settingsService: SettingsService,
        routePersistenceService: RoutePersistenceService,
        navigationService: NavigationService,
        fileDownloadService: FileDownloadService,
        fileLogger: DailyFileLogger
    ) {
        self.settingsService = settingsService
        self.routePersistenceService = routePersistenceService
        self.navigationService = navigationService
        self.fileDownloadService = fileDownloadService
        self.fileLogger = fileLogger
    }

    func setLocalPort(_ port: Int) {
        localPort = port
    }

    @MainActor
    func openMainWindow(route: String? = nil, geometry: SavedWindowState? = nil) {
        let index = windows.count
        let baseUrl = navigationService.getEffectiveBaseUrl(localPort: localPort)
        let startUrl = navigationService.buildStartUrl(baseUrl: baseUrl, route: route)
        let options = MainWindowOptions(
            index: index,
            startUrl: startUrl,
            baseUrl: baseUrl,
            mode: settingsService.current.mode,
            geometry: geometry,
            navigationService: navigationService,
            routePersistenceService: routePersistenceService,
            fileDownloadService: fileDownloadService,
            windowManager: self,
            fileLogger: fileLogger
        )
        let controller = MainWindowController(options: options)
        windows.append(controller)

        let cascadeOffset = CGFloat(index) * 24
        let resolved = WindowManager.resolveGeometry(geometry, offset: cascadeOffset)

        controller.window?.setFrame(
            NSRect(x: resolved.x, y: resolved.y, width: resolved.width, height: resolved.height),
            display: false
        )
        if resolved.isMaximized {
            controller.window?.zoom(nil)
        }
        controller.showWindow(nil)
    }

    // Maximized windows are always valid — macOS snaps them to the nearest screen.
    // For normal windows, require that at least 100 px of the title bar is reachable
    // on any connected screen so the user can grab and move the window.
    static func isOnScreen(_ state: SavedWindowState) -> Bool {
        if state.isMaximized { return true }
        if state.width < 200 || state.height < 100 { return false }

        let minVisible: CGFloat = 100
        let winLeft   = CGFloat(state.x)
        let winTop    = CGFloat(state.y)
        let winRight  = winLeft + CGFloat(state.width)
        let winBottom = winTop  + CGFloat(state.height)

        return NSScreen.screens.contains { screen in
            let f = screen.frame
            return winRight  > f.minX + minVisible
                && winLeft   < f.maxX - minVisible
                && winBottom > f.minY
                && winTop    < f.maxY
        }
    }

    static func resolveGeometry(_ geometry: SavedWindowState?, offset: CGFloat) -> SavedWindowState {
        if let g = geometry, isOnScreen(g) {
            var resolved = g
            resolved.x += Double(offset)
            return resolved
        }
        return SavedWindowState(
            route: geometry?.route ?? "?f=/",
            x: 100 + Double(offset),
            y: 100,
            width: 1200,
            height: 800,
            isMaximized: false
        )
    }

    @MainActor
    func restoreWindows() {
        let routes = routePersistenceService.getRoutes()
        if routes.isEmpty {
            openMainWindow()
        } else {
            for state in routes {
                openMainWindow(route: state.route, geometry: state)
            }
        }
    }

    @MainActor
    func closeAll() {
        for controller in windows {
            controller.window?.close()
        }
        windows.removeAll()
    }

    @MainActor
    func openMainWindow(route: String?) {
        openMainWindow(route: route, geometry: nil)
    }

    @MainActor
    func reopenAll() {
        isReopening = true
        routePersistenceService.clearAll()
        closeAll()
        openMainWindow()
        isReopening = false
    }

    @MainActor
    func reloadAll() {
        for controller in windows {
            controller.reload()
        }
    }

    @MainActor
    func remove(controller: MainWindowController) {
        windows.removeAll { $0 === controller }
        if windows.isEmpty && !isReopening {
            NSApplication.shared.terminate(nil)
        }
    }
}


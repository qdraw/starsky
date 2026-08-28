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
        let x = (geometry?.x ?? 100) + Double(cascadeOffset > 0 ? cascadeOffset : 0)
        let y = (geometry?.y ?? 100)
        let w = geometry?.width ?? 1200
        let h = geometry?.height ?? 800

        controller.window?.setFrame(
            NSRect(x: x, y: y, width: w, height: h),
            display: false
        )
        if geometry?.isMaximized == true {
            controller.window?.zoom(nil)
        }
        controller.showWindow(nil)
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


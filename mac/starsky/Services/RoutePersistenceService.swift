import Foundation

class RoutePersistenceService {
    private let settingsService: SettingsService

    init(settingsService: SettingsService) {
        self.settingsService = settingsService
    }

    func getRoutes() -> [SavedWindowState] {
        settingsService.current.windows
    }

    func saveRoute(index: Int, route: String, geometry: SavedWindowState? = nil) {
        var settings = settingsService.current
        while settings.windows.count <= index {
            settings.windows.append(SavedWindowState())
        }
        settings.windows[index].route = route
        if let g = geometry {
            settings.windows[index].x = g.x
            settings.windows[index].y = g.y
            settings.windows[index].width = g.width
            settings.windows[index].height = g.height
            settings.windows[index].isMaximized = g.isMaximized
        }
        settingsService.save(settings)
    }

    func removeRoute(index: Int) {
        var settings = settingsService.current
        guard index < settings.windows.count else { return }
        settings.windows.remove(at: index)
        settingsService.save(settings)
    }

    func clearAll() {
        var settings = settingsService.current
        settings.windows = []
        settingsService.save(settings)
    }
}

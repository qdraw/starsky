import Foundation

struct MainWindowOptions {
    let index: Int
    let startUrl: String
    let baseUrl: String
    let geometry: SavedWindowState?
    let navigationService: NavigationService
    let routePersistenceService: RoutePersistenceService
    let fileDownloadService: FileDownloadService
    let windowManager: WindowManager
    let fileLogger: DailyFileLogger
}

import Foundation

@MainActor
protocol WindowManagerProtocol: AnyObject {
    func openMainWindow(route: String?)
    func openMainWindow()
    func reopenAll()
    func setLocalPort(_ port: Int)
    func restoreWindows()
    func closeAll()
    func reloadAll()
}

// Default no-op implementations so existing conformers only need to add what they use.
extension WindowManagerProtocol {
    func openMainWindow() {
        // Default behavior: forward to the route-based API with no route.
        openMainWindow(route: nil)
    }
    func setLocalPort(_: Int) {
        // Default behavior: no-op when local backend ports are not relevant.
    }
    func restoreWindows() {
        // Default behavior: no-op when window state restoration is not supported.
    }
    func closeAll() {
        // Default behavior: no-op for managers that do not own window lifecycle.
    }
    func reloadAll() {
        // Default behavior: no-op unless window reload is supported.
    }
}

extension WindowManager: WindowManagerProtocol {}

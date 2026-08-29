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
    func openMainWindow() { openMainWindow(route: nil) }
    func setLocalPort(_: Int) {}
    func restoreWindows() {}
    func closeAll() {}
    func reloadAll() {}
}

extension WindowManager: WindowManagerProtocol {}

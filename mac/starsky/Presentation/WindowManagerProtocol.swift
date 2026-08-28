import Foundation

@MainActor
protocol WindowManagerProtocol: AnyObject {
    func openMainWindow(route: String?)
    func reopenAll()
}

extension WindowManager: WindowManagerProtocol {}

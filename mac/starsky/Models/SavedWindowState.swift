import Foundation

struct SavedWindowState: Codable {
    var route: String = "?f=/"
    var x: Double = 100
    var y: Double = 100
    var width: Double = 1200
    var height: Double = 800
    var isMaximized: Bool = false
}

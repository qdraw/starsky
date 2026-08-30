import Foundation

enum ApplicationPaths {
    static let appSupport: URL = {
        let base = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask)[0]
        return base.appendingPathComponent("starsky", isDirectory: true)
    }()

    static let caches: URL = {
        let base = FileManager.default.urls(for: .cachesDirectory, in: .userDomainMask)[0]
        return base.appendingPathComponent("starsky", isDirectory: true)
    }()

    static let settingsFile: URL = appSupport.appendingPathComponent("settings.json")
    static let appSettingsFile: URL = appSupport.appendingPathComponent("appsettings.json")
    static let appSettingsLocalFile: URL = appSupport.appendingPathComponent("appsettings.local.json")
    static let databaseFile: URL = appSupport.appendingPathComponent("starsky.db")
    static let logsDirectory: URL = appSupport.appendingPathComponent("logs", isDirectory: true)
    static let thumbnailTempFolder: URL = appSupport.appendingPathComponent("thumbnailTempFolder", isDirectory: true)
    static let tempFolder: URL = caches.appendingPathComponent("tempFolder", isDirectory: true)

    static var runtimeDirectory: URL {
        let macOSDir = Bundle.main.bundleURL
            .appendingPathComponent("Contents/MacOS", isDirectory: true)
        #if arch(arm64)
        return macOSDir.appendingPathComponent("runtime-starsky-osx-arm64", isDirectory: true)
        #else
        return macOSDir.appendingPathComponent("runtime-starsky-osx-x64", isDirectory: true)
        #endif
    }

    static func ensureDirectories() throws {
        let fm = FileManager.default
        let dirs = [appSupport, caches, logsDirectory, thumbnailTempFolder, tempFolder]
        for dir in dirs {
            try fm.createDirectory(at: dir, withIntermediateDirectories: true)
        }
    }
}

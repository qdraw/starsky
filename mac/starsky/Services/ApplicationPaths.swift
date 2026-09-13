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

    static let settingsFile: URL = {
        #if DEBUG
        return appSupport.appendingPathComponent("settings-debug.json")
        #else
        return appSupport.appendingPathComponent("settings.json")
        #endif
    }()

    // Paths shared with the backend. In MAS builds these live in the App Group container
    // so both the main app and the login item can read and write them.
    #if MAS
    static let groupContainer: URL = {
        guard let url = FileManager.default.containerURL(
            forSecurityApplicationGroupIdentifier: "group.nl.qdraw.starsky"
        ) else {
            // Fallback to app support when the group container isn't provisioned (e.g. in tests).
            return appSupport
        }
        return url
    }()

    private static let sharedBase = groupContainer
    #else
    private static let sharedBase = appSupport
    #endif

    static let appSettingsFile: URL = sharedBase.appendingPathComponent("appsettings.json")
    static let appSettingsLocalFile: URL = sharedBase.appendingPathComponent("appsettings.local.json")
    static let databaseFile: URL = sharedBase.appendingPathComponent("starsky.db")
    static let logsDirectory: URL = sharedBase.appendingPathComponent("logs", isDirectory: true)
    static let thumbnailTempFolder: URL = sharedBase.appendingPathComponent("thumbnailTempFolder", isDirectory: true)

    #if MAS
    static let tempFolder: URL = sharedBase.appendingPathComponent("tmp", isDirectory: true)
    static let backendConfigFile: URL = sharedBase.appendingPathComponent("backend-config.json")
    static let bookmarksDirectory: URL = sharedBase.appendingPathComponent("bookmarks", isDirectory: true)
    #else
    static let tempFolder: URL = caches.appendingPathComponent("tempFolder", isDirectory: true)
    #endif

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
        #if MAS
        let dirs = [appSupport, groupContainer, logsDirectory, thumbnailTempFolder, tempFolder, bookmarksDirectory]
        #else
        let dirs = [appSupport, caches, logsDirectory, thumbnailTempFolder, tempFolder]
        #endif
        for dir in dirs {
            try fm.createDirectory(at: dir, withIntermediateDirectories: true)
        }
    }
}

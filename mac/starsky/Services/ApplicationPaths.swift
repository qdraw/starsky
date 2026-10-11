import Foundation

enum ApplicationPaths {
    static let appSupport: URL = {
        let base = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask)[0]
        return base.appendingPathComponent("starsky", isDirectory: true)
    }()

    // Swift-only desktop settings file — never shared with the backend.
    static let settingsFile: URL = {
        #if DEBUG
        return appSupport.appendingPathComponent("settings-debug.json")
        #else
        return appSupport.appendingPathComponent("settings.json")
        #endif
    }()

    // App Group container shared between the main app and the backend process (login item
    // in MAS builds, child process in Developer ID builds). Falls back to appSupport when
    // the entitlement is not provisioned, e.g. in XCTest runs or ad-hoc local builds.
    static let groupContainer: URL = {
        guard let url = FileManager.default.containerURL(
            forSecurityApplicationGroupIdentifier: "group.nl.qdraw.starsky"
        ) else {
            return appSupport
        }
        return url
    }()

    private static let sharedBase = groupContainer

    static let appSettingsFile: URL = sharedBase.appendingPathComponent("appsettings.json")
    static let appSettingsLocalFile: URL = sharedBase.appendingPathComponent("appsettings.local.json")
    static let databaseFile: URL = sharedBase.appendingPathComponent("starsky.db")
    static let logsDirectory: URL = sharedBase.appendingPathComponent("logs", isDirectory: true)
    static let thumbnailTempFolder: URL = sharedBase.appendingPathComponent("thumbnailTempFolder", isDirectory: true)
    static let tempFolder: URL = sharedBase.appendingPathComponent("tmp", isDirectory: true)
    static let bookmarksDirectory: URL = sharedBase.appendingPathComponent("bookmarks", isDirectory: true)

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
        for dir in [appSupport, groupContainer, logsDirectory, thumbnailTempFolder,
                    tempFolder, bookmarksDirectory] {
            try fm.createDirectory(at: dir, withIntermediateDirectories: true)
        }
    }
}

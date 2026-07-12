import Foundation

var launchedProcess: Process? // keep a strong reference so it doesn't get deallocated
// Keep a reference to the started security-scoped URL so we can stop access when process exits
var launchedBookmarkURL: URL? = nil

func electronCacheLocation() -> String {
    // macOS default app data location used by the Electron app
    let home = NSHomeDirectory()
    return "\(home)/Library/Application Support/starsky"
    // "$HOME/Library/Containers/nl.qdraw.starsky/Data/Library/Application Support/starsky"
}

func appSettingsPath() -> String {
    return (electronCacheLocation() as NSString).appendingPathComponent("appsettings.json")
}

func createTempThumbnailFolders() -> (thumbnailTempFolder: String, tempFolder: String) {
    let cache = electronCacheLocation() as NSString
    let thumbnailTempFolder = cache.appendingPathComponent("thumbnailTempFolder")
    let tempFolder = cache.appendingPathComponent("tempFolder")

    let fm = FileManager.default
    if !fm.fileExists(atPath: thumbnailTempFolder) {
        try? fm.createDirectory(atPath: thumbnailTempFolder, withIntermediateDirectories: true, attributes: nil)
    }
    if !fm.fileExists(atPath: tempFolder) {
        try? fm.createDirectory(atPath: tempFolder, withIntermediateDirectories: true, attributes: nil)
    }

    return (thumbnailTempFolder, tempFolder)
}

func isPackaged() -> Bool {
    // Heuristic: packaged when running inside a .app bundle
    return Bundle.main.bundlePath.hasSuffix(".app")
}

/// Launch the Starsky binary in the background and return the selected port if successful.
/// Returns nil on failure.
func runBinary() -> UInt16? {
    #if arch(arm64)
    let binaryName = "osx-arm64/starsky"
    #elseif arch(x86_64)
    let binaryName = "osx-x64/starsky"
    #else
    print("Unsupported architecture")
    return nil
    #endif

    guard let binaryPath = Bundle.main.path(forResource: binaryName, ofType: nil) else {
        print("Binary not found: \(binaryName)")
        return nil
    }

    guard let port = getFreePort() else {
        print("Failed to get free port")
        return nil
    }

    // Prepare default folders and paths (matches Electron app defaults)
    let createTempThumbnailFolderResult = createTempThumbnailFolders()
    let appSettingsPath = (electronCacheLocation() as NSString).appendingPathComponent("appsettings.json")
    let appSettingsLocalPath = (electronCacheLocation() as NSString).appendingPathComponent("appsettings.local.json")
    let databaseConnection = "Data Source=\((electronCacheLocation() as NSString).appendingPathComponent("starsky.db"))"

    // Build environment variables
    var env = ProcessInfo.processInfo.environment // inherit current environment
    env["ASPNETCORE_URLS"] = "http://localhost:\(port)"
    env["app__thumbnailTempFolder"] = createTempThumbnailFolderResult.thumbnailTempFolder
    env["app__tempFolder"] = createTempThumbnailFolderResult.tempFolder
    env["app__appsettingspath"] = appSettingsPath
    env["app__appsettingslocalpath"] = appSettingsLocalPath
    env["app__NoAccountLocalhost"] = "true"
    env["app__UseLocalDesktop"] = "true"
    env["app__databaseConnection"] = databaseConnection
    env["app__ThumbnailGenerationIntervalInMinutes"] = isPackaged() ? "300" : "-1"
    env["app__AccountRegisterDefaultRole"] = "Administrator"

    // Read StorageFolder and StorageFolderToken from appsettings.json (if present) and add to environment
    let storageSettings = AppSettingsReader.getStorageFolderAndTokenFromAppSettings()
    if let storageFolder = storageSettings.storageFolder {
        env["app__StorageFolder"] = storageFolder
        print("Using StorageFolder from appsettings: \(storageFolder)")
    }
    if let storageToken = storageSettings.storageFolderToken {
        env["app__StorageFolderToken"] = storageToken
        print("Using StorageFolderToken from appsettings: \(storageToken)")
    }

    // Wire AppSettingsReader to StorageBookmarkManager: ensure bookmark exists before starting child process
    if let folder = storageSettings.storageFolder, let token = storageSettings.storageFolderToken {
        do {
            // Ensure bookmark is saved (create if missing)
            try StorageBookmarkManager.saveBookmark(forFolderPath: folder, token: token)
            print("Ensured bookmark is saved for storage folder")
            
            // Start security-scoped access
            let url = try StorageBookmarkManager.startAccessForToken(token)
            launchedBookmarkURL = url
            print("Started security-scoped access for storage folder: \(url.path)")
        } catch {
            NSLog("[runBinary] failed to wire AppSettingsReader to StorageBookmarkManager: %@", String(describing: error))
            // Continue launching; bookmark access failure is logged but not fatal here
        }
    }

    let process = Process()
    process.executableURL = URL(fileURLWithPath: binaryPath)
    process.arguments = ["--port", "\(port)"]
    process.environment = env

    // When the process terminates, stop the security-scoped access if we started it
    process.terminationHandler = { _ in
        if let url = launchedBookmarkURL {
            StorageBookmarkManager.stopAccess(url)
            launchedBookmarkURL = nil
            print("Stopped security-scoped access for storage folder")
        }
    }

    print("free port ", port)
    print("env -> \(env)")

    do {
        try process.run()
        // keep a strong reference so the Process isn't deallocated
        launchedProcess = process
        // Do not wait here; return the port so the caller can poll for readiness
        return port
    } catch {
        print("Failed to run binary: \(error)")
        // If we started the bookmark, stop it to avoid leaking access
        if let url = launchedBookmarkURL {
            StorageBookmarkManager.stopAccess(url)
            launchedBookmarkURL = nil
        }
        return nil
    }
}

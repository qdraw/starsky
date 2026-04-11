import Foundation

var launchedProcess: Process? // keep a strong reference so it doesn't get deallocated

func electronCacheLocation() -> String {
    // macOS default app data location used by the Electron app
    let home = NSHomeDirectory()
    return "\(home)/Library/Application Support/starsky"
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

    let process = Process()
    process.executableURL = URL(fileURLWithPath: binaryPath)
    process.arguments = ["--port", "\(port)"]
    process.environment = env

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
        return nil
    }
}

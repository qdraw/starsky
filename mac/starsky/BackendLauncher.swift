import Foundation

protocol BackendLaunching {
    func launch() -> UInt16?
    func terminate()
}

/// Responsible for launching the Starsky backend binary and returning the chosen port.
final class BackendLauncher: BackendLaunching {
    private(set) var launchedProcess: Process?
    // Keep reference to the URL for security-scoped access so we can stop it later
    private var launchedBookmarkURL: URL?

    func launch() -> UInt16? {
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

        let createTempThumbnailFolderResult = createTempThumbnailFolders()
        let appSettingsPath = (electronCacheLocation() as NSString).appendingPathComponent("appsettings.json")
        let appSettingsLocalPath = (electronCacheLocation() as NSString).appendingPathComponent("appsettings.local.json")
        let databaseConnection = "Data Source=\((electronCacheLocation() as NSString).appendingPathComponent("starsky.db"))"

        var env = ProcessInfo.processInfo.environment
        env["ASPNETCORE_URLS"] = "http://localhost:\(port)"
        env["app__thumbnailTempFolder"] = createTempThumbnailFolderResult.thumbnailTempFolder
        env["app__tempFolder"] = createTempThumbnailFolderResult.tempFolder
        env["app__appsettingspath"] = appSettingsPath
        env["app__appsettingslocalpath"] = appSettingsLocalPath
        env["app__NoAccountLocalhost"] = "true"
        env["app__UseLocalDesktop"] = "true"
        env["app__databaseConnection"] = databaseConnection
        env["app__ThumbnailGenerationIntervalInMinutes"] =  "300"
        env["app__AccountRegisterDefaultRole"] = "Administrator"

        let process = Process()
        process.executableURL = URL(fileURLWithPath: binaryPath)
        process.arguments = ["--port", "\(port)"]
        process.environment = env

        // Stop access when child process terminates
        process.terminationHandler = { [weak self] _ in
            guard let self = self else { return }
            if let url = self.launchedBookmarkURL {
                StorageBookmarkManager.stopAccess(url)
                self.launchedBookmarkURL = nil
            }
        }

        do {
            try process.run()
            launchedProcess = process
            return port
        } catch {
            print("Failed to run binary: \(error)")
            return nil
        }
    }

    func terminate() {
        launchedProcess?.terminate()
        launchedProcess = nil
    }
}

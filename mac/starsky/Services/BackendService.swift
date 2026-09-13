import Foundation
import OSLog
#if MAS
import ServiceManagement
#endif

class BackendService: @unchecked Sendable {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "BackendService")
    private let fileLogger: DailyFileLogger

    #if !MAS
    private var process: Process?
    private var isShuttingDown = false
    private var hasRestarted = false
    private var hasTriedQuarantineClear = false
    private var currentExePath: String?
    private var currentPort: Int = 0

    private let xattrPath: String
    private let codesignPath: String

    var sigtermTimeout: TimeInterval = 5
    var sigintTimeout: TimeInterval = 2
    var restartDelay: TimeInterval = 2
    #endif

    init(fileLogger: DailyFileLogger,
         xattrPath: String = "/usr/bin/xattr",
         codesignPath: String = "/usr/bin/codesign")
    {
        self.fileLogger = fileLogger
        #if !MAS
        self.xattrPath = xattrPath
        self.codesignPath = codesignPath
        #endif
    }

    // MARK: - MAS path (SMAppService login item)

    #if MAS
    private let loginItemIdentifier = "nl.qdraw.starsky.backend"
    private var loginItem: SMAppService { .loginItem(identifier: loginItemIdentifier) }

    var isRunning: Bool { loginItem.status == .enabled }

    func start(port: Int) throws {
        writePortToAppSettings(port: port)
        // Unregister any stale instance before registering fresh.
        try? loginItem.unregister()
        do {
            try loginItem.register()
            logger.info("Backend login item registered on port \(port)")
            fileLogger.info("Backend login item registered on port \(port)", category: "BackendService")
        } catch {
            if loginItem.status == .requiresApproval {
                throw BackendError.requiresUserApproval
            }
            throw BackendError.loginItemRegistrationFailed(error)
        }
    }

    func stop() {
        try? loginItem.unregister()
        logger.info("Backend login item unregistered")
        fileLogger.info("Backend login item unregistered", category: "BackendService")
    }

    func beginShutdown() { stop() }
    func forceStop() { stop() }

    // Merges the Kestrel port into the App Group container's appsettings.json.
    // The .NET backend reads this file on startup; PortProgramHelper detects
    // the Kestrel endpoint and skips its own URL override logic.
    private func writePortToAppSettings(port: Int) {
        let file = ApplicationPaths.appSettingsFile
        var root: [String: Any] = [:]
        if let data = try? Data(contentsOf: file),
           let existing = try? JSONSerialization.jsonObject(with: data) as? [String: Any] {
            root = existing
        }
        var kestrel = root["Kestrel"] as? [String: Any] ?? [:]
        var endpoints = kestrel["Endpoints"] as? [String: Any] ?? [:]
        endpoints["Http"] = ["Url": "http://localhost:\(port)"]
        kestrel["Endpoints"] = endpoints
        root["Kestrel"] = kestrel
        guard let data = try? JSONSerialization.data(
            withJSONObject: root, options: [.prettyPrinted, .sortedKeys]) else { return }
        try? data.write(to: file, options: .atomic)
    }

    // MARK: - Non-MAS path (Foundation.Process)

    #else

    var isRunning: Bool { process?.isRunning ?? false }

    func start(port: Int) throws {
        currentPort = port
        try launch(port: port)
    }

    // Returns the environment dictionary the backend process needs.
    // Uses ApplicationPaths.* which always resolves to the App Group container,
    // ensuring the same paths are used regardless of distribution channel.
    static func buildEnvironment(port: Int) -> [String: String] {
        var env = ProcessInfo.processInfo.environment
        env["ASPNETCORE_URLS"] = "http://localhost:\(port)"
        env["STARSKY_APP_GROUP"] = "group.nl.qdraw.starsky"
        env["app__appsettingspath"] = ApplicationPaths.appSettingsFile.path
        env["app__appsettingslocalpath"] = ApplicationPaths.appSettingsLocalFile.path
        env["app__databaseConnection"] = "Data Source=\(ApplicationPaths.databaseFile.path)"
        env["app__tempFolder"] = ApplicationPaths.tempFolder.path + "/"
        env["app__thumbnailTempFolder"] = ApplicationPaths.thumbnailTempFolder.path + "/"
        env["app__NoAccountLocalhost"] = "true"
        env["app__UseLocalDesktop"] = "true"
        env["app__AccountRegisterDefaultRole"] = "Administrator"
        env["app__ThumbnailGenerationIntervalInMinutes"] = "300"
        env["app__Verbose"] = "false"
        return env
    }

    private func launch(port: Int) throws {
        guard let executableURL = findBackendExe() else {
            throw BackendError.executableNotFound
        }

        currentExePath = executableURL.path

        let proc = Process()
        proc.executableURL = executableURL
        proc.environment = Self.buildEnvironment(port: port)

        let pipe = Pipe()
        proc.standardOutput = pipe
        proc.standardError = pipe

        pipe.fileHandleForReading.readabilityHandler = { [weak self] handle in
            let data = handle.availableData
            if !data.isEmpty, let line = String(data: data, encoding: .utf8) {
                self?.fileLogger.info(line.trimmingCharacters(in: .newlines), category: "Backend")
            }
        }

        proc.terminationHandler = { [weak self] _ in
            self?.onProcessExited(port: port)
        }

        try proc.run()
        self.process = proc
        logger.info("Backend started on port \(port), pid \(proc.processIdentifier)")
        fileLogger.info("Backend started on port \(port)", category: "BackendService")
    }

    func stop() {
        isShuttingDown = true
        guard let proc = process, proc.isRunning else {
            logger.info("Backend stop: no running process (pid=\(self.process?.processIdentifier ?? -1), isRunning=\(self.process?.isRunning ?? false))")
            return
        }
        logger.info("Backend stop: sending SIGTERM to pid \(proc.processIdentifier)")
        proc.terminate()
        let sigtermDeadline = Date().addingTimeInterval(sigtermTimeout)
        while proc.isRunning && Date() < sigtermDeadline {
            Thread.sleep(forTimeInterval: 0.1)
        }
        if proc.isRunning {
            logger.warning("Backend stop: SIGTERM timeout, sending SIGINT to pid \(proc.processIdentifier)")
            proc.interrupt()
            let sigintDeadline = Date().addingTimeInterval(sigintTimeout)
            while proc.isRunning && Date() < sigintDeadline {
                Thread.sleep(forTimeInterval: 0.1)
            }
            if proc.isRunning {
                logger.warning("Backend stop: SIGINT timeout, sending SIGKILL to pid \(proc.processIdentifier)")
                kill(proc.processIdentifier, SIGKILL)
            }
        }
        process = nil
        logger.info("Backend stopped")
        fileLogger.info("Backend stopped", category: "BackendService")
    }

    // Marks shutdown intent and sends SIGTERM without waiting.
    func beginShutdown() {
        isShuttingDown = true
        guard let proc = process, proc.isRunning else { return }
        logger.info("Backend beginShutdown: sending SIGTERM to pid \(proc.processIdentifier)")
        proc.terminate()
    }

    // Sends SIGKILL immediately and clears the process reference.
    func forceStop() {
        guard let proc = process else { return }
        if proc.isRunning {
            logger.warning("Backend forceStop: sending SIGKILL to pid \(proc.processIdentifier)")
            kill(proc.processIdentifier, SIGKILL)
        }
        process = nil
        logger.info("Backend force stopped")
        fileLogger.info("Backend force stopped", category: "BackendService")
    }

    func onProcessExited(port: Int) {
        guard !isShuttingDown, !hasRestarted else { return }
        hasRestarted = true
        if !hasTriedQuarantineClear, let path = currentExePath {
            hasTriedQuarantineClear = true
            logger.warning("Backend exited unexpectedly; clearing quarantine/signature before restart")
            fileLogger.warning("Backend exited unexpectedly; clearing quarantine/signature before restart", category: "BackendService")
            clearQuarantine(path: path)
        } else {
            logger.warning("Backend exited unexpectedly, restarting in 2 s...")
            fileLogger.warning("Backend exited unexpectedly, restarting in 2 s", category: "BackendService")
        }
        DispatchQueue.global().asyncAfter(deadline: .now() + restartDelay) { [weak self] in
            guard self?.isShuttingDown == false else { return }
            try? self?.launch(port: port)
        }
    }

    func findBackendExe() -> URL? {
        let dir = ApplicationPaths.runtimeDirectory
        let binary = dir.appendingPathComponent("starsky")
        return FileManager.default.fileExists(atPath: binary.path) ? binary : nil
    }

    private func clearQuarantine(path: String) {
        let xattr = Process()
        xattr.executableURL = URL(fileURLWithPath: xattrPath)
        xattr.arguments = ["-rd", "com.apple.quarantine", path]
        if (try? xattr.run()) != nil { xattr.waitUntilExit() }

        let codesign = Process()
        codesign.executableURL = URL(fileURLWithPath: codesignPath)
        codesign.arguments = ["--force", "--deep", "-s", "-", path]
        if (try? codesign.run()) != nil { codesign.waitUntilExit() }
        quarantineDidClear(path)
    }

    func quarantineDidClear(_: String) {
        // Intentionally no-op by default: hook in tests
    }

    #endif

    deinit {
        stop()
    }
}

enum BackendError: LocalizedError {
    case executableNotFound
    #if MAS
    case requiresUserApproval
    case loginItemRegistrationFailed(Error)
    #endif

    var errorDescription: String? {
        switch self {
        case .executableNotFound:
            return "The Starsky backend executable was not found in the application bundle."
        #if MAS
        case .requiresUserApproval:
            return "Starsky needs permission to run its background helper. Open System Settings → General → Login Items & Extensions and enable Starsky."
        case .loginItemRegistrationFailed(let e):
            return "Failed to register the Starsky backend: \(e.localizedDescription)"
        #endif
        }
    }
}

import Foundation
import OSLog

class BackendService: @unchecked Sendable {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "BackendService")
    private let fileLogger: DailyFileLogger
    private var process: Process?
    private var isShuttingDown = false
    private var hasRestarted = false
    private var currentPort: Int = 0

    private let xattrPath: String
    private let codesignPath: String

    var sigtermTimeout: TimeInterval = 5
    var sigintTimeout: TimeInterval = 2
    var restartDelay: TimeInterval = 2

    init(fileLogger: DailyFileLogger, xattrPath: String = "/usr/bin/xattr", codesignPath: String = "/usr/bin/codesign") {
        self.fileLogger = fileLogger
        self.xattrPath = xattrPath
        self.codesignPath = codesignPath
    }

    var isRunning: Bool { process?.isRunning ?? false }

    func start(port: Int) throws {
        currentPort = port
        try launch(port: port)
    }

    private func launch(port: Int) throws {
        guard let executableURL = findBackendExe() else {
            throw BackendError.executableNotFound
        }

        clearQuarantine(path: executableURL.path)

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

    // Marks shutdown intent and sends SIGTERM without waiting. Call this early in the
    // termination path to give the backend time to flush; follow up with forceStop().
    func beginShutdown() {
        isShuttingDown = true
        guard let proc = process, proc.isRunning else { return }
        logger.info("Backend beginShutdown: sending SIGTERM to pid \(proc.processIdentifier)")
        proc.terminate()
    }

    // Sends SIGKILL immediately and clears the process reference. Call after a grace period
    // to ensure the backend is dead before the parent process exits.
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

    private func onProcessExited(port: Int) {
        guard !isShuttingDown, !hasRestarted else { return }
        hasRestarted = true
        logger.warning("Backend exited unexpectedly, restarting in 2 s...")
        fileLogger.warning("Backend exited unexpectedly, restarting in 2 s", category: "BackendService")
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
    }

    static func buildEnvironment(port: Int) -> [String: String] {
        var env = ProcessInfo.processInfo.environment
        let appSupport = ApplicationPaths.appSupport.path
        let caches = ApplicationPaths.caches.path

        env["ASPNETCORE_URLS"] = "http://localhost:\(port)"
        env["app__appsettingspath"] = "\(appSupport)/appsettings.json"
        env["app__appsettingslocalpath"] = "\(appSupport)/appsettings.local.json"
        env["app__databaseConnection"] = "Data Source=\(appSupport)/starsky.db"
        env["app__tempFolder"] = "\(caches)/tempFolder/"
        env["app__thumbnailTempFolder"] = "\(appSupport)/thumbnailTempFolder/"
        env["app__NoAccountLocalhost"] = "true"
        env["app__UseLocalDesktop"] = "true"
        env["app__AccountRegisterDefaultRole"] = "Administrator"
        env["app__ThumbnailGenerationIntervalInMinutes"] = "300"
        env["app__Verbose"] = "false"
        return env
    }

    deinit {
        stop()
    }
}

enum BackendError: LocalizedError {
    case executableNotFound

    var errorDescription: String? {
        switch self {
        case .executableNotFound:
            return "The Starsky backend executable was not found in the application bundle."
        }
    }
}

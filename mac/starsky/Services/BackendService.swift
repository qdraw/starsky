import Foundation
import OSLog

class BackendService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "BackendService")
    private let fileLogger: DailyFileLogger
    private var process: Process?
    private var isShuttingDown = false
    private var hasRestarted = false
    private var currentPort: Int = 0

    init(fileLogger: DailyFileLogger) {
        self.fileLogger = fileLogger
    }

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
        guard let proc = process, proc.isRunning else { return }
        proc.terminate()
        let deadline = Date().addingTimeInterval(5)
        while proc.isRunning && Date() < deadline {
            Thread.sleep(forTimeInterval: 0.1)
        }
        if proc.isRunning { proc.interrupt() }
        process = nil
        logger.info("Backend stopped")
        fileLogger.info("Backend stopped", category: "BackendService")
    }

    private func onProcessExited(port: Int) {
        guard !isShuttingDown, !hasRestarted else { return }
        hasRestarted = true
        logger.warning("Backend exited unexpectedly, restarting in 2 s...")
        fileLogger.warning("Backend exited unexpectedly, restarting in 2 s", category: "BackendService")
        DispatchQueue.global().asyncAfter(deadline: .now() + 2) { [weak self] in
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
        xattr.executableURL = URL(fileURLWithPath: "/usr/bin/xattr")
        xattr.arguments = ["-rd", "com.apple.quarantine", path]
        try? xattr.run()
        xattr.waitUntilExit()

        let codesign = Process()
        codesign.executableURL = URL(fileURLWithPath: "/usr/bin/codesign")
        codesign.arguments = ["--force", "--deep", "-s", "-", path]
        try? codesign.run()
        codesign.waitUntilExit()
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

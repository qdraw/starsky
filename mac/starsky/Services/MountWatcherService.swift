import Foundation
import OSLog

// Abstracts Process execution so MountWatcherService is testable without spawning real processes.
protocol ProcessRunner {
    func run(executable: URL, arguments: [String]) async -> Int32
    func runSync(executable: URL, arguments: [String])
}

final class DefaultProcessRunner: ProcessRunner {
    func run(executable: URL, arguments: [String]) async -> Int32 {
        await withCheckedContinuation { continuation in
            let process = Process()
            process.executableURL = executable
            process.arguments = arguments
            process.standardOutput = FileHandle.nullDevice
            process.standardError = FileHandle.nullDevice
            process.terminationHandler = { p in continuation.resume(returning: p.terminationStatus) }
            do {
                try process.run()
            } catch {
                continuation.resume(returning: -1)
            }
        }
    }

    func runSync(executable: URL, arguments: [String]) {
        let process = Process()
        process.executableURL = executable
        process.arguments = arguments
        process.standardOutput = FileHandle.nullDevice
        process.standardError = FileHandle.nullDevice
        try? process.run()
        process.waitUntilExit()
    }
}

#if DEBUG
private let mountWatcherLaunchAgentName = "nl.qdraw.mountwatcher.debug"
#else
private let mountWatcherLaunchAgentName = "nl.qdraw.mountwatcher"
#endif

final class MountWatcherService: MountWatcherServiceProtocol {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "MountWatcher")
    private let processRunner: ProcessRunner
    let cliBinaryURL: URL
    let plistURL: URL
    let serviceName: String

    init(
        processRunner: ProcessRunner = DefaultProcessRunner(),
        cliBinaryURL: URL? = nil,
        plistURL: URL? = nil,
        serviceName: String? = nil
    ) {
        self.processRunner = processRunner
        let name = serviceName ?? mountWatcherLaunchAgentName
        self.serviceName = name
        self.cliBinaryURL = cliBinaryURL
            ?? ApplicationPaths.runtimeDirectory.appendingPathComponent("starskymountwatchercli")
        self.plistURL = plistURL
            ?? FileManager.default.homeDirectoryForCurrentUser
                .appendingPathComponent("Library/LaunchAgents/\(name).plist")
    }

    func enable() async -> Bool {
        logger.info("Enabling MountWatcher service (\(self.serviceName, privacy: .public))")
        guard FileManager.default.fileExists(atPath: cliBinaryURL.path) else {
            logger.error("Enable failed: CLI binary not found at \(self.cliBinaryURL.path, privacy: .public)")
            return false
        }
        let code = await processRunner.run(executable: cliBinaryURL, arguments: ["--install"])
        if code != 0 {
            logger.error("Enable failed: --install exited with code \(code, privacy: .public)")
            return false
        }
        logger.info("MountWatcher service enabled successfully")
        return true
    }

    func disable() async -> Bool {
        logger.info("Disabling MountWatcher service (\(self.serviceName, privacy: .public))")
        guard FileManager.default.fileExists(atPath: cliBinaryURL.path) else {
            let plistPresent = FileManager.default.fileExists(atPath: plistURL.path)
            logger.warning("Disable: CLI binary not found at \(self.cliBinaryURL.path, privacy: .public); plist present: \(plistPresent, privacy: .public)")
            // CLI missing — report success only if the plist is also gone.
            return !plistPresent
        }
        let code = await processRunner.run(executable: cliBinaryURL, arguments: ["--uninstall"])
        if code != 0 {
            logger.error("Disable failed: --uninstall exited with code \(code, privacy: .public)")
            return false
        }
        logger.info("MountWatcher service disabled successfully")
        return true
    }

    func status() async -> MountWatcherStatus {
        guard FileManager.default.fileExists(atPath: plistURL.path) else {
            return .notInstalled
        }
        let launchctl = URL(fileURLWithPath: "/bin/launchctl")
        let code = await processRunner.run(executable: launchctl, arguments: ["list", serviceName])
        let result: MountWatcherStatus = code == 0 ? .running : .stopped
        logger.debug("Status check: launchctl list exited \(code, privacy: .public) → \(result.displayString, privacy: .public)")
        return result
    }

    func stopSync() {
        logger.info("Stopping MountWatcher synchronously (pre-update or termination)")
        guard FileManager.default.fileExists(atPath: cliBinaryURL.path) else {
            logger.warning("stopSync: CLI binary not found at \(self.cliBinaryURL.path, privacy: .public); skipping")
            return
        }
        processRunner.runSync(executable: cliBinaryURL, arguments: ["--uninstall"])
        logger.info("MountWatcher stopSync completed")
    }
}

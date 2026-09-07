import Foundation

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
        guard FileManager.default.fileExists(atPath: cliBinaryURL.path) else { return false }
        let code = await processRunner.run(executable: cliBinaryURL, arguments: ["--install"])
        return code == 0
    }

    func disable() async -> Bool {
        guard FileManager.default.fileExists(atPath: cliBinaryURL.path) else {
            // CLI missing — report success only if the plist is also gone.
            return !FileManager.default.fileExists(atPath: plistURL.path)
        }
        let code = await processRunner.run(executable: cliBinaryURL, arguments: ["--uninstall"])
        return code == 0
    }

    func status() async -> MountWatcherStatus {
        guard FileManager.default.fileExists(atPath: plistURL.path) else {
            return .notInstalled
        }
        let launchctl = URL(fileURLWithPath: "/bin/launchctl")
        let code = await processRunner.run(executable: launchctl, arguments: ["list", serviceName])
        return code == 0 ? .running : .stopped
    }

    func stopSync() {
        guard FileManager.default.fileExists(atPath: cliBinaryURL.path) else { return }
        processRunner.runSync(executable: cliBinaryURL, arguments: ["--uninstall"])
    }
}

import Foundation

class DailyFileLogger {
    private let logsDirectory: URL
    private let lock = NSLock()
    private let dateFormatter: DateFormatter
    private let fileDateFormatter: DateFormatter
    private var currentLogFile: URL?
    private let isDebugBuild: Bool

    static let symlinkName = "starsky-latest.log"
    static let debugSuffix = "-debug"

    init(logsDirectory: URL = ApplicationPaths.logsDirectory, isDebugBuild: Bool = {
        #if DEBUG
        return true
        #else
        return false
        #endif
    }()) {
        self.logsDirectory = logsDirectory
        self.isDebugBuild = isDebugBuild

        dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd HH:mm:ss"
        dateFormatter.locale = Locale(identifier: "en_US_POSIX")

        fileDateFormatter = DateFormatter()
        fileDateFormatter.dateFormat = "yyyy-MM-dd"
        fileDateFormatter.locale = Locale(identifier: "en_US_POSIX")
    }

    func log(level: String, category: String, message: String, error: Error? = nil) {
        lock.lock()
        defer { lock.unlock() }

        let now = Date()
        let timestamp = dateFormatter.string(from: now)
        let dateSuffix = fileDateFormatter.string(from: now)
        let suffix = isDebugBuild ? "\(dateSuffix)\(DailyFileLogger.debugSuffix)" : dateSuffix
        let logFile = logsDirectory.appendingPathComponent("starsky-\(suffix).log")

        var line = "\(timestamp) [\(level)] \(category): \(message)\n"
        if let error = error {
            line += "\(error)\n"
        }

        guard let data = line.data(using: .utf8) else { return }
        if FileManager.default.fileExists(atPath: logFile.path) {
            guard let handle = try? FileHandle(forWritingTo: logFile) else { return }
            handle.seekToEndOfFile()
            handle.write(data)
            try? handle.close()
        } else {
            try? data.write(to: logFile, options: .atomic)
        }

        if !isDebugBuild, currentLogFile != logFile {
            updateSymlink(to: logFile)
        }
    }

    private func updateSymlink(to logFile: URL) {
        let symlink = logsDirectory.appendingPathComponent(DailyFileLogger.symlinkName)
        let fm = FileManager.default
        try? fm.removeItem(at: symlink)
        try? fm.createSymbolicLink(at: symlink, withDestinationURL: logFile)
        currentLogFile = logFile
    }

    func info(_ message: String, category: String = "App") {
        log(level: "INFO", category: category, message: message)
    }

    func warning(_ message: String, category: String = "App") {
        log(level: "WARN", category: category, message: message)
    }

    func error(_ message: String, error: Error? = nil, category: String = "App") {
        log(level: "ERROR", category: category, message: message, error: error)
    }
}

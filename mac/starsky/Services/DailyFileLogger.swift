import Foundation

class DailyFileLogger {
    private let logsDirectory: URL
    private let lock = NSLock()
    private let dateFormatter: DateFormatter
    private let fileDateFormatter: DateFormatter
    private let dateProvider: () -> Date
    private var currentDateSuffix: String?
    private let isDebugBuild: Bool

    static let latestFileName = "starsky-latest.log"
    static let latestDebugFileName = "starsky-latest-debug.log"
    static let debugSuffix = "-debug"
    static let logRetentionDays = 90

    init(
        logsDirectory: URL = ApplicationPaths.logsDirectory,
        isDebugBuild: Bool = {
            #if DEBUG
            return true
            #else
            return false
            #endif
        }(),
        dateProvider: @escaping () -> Date = { Date() }
    ) {
        self.logsDirectory = logsDirectory
        self.isDebugBuild = isDebugBuild
        self.dateProvider = dateProvider

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

        let now = dateProvider()
        let timestamp = dateFormatter.string(from: now)
        let dateSuffix = fileDateFormatter.string(from: now)

        var line = "\(timestamp) [\(level)] \(category): \(message)\n"
        if let error = error {
            line += "\(error)\n"
        }

        guard let data = line.data(using: .utf8) else { return }

        let latestName = isDebugBuild ? DailyFileLogger.latestDebugFileName : DailyFileLogger.latestFileName
        let latestFile = logsDirectory.appendingPathComponent(latestName)

        if currentDateSuffix != dateSuffix {
            archiveLatestIfNeeded(latestFile: latestFile, archiveDateSuffix: currentDateSuffix, isDebug: isDebugBuild)
            pruneOldLogs(before: now)
            currentDateSuffix = dateSuffix
        }

        appendData(data, to: latestFile)
    }

    // Archives the latest log file to a dated name before a new day begins.
    // archiveDateSuffix is nil on the first write after app launch — falls back to the file's modification date.
    private func archiveLatestIfNeeded(latestFile: URL, archiveDateSuffix: String?, isDebug: Bool) {
        let fm = FileManager.default
        guard fm.fileExists(atPath: latestFile.path) else { return }

        let dateSuffix: String
        if let s = archiveDateSuffix {
            dateSuffix = s
        } else {
            let attrs = try? fm.attributesOfItem(atPath: latestFile.path)
            let modDate = (attrs?[.modificationDate] as? Date) ?? dateProvider()
            dateSuffix = fileDateFormatter.string(from: modDate)
        }

        let archiveName = isDebug
            ? "starsky-\(dateSuffix)\(DailyFileLogger.debugSuffix).log"
            : "starsky-\(dateSuffix).log"
        let archive = logsDirectory.appendingPathComponent(archiveName)

        if !fm.fileExists(atPath: archive.path) {
            try? fm.moveItem(at: latestFile, to: archive)
        } else {
            if let latestData = try? Data(contentsOf: latestFile),
               let handle = try? FileHandle(forWritingTo: archive) {
                handle.seekToEndOfFile()
                handle.write(latestData)
                try? handle.close()
            }
            try? fm.removeItem(at: latestFile)
        }
    }

    // Deletes dated log files (starsky-YYYY-MM-DD[.log / -debug.log]) older than logRetentionDays.
    // The two "latest" files are never touched.
    private func pruneOldLogs(before now: Date) {
        let fm = FileManager.default
        guard let files = try? fm.contentsOfDirectory(at: logsDirectory, includingPropertiesForKeys: nil) else { return }
        let cutoff = Calendar(identifier: .gregorian)
            .date(byAdding: .day, value: -DailyFileLogger.logRetentionDays, to: now) ?? now

        for file in files {
            let name = file.lastPathComponent
            guard name != DailyFileLogger.latestFileName,
                  name != DailyFileLogger.latestDebugFileName,
                  name.hasSuffix(".log"),
                  name.hasPrefix("starsky-") else { continue }

            // Extract the date portion: the 10 characters after "starsky-"
            let afterPrefix = name.dropFirst("starsky-".count)
            let dateString = String(afterPrefix.prefix(10))
            guard let fileDate = fileDateFormatter.date(from: dateString),
                  fileDate < cutoff else { continue }

            try? fm.removeItem(at: file)
        }
    }

    private func appendData(_ data: Data, to file: URL) {
        if FileManager.default.fileExists(atPath: file.path) {
            guard let handle = try? FileHandle(forWritingTo: file) else { return }
            handle.seekToEndOfFile()
            handle.write(data)
            try? handle.close()
        } else {
            try? data.write(to: file, options: .atomic)
        }
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

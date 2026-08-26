import Foundation
import OSLog

class FileWatcherService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "FileWatcherService")
    private let fileLogger: DailyFileLogger
    private let watchedDirectory: URL
    private var source: DispatchSourceFileSystemObject?
    private var dirFD: Int32 = -1
    private var debounceItems: [String: DispatchWorkItem] = [:]
    private let debounceQueue = DispatchQueue(label: "nl.qdraw.starsky.filewatcher")

    init(
        fileLogger: DailyFileLogger,
        watchedDirectory: URL = ApplicationPaths.tempFolder
    ) {
        self.fileLogger = fileLogger
        self.watchedDirectory = watchedDirectory
    }

    func start() {
        let fm = FileManager.default
        try? fm.createDirectory(at: watchedDirectory, withIntermediateDirectories: true)

        let fd = open(watchedDirectory.path, O_EVTONLY)
        guard fd >= 0 else {
            logger.warning("Could not open temp folder for watching")
            return
        }
        dirFD = fd

        let watchPath = self.watchedDirectory.path

        let src = DispatchSource.makeFileSystemObjectSource(
            fileDescriptor: fd,
            eventMask: [.write, .link],
            queue: debounceQueue
        )
        src.setEventHandler { [weak self] in
            self?.handleDirectoryChange()
        }
        src.setCancelHandler { [weak self] in
            if let fd = self?.dirFD, fd >= 0 {
                close(fd)
                self?.dirFD = -1
            }
        }
        src.resume()
        source = src
        logger.info("FileWatcherService started watching \(watchPath)")
    }

    private func handleDirectoryChange() {
        let key = self.watchedDirectory.path
        debounceItems[key]?.cancel()
        let item = DispatchWorkItem { [weak self] in
            self?.onDirectoryChanged()
        }
        debounceItems[key] = item
        debounceQueue.asyncAfter(deadline: .now() + 0.5, execute: item)
    }

    private func onDirectoryChanged() {
        let dir = self.watchedDirectory
        guard let contents = try? FileManager.default.contentsOfDirectory(
            at: dir,
            includingPropertiesForKeys: nil
        ) else { return }

        for url in contents where url.pathExtension != "tmp" {
            let path = url.path
            logger.info("File changed in workspace: \(path)")
            fileLogger.info("File changed in workspace: \(path)", category: "FileWatcherService")
        }
    }

    func stop() {
        debounceItems.values.forEach { $0.cancel() }
        debounceItems.removeAll()
        source?.cancel()
        source = nil
    }

    deinit {
        stop()
    }
}

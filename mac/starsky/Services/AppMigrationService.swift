import Foundation
import OSLog

enum AppMigrationService {
    private static let logger = Logger(subsystem: "nl.qdraw.starsky", category: "AppMigrationService")

    // Moves shared data files from the legacy Application Support location to the
    // App Group container. Runs once on first launch after upgrading. Safe to call
    // repeatedly — each item is skipped if the destination already exists.
    static func migrateToGroupContainerIfNeeded() {
        let src = ApplicationPaths.appSupport
        let dst = ApplicationPaths.groupContainer
        // When the entitlement isn't provisioned, groupContainer falls back to appSupport.
        // In that case there is nothing to migrate.
        guard src.standardizedFileURL != dst.standardizedFileURL else { return }

        let fm = FileManager.default
        let sentinel = dst.appendingPathComponent(".migration-done")
        guard !fm.fileExists(atPath: sentinel.path) else { return }

        let names = [
            "starsky.db",
            "appsettings.json",
            "appsettings.local.json",
            "logs",
            "thumbnailTempFolder"
        ]

        for name in names {
            let srcURL = src.appendingPathComponent(name)
            let dstURL = dst.appendingPathComponent(name)
            guard fm.fileExists(atPath: srcURL.path),
                  !fm.fileExists(atPath: dstURL.path) else { continue }
            do {
                // TODO fm.moveItem
                try fm.copyItem(at: srcURL, to: dstURL)
                logger.info("Migrated \(name, privacy: .public) to App Group container")
            } catch {
                logger.error("Failed to migrate \(name, privacy: .public): \(error.localizedDescription, privacy: .public)")
            }
        }

        fm.createFile(atPath: sentinel.path, contents: nil)
    }
}

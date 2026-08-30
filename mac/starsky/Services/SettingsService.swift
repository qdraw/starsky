import Foundation
import OSLog

class SettingsService {
    private let logger = Logger(subsystem: "nl.qdraw.starsky", category: "SettingsService")
    private let settingsFile: URL

    private(set) var current: DesktopSettings = DesktopSettings()

    init(settingsFile: URL = ApplicationPaths.settingsFile) {
        self.settingsFile = settingsFile
    }

    func load() {
        guard FileManager.default.fileExists(atPath: settingsFile.path) else {
            current = DesktopSettings()
            return
        }
        do {
            let data = try Data(contentsOf: settingsFile)
            let decoder = JSONDecoder()
            decoder.dateDecodingStrategy = .iso8601
            current = try decoder.decode(DesktopSettings.self, from: data)
        } catch {
            logger.warning("Failed to load settings, using defaults: \(error.localizedDescription)")
            current = DesktopSettings()
        }
    }

    func save() {
        save(current)
    }

    func save(_ settings: DesktopSettings) {
        current = settings
        do {
            let encoder = JSONEncoder()
            encoder.outputFormatting = [.prettyPrinted, .sortedKeys]
            encoder.dateEncodingStrategy = .iso8601
            let data = try encoder.encode(settings)
            try data.write(to: settingsFile, options: .atomic)
        } catch {
            logger.warning("Failed to save settings: \(error.localizedDescription)")
        }
    }
}

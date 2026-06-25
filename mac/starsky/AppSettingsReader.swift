import Foundation

/// Simple reader for appsettings.json located in the Electron cache folder.
/// It reads the top-level `app` object and returns `StorageFolder` and `StorageFolderToken`.
enum AppSettingsReader {
    /// Optional override for the appsettings.json file path (used in tests)
    static var storageFileOverride: String? = nil

    /// Return path to appsettings.json in the Electron cache folder.
    private static func appSettingsPath() -> String {
        if let override = storageFileOverride { return override }
        let home = NSHomeDirectory()
        let base = "\(home)/Library/Application Support/starsky"
        return (base as NSString).appendingPathComponent("appsettings.json")
    }

    /// Normalize and expand tilde if present, and validate folder existence.
    /// Returns nil when the expanded path does not exist on disk.
    private static func normalizeAndValidateFolder(_ raw: String?) -> String? {
        guard var folder = raw else { return nil }
        // Expand tilde
        folder = (folder as NSString).expandingTildeInPath

        // Trim whitespace
        folder = folder.trimmingCharacters(in: .whitespacesAndNewlines)

        // Ensure folder exists and is a directory
        var isDir: ObjCBool = false
        if FileManager.default.fileExists(atPath: folder, isDirectory: &isDir) && isDir.boolValue {
            return folder
        }
        return nil
    }

    /// Read and parse the JSON file and return the values from the top-level `app` object.
    static func getStorageFolderAndTokenFromAppSettings() -> (storageFolder: String?, storageFolderToken: String?) {
        let path = appSettingsPath()
        let fm = FileManager.default
        guard fm.fileExists(atPath: path) else { return (nil, nil) }
        do {
            let data = try Data(contentsOf: URL(fileURLWithPath: path))
            let root = try JSONSerialization.jsonObject(with: data, options: [])
            if let dict = root as? [String: Any], let app = dict["app"] as? [String: Any] {
                let rawFolder = app["StorageFolder"] as? String
                let token = app["StorageFolderToken"] as? String
                let folder = normalizeAndValidateFolder(rawFolder)
                return (folder, token)
            }
            // If format differs, return nils
            return (nil, nil)
        } catch {
            // Parsing error — return nils
            NSLog("[AppSettingsReader] failed to read/parse appsettings.json: %@", String(describing: error))
            return (nil, nil)
        }
    }
}

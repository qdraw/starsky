import Foundation

struct BackendAppSettings {
    let storageFolder: String
    let storageFolderMappings: [String: String]
}

enum AppSettingsReader {
    static func read(
        mainFile: URL = ApplicationPaths.appSettingsFile,
        localFile: URL = ApplicationPaths.appSettingsLocalFile
    ) -> BackendAppSettings? {
        let base  = parse(mainFile)
        let local = parse(localFile)

        let folder   = local?.storageFolder.nilIfEmpty         ?? base?.storageFolder.nilIfEmpty
        let mappings = local?.storageFolderMappings            ?? base?.storageFolderMappings ?? [:]
        guard let folder else { return nil }
        return BackendAppSettings(storageFolder: folder, storageFolderMappings: mappings)
    }

    private static func parse(_ url: URL) -> BackendAppSettings? {
        guard let data = try? Data(contentsOf: url),
              let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any],
              let app  = json["app"] as? [String: Any] else { return nil }
        let folder   = app["StorageFolder"] as? String ?? ""
        let mappings = app["StorageFolderMappings"] as? [String: String] ?? [:]
        return BackendAppSettings(storageFolder: folder, storageFolderMappings: mappings)
    }
}

private extension String {
    var nilIfEmpty: String? { isEmpty ? nil : self }
}

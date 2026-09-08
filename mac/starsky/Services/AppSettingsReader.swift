import Foundation

struct BackendAppSettings {
    let storageFolder: String
    let storageFolderMappings: [String: String]
}

enum AppSettingsReader {
    private struct RawSettings {
        var storageFolder: String?
        var storageFolderMappings: [String: String]?   // nil = key absent in file
    }

    static func read(
        mainFile: URL = ApplicationPaths.appSettingsFile,
        localFile: URL = ApplicationPaths.appSettingsLocalFile
    ) -> BackendAppSettings? {
        let base  = parse(mainFile)
        let local = parse(localFile)

        // Per-key fallback: local only wins for keys it explicitly contains.
        let folder   = (local?.storageFolder ?? base?.storageFolder)?.nilIfEmpty
        let mappings = local?.storageFolderMappings ?? base?.storageFolderMappings ?? [:]
        guard let folder else { return nil }
        return BackendAppSettings(storageFolder: folder, storageFolderMappings: mappings)
    }

    private static func parse(_ url: URL) -> RawSettings? {
        guard let data = try? Data(contentsOf: url),
              let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any],
              let app  = json["app"] as? [String: Any] else { return nil }
        return RawSettings(
            storageFolder:         app["StorageFolder"]         as? String,
            storageFolderMappings: app["StorageFolderMappings"] as? [String: String]
        )
    }
}

private extension String {
    var nilIfEmpty: String? { isEmpty ? nil : self }
}

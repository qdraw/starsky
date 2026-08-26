import Foundation

enum FakeStarskyBin {
    static func create(in directory: URL) throws -> URL {
        try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
        let binary = directory.appendingPathComponent("starsky")
        let script = "#!/bin/sh\nsleep 3600\n"
        try script.write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes(
            [.posixPermissions: 0o755],
            ofItemAtPath: binary.path
        )
        return binary
    }
}

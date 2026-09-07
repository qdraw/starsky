import Foundation

enum FakeProcessBin {
    /// Creates a shell script that exits with the given code and returns its URL.
    @discardableResult
    static func create(in directory: URL, exitCode: Int32 = 0, name: String = "process") throws -> URL {
        try FileManager.default.createDirectory(at: directory, withIntermediateDirectories: true)
        let binary = directory.appendingPathComponent(name)
        let script = "#!/bin/sh\nexit \(exitCode)\n"
        try script.write(to: binary, atomically: true, encoding: .utf8)
        try FileManager.default.setAttributes(
            [.posixPermissions: 0o755],
            ofItemAtPath: binary.path
        )
        return binary
    }
}

import Foundation

func runBinary() {
    #if arch(arm64)
    let binaryName = "resources/osx-arm64/starsky"
    #elseif arch(x86_64)
    let binaryName = "resources/osx-x64/starsky"
    #else
    print("Unsupported architecture")
    return
    #endif

    guard let binaryPath = Bundle.main.path(forResource: binaryName, ofType: nil) else {
        print("Binary not found: \(binaryName)")
        return
    }
    
    guard let port = getFreePort() else {
        print("Failed to get free port")
        return
    }

    let process = Process()
    process.executableURL = URL(fileURLWithPath: binaryPath)
    process.arguments = ["--port", "\(port)"]

    print("free port ", port)
    
    do {
        try process.run()
        process.waitUntilExit()
        print("Binary finished with code \(process.terminationStatus)")
    } catch {
        print("Failed to run binary: \(error)")
    }
}

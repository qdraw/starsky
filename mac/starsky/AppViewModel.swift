import Foundation
import Combine

final class AppViewModel: ObservableObject {
    @Published var webUrl: URL? = nil
    @Published var isLoading: Bool = true

    private let launcher: BackendLaunching
    private let checker: ReadinessChecking
    private var backgroundQueue = DispatchQueue(label: "app.viewmodel", qos: .userInitiated)

    init(launcher: BackendLaunching = BackendLauncher(), checker: ReadinessChecking = ReadinessChecker()) {
        self.launcher = launcher
        self.checker = checker
    }

    func start() {
        backgroundQueue.async { [weak self] in
            guard let self = self else { return }
            guard let port = self.launcher.launch() else {
                DispatchQueue.main.async {
                    self.isLoading = false
                }
                return
            }

            let localUrl = URL(string: "http://localhost:\(port)/")!
            let ready = self.checker.waitUntilReady(localUrl: localUrl, timeout: 20)

            DispatchQueue.main.async {
                if ready {
                    self.webUrl = localUrl
                }
                // Finished attempting to start; loading should be false regardless of outcome
                self.isLoading = false
            }
        }
    }

    func stop() {
        launcher.terminate()
    }
}

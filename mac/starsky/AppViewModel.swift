import Foundation
import Combine

final class AppViewModel: ObservableObject {
    #if DEBUG
    /// Session-only override for the base web URL, settable in DEBUG builds.
    /// This override is temporary and will reset on next launch.
    @Published var sessionOverrideWebUrl: URL? = nil

    /// Internal backing store for the web URL.
    @Published private var internalWebUrl: URL? = nil
    var webUrl: URL? {
        sessionOverrideWebUrl ?? internalWebUrl
    }
    #else
    @Published var webUrl: URL? = nil
    #endif

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
                    #if DEBUG
                    self.internalWebUrl = localUrl
                    #else
                    self.webUrl = localUrl
                    #endif
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

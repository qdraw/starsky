import SwiftUI

@main
struct starskyApp: App {
    @State private var webUrl: URL? = nil
    @State private var isLoading = true

    var body: some Scene {
        WindowGroup {
            Group {
                if let url = webUrl {
                    WebView(url: url)
                        .onDisappear {
                            // Optionally terminate launched process if needed
                            // launchedProcess?.terminate()
                        }
                } else if isLoading {
                    VStack(spacing: 12) {
                        ProgressView("Starting backend...")
                        Text("Waiting for local server to be ready")
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
                    .onAppear(perform: startBackendAndWait)
                } else {
                    // Fallback to a safe remote URL if local startup failed
                    WebView(url: URL(string: "https://example.com")!)
                }
            }
        }
    }

    private func startBackendAndWait() {
        DispatchQueue.global(qos: .userInitiated).async {
            guard let port = runBinary() else {
                DispatchQueue.main.async {
                    self.isLoading = false
                }
                return
            }

            let localUrl = URL(string: "http://localhost:\(port)/")!
            let ready = waitUntilReady(localUrl: localUrl, timeout: 20)

            DispatchQueue.main.async {
                if ready {
                    self.webUrl = localUrl
                } else {
                    self.isLoading = false
                }
            }
        }
    }

    private func waitUntilReady(localUrl: URL, timeout: TimeInterval) -> Bool {
        let deadline = Date().addingTimeInterval(timeout)
        let session = URLSession(configuration: .ephemeral)

        while Date() < deadline {
            var request = URLRequest(url: localUrl)
            request.httpMethod = "GET"
            let semaphore = DispatchSemaphore(value: 0)
            var isReady = false

            let task = session.dataTask(with: request) { data, response, error in
                if let http = response as? HTTPURLResponse {
                    if (200...399).contains(http.statusCode) {
                        isReady = true
                    }
                }
                semaphore.signal()
            }
            task.resume()
            let _ = semaphore.wait(timeout: .now() + 1.0)

            if isReady {
                return true
            }
            Thread.sleep(forTimeInterval: 0.5)
        }
        return false
    }
}

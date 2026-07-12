import Foundation

protocol ReadinessChecking {
    func waitUntilReady(localUrl: URL, timeout: TimeInterval) -> Bool
}

/// Responsible for polling a local URL until it's ready (http 2xx-3xx) or timeout.
final class ReadinessChecker: ReadinessChecking {
    func waitUntilReady(localUrl: URL, timeout: TimeInterval) -> Bool {
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

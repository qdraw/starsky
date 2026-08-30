import Foundation

class FakeURLProtocol: URLProtocol {
    private static let lock = NSLock()
    // Keyed by URL path (scheme/host/port/query ignored) so dynamic ports and query
    // parameters don't break matching, while distinct API endpoints remain isolated.
    private static var _responses: [String: [(Data, HTTPURLResponse)]] = [:]
    private static var _capturedRequests: [URLRequest] = []

    static var capturedRequests: [URLRequest] {
        lock.lock(); defer { lock.unlock() }
        return _capturedRequests
    }

    override class func canInit(with _: URLRequest) -> Bool { true }
    override class func canonicalRequest(for request: URLRequest) -> URLRequest { request }

    override func startLoading() {
        let pathKey = request.url?.path ?? ""
        Self.lock.lock()
        Self._capturedRequests.append(request)
        let entry: (Data, HTTPURLResponse)?
        if Self._responses[pathKey]?.isEmpty == false {
            entry = Self._responses[pathKey]!.removeFirst()
            if Self._responses[pathKey]!.isEmpty { Self._responses.removeValue(forKey: pathKey) }
        } else {
            entry = nil
        }
        Self.lock.unlock()

        guard let (data, response) = entry else {
            client?.urlProtocol(self, didFailWithError: URLError(.fileDoesNotExist))
            return
        }
        client?.urlProtocol(self, didReceive: response, cacheStoragePolicy: .notAllowed)
        client?.urlProtocol(self, didLoad: data)
        client?.urlProtocolDidFinishLoading(self)
    }

    override func stopLoading() {}

    static func makeSession() -> URLSession {
        let config = URLSessionConfiguration.ephemeral
        config.protocolClasses = [FakeURLProtocol.self]
        return URLSession(configuration: config)
    }

    static func enqueue(statusCode: Int, url: URL, data: Data = Data()) {
        let response = HTTPURLResponse(url: url, statusCode: statusCode, httpVersion: nil, headerFields: nil)!
        lock.lock(); defer { lock.unlock() }
        _responses[url.path, default: []].append((data, response))
    }

    static func reset() {
        lock.lock(); defer { lock.unlock() }
        _responses = [:]
        _capturedRequests = []
    }
}

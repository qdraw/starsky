import Foundation
import WebKit

// Cookie persistence helper: saves/restores cookies to a file in Application Support so they survive app restarts
// This was extracted from webView.swift to keep responsibilities separated.
enum CookiePersistence {
    // Allow overriding storage URL for tests
    static var storageURLOverride: URL? = nil
    private static let fileName = "cookies.json"

    private static var fileURL: URL? {
        if let override = storageURLOverride { return override }
        // Application Support/<BundleIdentifier>/cookies.json
        let fm = FileManager.default
        guard let appSupport = try? fm.url(for: .applicationSupportDirectory, in: .userDomainMask, appropriateFor: nil, create: true) else { return nil }
        // Use bundle identifier folder if available
        let bundleId = Bundle.main.bundleIdentifier ?? "starsky"
        let dir = appSupport.appendingPathComponent(bundleId, isDirectory: true)
        if !fm.fileExists(atPath: dir.path) {
            try? fm.createDirectory(at: dir, withIntermediateDirectories: true, attributes: nil)
        }
        return dir.appendingPathComponent(fileName)
    }

    // MARK: - JSON-safe serialization helpers
    private static func jsonSafeProperties(from cookie: HTTPCookie) -> [String: Any] {
        var out = [String: Any]()
        if let props = cookie.properties {
            for (k, v) in props {
                let key = k.rawValue
                if let date = v as? Date {
                    out[key] = date.timeIntervalSince1970
                } else if let number = v as? NSNumber {
                    out[key] = number
                } else if let str = v as? String {
                    out[key] = str
                } else {
                    // Fallback to description
                    out[key] = String(describing: v)
                }
            }
        }
        return out
    }

    private static func cookieFrom(json dict: [String: Any]) -> HTTPCookie? {
        var props = [HTTPCookiePropertyKey: Any]()
        for (k, v) in dict {
            let key = HTTPCookiePropertyKey(k)
            // Handle expiry keys in a SDK-agnostic way (some environments use different key names)
            if k.lowercased().contains("expire") {
                // expiry may be stored as Double (timeInterval) or string
                if let ti = v as? TimeInterval {
                    props[key] = Date(timeIntervalSince1970: ti)
                } else if let dbl = v as? Double {
                    props[key] = Date(timeIntervalSince1970: dbl)
                } else if let str = v as? String, let dbl = Double(str) {
                    props[key] = Date(timeIntervalSince1970: dbl)
                } else {
                    // fallback: try to parse RFC3339 date string
                    if let str = v as? String {
                        let formatter = ISO8601DateFormatter()
                        if let d = formatter.date(from: str) {
                            props[key] = d
                        }
                    }
                }
            } else {
                props[key] = v
            }
        }
        return HTTPCookie(properties: props)
    }

    // MARK: - Testable save/load helpers
    // Save an array of HTTPCookie into the storage file (JSON)
    static func saveCookiesArray(_ cookies: [HTTPCookie]) throws {
        guard let file = fileURL else { throw NSError(domain: "CookiePersistence", code: 1, userInfo: [NSLocalizedDescriptionKey: "No file URL available"]) }
        var arr = [[String: Any]]()
        for cookie in cookies {
            arr.append(jsonSafeProperties(from: cookie))
        }
        let data = try JSONSerialization.data(withJSONObject: arr, options: [])
        try data.write(to: file, options: [.atomic])
    }

    // Load cookies from storage file into array
    static func loadCookiesArray() throws -> [HTTPCookie] {
        guard let file = fileURL else { return [] }
        let fm = FileManager.default
        guard fm.fileExists(atPath: file.path) else { return [] }
        let data = try Data(contentsOf: file)
        guard let arr = try JSONSerialization.jsonObject(with: data, options: []) as? [[String: Any]] else { return [] }
        var out = [HTTPCookie]()
        for dict in arr {
            if let cookie = cookieFrom(json: dict) {
                out.append(cookie)
            }
        }
        return out
    }

    // MARK: - Public persist/restore that operate on WKHTTPCookieStore
    static func persistCookies(from cookieStore: WKHTTPCookieStore, completion: (() -> Void)? = nil) {
        cookieStore.getAllCookies { cookies in
            do {
                try saveCookiesArray(cookies)
            } catch {
                NSLog("[CookiePersistence] persist error: %@", String(describing: error))
            }
            completion?()
        }
    }

    static func restoreCookies(into cookieStore: WKHTTPCookieStore, completion: (() -> Void)? = nil) {
        DispatchQueue.global(qos: .userInitiated).async {
            do {
                let cookies = try loadCookiesArray()
                let group = DispatchGroup()
                for cookie in cookies {
                    group.enter()
                    cookieStore.setCookie(cookie) {
                        group.leave()
                    }
                }
                group.notify(queue: .main) {
                    completion?()
                }
            } catch {
                NSLog("[CookiePersistence] restore error: %@", String(describing: error))
                DispatchQueue.main.async { completion?() }
            }
        }
    }

    // Remove persisted storage file
    static func clearStoredCookies() throws {
        guard let file = fileURL else { return }
        let fm = FileManager.default
        if fm.fileExists(atPath: file.path) {
            try fm.removeItem(at: file)
        }
    }
}

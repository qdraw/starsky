import SwiftUI

@main
struct starskyApp: App {
    var body: some Scene {
        WindowGroup {
            WebView(url: URL(string: "https://example.com")!)
                .onAppear {
                    runBinary()
                }
        }
    }
}

import SwiftUI
import AppKit

@main
struct starskyApp: App {
    @StateObject private var viewModel = AppViewModel()

    var body: some Scene {
        WindowGroup {
            RootView(viewModel: viewModel)
                .onAppear {
                    viewModel.start()
                }
        }
        // Add a Developer menu in Debug builds to open the Web Inspector
        .commands {
            #if DEBUG
            CommandMenu("Developer") {
                Button("Show Web Inspector") {
                    // Sends the action through the responder chain; WKWebView responds to this selector when
                    // developer extras are enabled on its preferences.
                    NSApp.sendAction(Selector(("showWebInspector:")), to: nil, from: nil)
                }
                .keyboardShortcut("I", modifiers: [.command, .option])
            }
            #endif
        }
    }
}

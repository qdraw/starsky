import SwiftUI
import WebKit
import AppKit

@main
struct starskyApp: App {
    @StateObject private var viewModel = AppViewModel()
    @State private var showBaseUrlSheet = false
    @State private var baseUrlInput = ""
    @State private var baseUrlError: String? = nil

    var body: some Scene {
        WindowGroup {
            RootView(viewModel: viewModel)
                .onAppear {
                    viewModel.start()
                }
            #if DEBUG
                .sheet(isPresented: $showBaseUrlSheet) {
                    VStack(alignment: .leading, spacing: 16) {
                        Text("Set Base URL")
                            .font(.headline)
                        TextField("Base URL", text: $baseUrlInput)
                            .textFieldStyle(RoundedBorderTextFieldStyle())
                            .frame(width: 380)
                        if let error = baseUrlError {
                            Text(error).foregroundColor(.red)
                        }
                        HStack {
                            Button("Cancel") { showBaseUrlSheet = false }
                            Spacer()
                            Button("Set") {
                                if let url = URL(string: baseUrlInput), url.scheme != nil {
                                    viewModel.sessionOverrideWebUrl = url
                                    showBaseUrlSheet = false
                                } else {
                                    baseUrlError = "Please enter a valid URL."
                                }
                            }
                            .keyboardShortcut(.defaultAction)
                        }
                    }
                    .padding(24)
                    .frame(width: 400)
                }
            #endif
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
                
                Divider()
                
                Button("Open in Browser") {
                    if let url = viewModel.webUrl {
                        NSWorkspace.shared.open(url)
                    }
                }
                .keyboardShortcut("B", modifiers: [.command, .option])
                .disabled(viewModel.webUrl == nil)
                
                Divider()
                
                Button("Set Base URL…") {
                    baseUrlInput = viewModel.webUrl?.absoluteString ?? ""
                    baseUrlError = nil
                    showBaseUrlSheet = true
                }
                .keyboardShortcut(",", modifiers: [.command, .option])
                
                Divider()

                Button("Clear persisted cookies") {
                    // Remove persisted cookie file
                    do {
                        try CookiePersistence.clearStoredCookies()
                    } catch {
                        NSLog("[Starsky] Failed to clear persisted cookies: %@", String(describing: error))
                    }

                    // Also remove cookies from WKWebsiteDataStore
                    let dataStore = WKWebsiteDataStore.default()
                    let cookieTypes = Set([WKWebsiteDataTypeCookies])
                    let since = Date(timeIntervalSince1970: 0)
                    dataStore.fetchDataRecords(ofTypes: cookieTypes) { _ in
                        dataStore.removeData(ofTypes: cookieTypes, modifiedSince: since) {
                            NSLog("[Starsky] Cleared WKWebsiteDataStore cookies")
                        }
                    }
                }
                .keyboardShortcut("K", modifiers: [.command, .option])

                Divider()
            }
            #endif
        }
    }
}

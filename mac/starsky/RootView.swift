import SwiftUI

struct RootView: View {
    @ObservedObject var viewModel: AppViewModel

    var body: some View {
        Group {
            if let url = viewModel.webUrl {
                WebView(url: url)
                    .onDisappear {
                        viewModel.stop()
                    }
            } else if viewModel.isLoading {
                VStack(spacing: 12) {
                    ProgressView("Starting backend...")
                    Text("Waiting for local server to be ready")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }
                .frame(maxWidth: .infinity, maxHeight: .infinity)
            } else {
                WebView(url: URL(string: "https://example.com")!)
            }
        }
    }
}

import SwiftUI

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
    }
}

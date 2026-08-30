import Foundation

protocol SettingsView: AnyObject {
    func setValidating()
    func setResult(success: Bool, message: String)
    func setUrlFieldEnabled(_ enabled: Bool)
    func setSaveEnabled(_ enabled: Bool)
}

class SettingsPresenter {
    weak var view: SettingsView?

    var onSwitchToLocal: (() -> Void)?
    var onSwitchToRemote: (() -> Void)?

    private let settingsService: SettingsService
    private let remoteUrlValidator: RemoteUrlValidator
    private let windowManager: WindowManagerProtocol

    init(
        settingsService: SettingsService,
        remoteUrlValidator: RemoteUrlValidator,
        windowManager: WindowManagerProtocol
    ) {
        self.settingsService = settingsService
        self.remoteUrlValidator = remoteUrlValidator
        self.windowManager = windowManager
    }

    var currentSettings: DesktopSettings { settingsService.current }

    func modeChanged(to mode: RuntimeMode) {
        let wasRemote = settingsService.current.mode == .remote
        var settings = settingsService.current
        settings.mode = mode
        settingsService.save(settings)
        let isRemote = mode == .remote
        view?.setUrlFieldEnabled(isRemote)
        view?.setSaveEnabled(isRemote)
        if mode == .local && wasRemote { onSwitchToLocal?() }
        else if mode == .remote && !wasRemote { onSwitchToRemote?() }
    }

    func saveUrl(_ urlString: String) async {
        view?.setValidating()
        view?.setSaveEnabled(false)
        let result = await remoteUrlValidator.validate(urlString: urlString)
        await MainActor.run { [weak self] in
            guard let self else { return }
            view?.setSaveEnabled(true)
            if result.success {
                var settings = settingsService.current
                settings.remoteBaseUrl = urlString.hasSuffix("/") ? String(urlString.dropLast()) : urlString
                settingsService.save(settings)
                windowManager.reopenAll()
                view?.setResult(success: true, message: "Setting is saved")
            } else {
                view?.setResult(success: false, message: result.error ?? "Validation failed.")
            }
        }
    }

    func updateCheckChanged(enabled: Bool) {
        var settings = settingsService.current
        settings.updateCheckEnabled = enabled
        settingsService.save(settings)
    }
}

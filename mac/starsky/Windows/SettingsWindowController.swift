import AppKit

class SettingsWindowController: NSWindowController {
    private let remoteUrlPlaceholder: String

    var onSwitchToLocal: (() -> Void)? {
        didSet { presenter.onSwitchToLocal = onSwitchToLocal }
    }
    var onSwitchToRemote: (() -> Void)? {
        didSet { presenter.onSwitchToRemote = onSwitchToRemote }
    }

    private let presenter: SettingsPresenter
    private var localRadio: NSButton!
    private var remoteRadio: NSButton!
    private var urlField: NSTextField!
    private var saveUrlButton: NSButton!
    private var updateCheckBox: NSButton!
    private var statusLabel: NSTextField!

    init(
        settingsService: SettingsService,
        remoteUrlValidator: RemoteUrlValidator,
        windowManager: WindowManagerProtocol,
        remoteUrlPlaceholder: String = "https://your-starsky-server.com"
    ) {
        self.remoteUrlPlaceholder = remoteUrlPlaceholder
        presenter = SettingsPresenter(
            settingsService: settingsService,
            remoteUrlValidator: remoteUrlValidator,
            windowManager: windowManager
        )
        let window = NSWindow(
            contentRect: NSRect(x: 0, y: 0, width: 380, height: 480),
            styleMask: [.titled, .closable],
            backing: .buffered,
            defer: false
        )
        window.title = "Connection Settings"
        window.isReleasedWhenClosed = false
        super.init(window: window)
        presenter.view = self
        setupContent()
        loadCurrentSettings()
    }

    required init?(coder _: NSCoder) { fatalError("init(coder:) not supported") }

    // MARK: - Setup

    private func setupContent() {
        guard let contentView = window?.contentView else { return }
        contentView.wantsLayer = true

        let modeLabel = NSTextField(labelWithString: "Connection Mode")
        modeLabel.font = NSFont.boldSystemFont(ofSize: 12)
        modeLabel.translatesAutoresizingMaskIntoConstraints = false

        localRadio = NSButton(radioButtonWithTitle: "Local (bundled backend)", target: self, action: #selector(modeChanged(_:)))
        localRadio.tag = 0
        localRadio.translatesAutoresizingMaskIntoConstraints = false

        remoteRadio = NSButton(radioButtonWithTitle: "Remote (connect to server)", target: self, action: #selector(modeChanged(_:)))
        remoteRadio.tag = 1
        remoteRadio.translatesAutoresizingMaskIntoConstraints = false

        let urlLabel = NSTextField(labelWithString: "Server URL:")
        urlLabel.translatesAutoresizingMaskIntoConstraints = false

        urlField = NSTextField()
        urlField.placeholderString = remoteUrlPlaceholder
        urlField.translatesAutoresizingMaskIntoConstraints = false

        saveUrlButton = NSButton(title: "Save URL", target: self, action: #selector(saveUrl))
        saveUrlButton.translatesAutoresizingMaskIntoConstraints = false

        statusLabel = NSTextField(labelWithString: "")
        statusLabel.font = NSFont.systemFont(ofSize: 11)
        statusLabel.translatesAutoresizingMaskIntoConstraints = false

        updateCheckBox = NSButton(checkboxWithTitle: "Check for updates on startup", target: self, action: #selector(updateCheckChanged))
        updateCheckBox.translatesAutoresizingMaskIntoConstraints = false

        for view in [modeLabel, localRadio!, remoteRadio!, urlLabel, urlField!, saveUrlButton!, statusLabel!, updateCheckBox!] as [NSView] {
            contentView.addSubview(view)
        }

        NSLayoutConstraint.activate([
            modeLabel.topAnchor.constraint(equalTo: contentView.topAnchor, constant: 24),
            modeLabel.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20),

            localRadio.topAnchor.constraint(equalTo: modeLabel.bottomAnchor, constant: 12),
            localRadio.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20),

            remoteRadio.topAnchor.constraint(equalTo: localRadio.bottomAnchor, constant: 8),
            remoteRadio.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20),

            urlLabel.topAnchor.constraint(equalTo: remoteRadio.bottomAnchor, constant: 20),
            urlLabel.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20),

            urlField.topAnchor.constraint(equalTo: urlLabel.bottomAnchor, constant: 6),
            urlField.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20),
            urlField.trailingAnchor.constraint(equalTo: contentView.trailingAnchor, constant: -20),

            saveUrlButton.topAnchor.constraint(equalTo: urlField.bottomAnchor, constant: 10),
            saveUrlButton.trailingAnchor.constraint(equalTo: contentView.trailingAnchor, constant: -20),

            statusLabel.topAnchor.constraint(equalTo: saveUrlButton.bottomAnchor, constant: 8),
            statusLabel.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20),
            statusLabel.trailingAnchor.constraint(equalTo: contentView.trailingAnchor, constant: -20),

            updateCheckBox.bottomAnchor.constraint(equalTo: contentView.bottomAnchor, constant: -20),
            updateCheckBox.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20)
        ])
    }

    private func loadCurrentSettings() {
        let settings = presenter.currentSettings
        localRadio.state = settings.mode == .local ? .on : .off
        remoteRadio.state = settings.mode == .remote ? .on : .off
        urlField.stringValue = settings.remoteBaseUrl
        updateCheckBox.state = settings.updateCheckEnabled ? .on : .off
        let isRemote = settings.mode == .remote
        urlField.isEnabled = isRemote
        saveUrlButton.isEnabled = isRemote
    }

    // MARK: - Actions

    @objc private func modeChanged(_ sender: NSButton) {
        let mode: RuntimeMode = sender.tag == 0 ? .local : .remote
        presenter.modeChanged(to: mode)
        localRadio.state = mode == .local ? .on : .off
        remoteRadio.state = mode == .remote ? .on : .off
    }

    @objc private func saveUrl() {
        Task { await presenter.saveUrl(urlField.stringValue) }
    }

    @objc private func updateCheckChanged() {
        presenter.updateCheckChanged(enabled: updateCheckBox.state == .on)
    }
}

// MARK: - SettingsView

extension SettingsWindowController: SettingsView {
    func setValidating() {
        statusLabel.stringValue = "Validating…"
        statusLabel.textColor = .labelColor
    }

    func setResult(success: Bool, message: String) {
        statusLabel.stringValue = message
        statusLabel.textColor = success ? .systemGreen : .systemRed
    }

    func setUrlFieldEnabled(_ enabled: Bool) {
        urlField.isEnabled = enabled
    }

    func setSaveEnabled(_ enabled: Bool) {
        saveUrlButton.isEnabled = enabled
    }
}

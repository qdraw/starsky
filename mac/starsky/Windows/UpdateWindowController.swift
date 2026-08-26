import AppKit

class UpdateWindowController: NSWindowController {
    private var updateService: UpdateService
    private var updateButton: NSButton!

    init(updateService: UpdateService) {
        self.updateService = updateService

        let window = NSWindow(
            contentRect: NSRect(x: 0, y: 0, width: 400, height: 180),
            styleMask: [.titled, .closable],
            backing: .buffered,
            defer: false
        )
        window.title = "Starsky Update Available"
        window.isReleasedWhenClosed = false
        super.init(window: window)
        setupContent()
    }

    required init?(coder: NSCoder) { fatalError("init(coder:) not supported") }

    private func setupContent() {
        guard let contentView = window?.contentView else { return }

        let label = NSTextField(wrappingLabelWithString: "A new version of Starsky is available.")
        label.alignment = .center
        label.font = NSFont.systemFont(ofSize: 13)
        label.translatesAutoresizingMaskIntoConstraints = false
        contentView.addSubview(label)

        updateButton = NSButton(title: "Update Now", target: self, action: #selector(updateNow))
        updateButton.keyEquivalent = "\r"
        updateButton.translatesAutoresizingMaskIntoConstraints = false
        contentView.addSubview(updateButton)

        let closeButton = NSButton(title: "Close", target: self, action: #selector(dismissAndSuppress))
        closeButton.translatesAutoresizingMaskIntoConstraints = false
        contentView.addSubview(closeButton)

        NSLayoutConstraint.activate([
            label.topAnchor.constraint(equalTo: contentView.topAnchor, constant: 30),
            label.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20),
            label.trailingAnchor.constraint(equalTo: contentView.trailingAnchor, constant: -20),
            updateButton.bottomAnchor.constraint(equalTo: contentView.bottomAnchor, constant: -20),
            updateButton.trailingAnchor.constraint(equalTo: contentView.centerXAnchor, constant: -8),
            closeButton.bottomAnchor.constraint(equalTo: contentView.bottomAnchor, constant: -20),
            closeButton.leadingAnchor.constraint(equalTo: contentView.centerXAnchor, constant: 8)
        ])
    }

    @objc private func updateNow() {
        updateButton.isEnabled = false
        updateService.applyUpdate()
    }

    @objc private func dismissAndSuppress() {
        updateService.recordWarningShown()
        window?.close()
    }
}

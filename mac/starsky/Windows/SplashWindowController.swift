import AppKit

class SplashWindowController: NSWindowController {
    private let statusLabel = NSTextField(labelWithString: "Starting…")

    convenience init() {
        let window = NSPanel(
            contentRect: NSRect(x: 0, y: 0, width: 320, height: 180),
            styleMask: [.borderless, .nonactivatingPanel],
            backing: .buffered,
            defer: false
        )
        window.backgroundColor = NSColor(calibratedRed: 0.102, green: 0.102, blue: 0.180, alpha: 1)
        window.isOpaque = true
        window.hasShadow = true
        window.level = .floating
        window.center()

        self.init(window: window)
        setupContent()
    }

    private func setupContent() {
        guard let contentView = window?.contentView else { return }

        statusLabel.textColor = .white
        statusLabel.font = NSFont.systemFont(ofSize: 13)
        statusLabel.alignment = .center
        statusLabel.translatesAutoresizingMaskIntoConstraints = false
        contentView.addSubview(statusLabel)

        NSLayoutConstraint.activate([
            statusLabel.centerXAnchor.constraint(equalTo: contentView.centerXAnchor),
            statusLabel.centerYAnchor.constraint(equalTo: contentView.centerYAnchor),
            statusLabel.widthAnchor.constraint(equalTo: contentView.widthAnchor, constant: -20)
        ])
    }

    func setStatus(_ message: String) {
        DispatchQueue.main.async { [weak self] in
            self?.statusLabel.stringValue = message
        }
    }
}

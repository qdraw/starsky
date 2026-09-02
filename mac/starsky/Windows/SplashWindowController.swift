import AppKit

class SplashWindowController: NSWindowController {
    private let statusLabel = NSTextField(labelWithString: NSLocalizedString("splash.status.starting", comment: ""))
    private let hintLabel = NSTextField(labelWithString: "")
    private var isDismissable = false

    convenience init() {
        let window = NSPanel(
            contentRect: NSRect(x: 0, y: 0, width: 320, height: 180),
            styleMask: [.borderless],
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

        let clickView = ClickThroughView()
        clickView.translatesAutoresizingMaskIntoConstraints = false
        clickView.onMouseDown = { [weak self] in
            guard self?.isDismissable == true else { return }
            self?.close()
        }
        contentView.addSubview(clickView)
        NSLayoutConstraint.activate([
            clickView.topAnchor.constraint(equalTo: contentView.topAnchor),
            clickView.bottomAnchor.constraint(equalTo: contentView.bottomAnchor),
            clickView.leadingAnchor.constraint(equalTo: contentView.leadingAnchor),
            clickView.trailingAnchor.constraint(equalTo: contentView.trailingAnchor)
        ])

        statusLabel.textColor = .white
        statusLabel.font = NSFont.systemFont(ofSize: 13)
        statusLabel.alignment = .center
        statusLabel.translatesAutoresizingMaskIntoConstraints = false
        clickView.addSubview(statusLabel)

        hintLabel.textColor = NSColor.white.withAlphaComponent(0.45)
        hintLabel.font = NSFont.systemFont(ofSize: 10)
        hintLabel.alignment = .center
        hintLabel.translatesAutoresizingMaskIntoConstraints = false
        clickView.addSubview(hintLabel)

        NSLayoutConstraint.activate([
            statusLabel.centerXAnchor.constraint(equalTo: clickView.centerXAnchor),
            statusLabel.centerYAnchor.constraint(equalTo: clickView.centerYAnchor, constant: -10),
            statusLabel.widthAnchor.constraint(equalTo: clickView.widthAnchor, constant: -20),

            hintLabel.centerXAnchor.constraint(equalTo: clickView.centerXAnchor),
            hintLabel.topAnchor.constraint(equalTo: statusLabel.bottomAnchor, constant: 8),
            hintLabel.widthAnchor.constraint(equalTo: clickView.widthAnchor, constant: -20)
        ])
    }

    func setStatus(_ message: String) {
        DispatchQueue.main.async { [weak self] in
            self?.statusLabel.stringValue = message
        }
    }

    func enableDismiss() {
        DispatchQueue.main.async { [weak self] in
            self?.isDismissable = true
            self?.hintLabel.stringValue = NSLocalizedString("splash.hint.clickToDismiss", comment: "")
        }
    }
}

private class ClickThroughView: NSView {
    var onMouseDown: (() -> Void)?

    override func mouseDown(with _: NSEvent) {
        onMouseDown?()
    }
}

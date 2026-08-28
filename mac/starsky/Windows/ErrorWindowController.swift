import AppKit

class ErrorWindowController: NSWindowController {
    convenience init(message: String) {
        let window = NSWindow(
            contentRect: NSRect(x: 0, y: 0, width: 440, height: 220),
            styleMask: [.titled, .closable],
            backing: .buffered,
            defer: false
        )
        window.title = "Starsky — Error"
        window.isReleasedWhenClosed = false
        self.init(window: window)
        setupContent(message: message)
    }

    private func setupContent(message: String) {
        guard let contentView = window?.contentView else { return }

        let label = NSTextField(wrappingLabelWithString: message)
        label.alignment = .center
        label.font = NSFont.systemFont(ofSize: 13)
        label.translatesAutoresizingMaskIntoConstraints = false
        contentView.addSubview(label)

        let button = NSButton(title: "OK", target: self, action: #selector(dismiss))
        button.keyEquivalent = "\r"
        button.translatesAutoresizingMaskIntoConstraints = false
        contentView.addSubview(button)

        NSLayoutConstraint.activate([
            label.topAnchor.constraint(equalTo: contentView.topAnchor, constant: 30),
            label.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 20),
            label.trailingAnchor.constraint(equalTo: contentView.trailingAnchor, constant: -20),
            button.centerXAnchor.constraint(equalTo: contentView.centerXAnchor),
            button.bottomAnchor.constraint(equalTo: contentView.bottomAnchor, constant: -20)
        ])
    }

    @objc private func dismiss() {
        if let parent = window?.sheetParent {
            parent.endSheet(window!)
        } else {
            window?.close()
        }
    }

    static func show(message: String, parentWindow: NSWindow? = nil) {
        let controller = ErrorWindowController(message: message)
        controller.window?.center()
        if let parent = parentWindow {
            parent.beginSheet(controller.window!) { _ in }
        } else {
            controller.showWindow(nil)
        }
    }
}

---
sidebar_position: 7
---

# Swift and AppKit

Starsky for macOS is a native Swift and AppKit application in the `mac/` folder.
It provides a desktop shell around the Starsky web interface using `WKWebView`.

The app starts and manages a bundled Starsky backend in local mode, or connects to a configured remote server in remote mode. Business logic remains in the .NET backend; the macOS client owns the native application lifecycle, windows, menus, settings, and WebKit integration.

## WebKit and Sparkle

WebKit supplies the `WKWebView` that presents the Starsky web interface. Sparkle provides signed automatic updates for the native macOS application.

## Build and test

After adding or removing Swift source files, regenerate the Xcode project:

```bash
cd mac
xcodegen generate
```

Run the macOS XCTest suite with the `starsky` scheme:

```bash
xcodebuild test -project starsky.xcodeproj -scheme starsky \
  -destination 'platform=macOS' \
  CODE_SIGN_IDENTITY="" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO
```
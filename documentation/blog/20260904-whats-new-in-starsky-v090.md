---
slug: whats-new-in-starsky-v090
title: "What's new in Starsky v0.9.0?"
authors: dion
tags: [photo management, software update]
date: 2026-09-04
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# What's new in Starsky v0.9.0?

Version 0.9.0 is a landmark release, the biggest architectural shift in the desktop app since the
project started, plus a round of stability fixes across the back-end and front-end. The headline:
Electron is gone, replaced by native desktop apps on both macOS and Windows.

<!-- truncate -->

### 🚨 Breaking Changes

- **Native desktop apps replace Electron (PR #3238, #3234)**
    - The Electron-based desktop app has been removed. Starsky now ships two native desktop apps:
        - **macOS**: a Swift/AppKit application wrapping the web UI via `WKWebView`
        - **Windows**: a WPF-based application
    - Native apps mean a smaller footprint, faster startup, and tighter OS integration.
    - **Windows users upgrading from below 0.8.2 must uninstall the old version first** before
      installing 0.9.0. The packaging has changed enough that an in-place upgrade won't work
      cleanly.

- **ExifTool namespace reorganised for AI features (PR #3296)**
    - The namespace housing AI-related ExifTool functionality has been restructured. If you
      integrate at that level, review the changes in PR #3296.

- **Windows desktop wrapper changed (PR #3234)**
    - The way the Windows app wraps the backend has changed significantly as part of the
      Electron-to-WPF migration.

---

### 🆕 New Features

- **macOS Sparkle auto-updates with pre-release opt-in (PR #3261, #3275)**
    - The macOS app now publishes an AppCast feed and supports Sparkle-based auto-updates. Users can
      opt in to pre-release builds directly from the app's preferences.

- **Windows pre-release update opt-in (PR #3266)**
    - The Windows app gains the same pre-release update channel as macOS.

- **Next / Previous by keyboard in Archive (PR #3244)**
    - Arrow key navigation now works in the Archive view, letting you step through photos without
      touching the mouse.

---

### 🛠️ Bug Fixes & Improvements

**Back-end**

- **Background service execution restricted (PR #3292)**
    - Services that previously ran more freely in the background are now gated more carefully,
      reducing resource contention during long-running sessions.

- **Reduced cache duration (PR #3292, #3294)**
    - Cache lifetimes have been shortened to address memory pressure issues that could accumulate
      over time.

- **SQLite unique constraint violations handled (PR #3282)**
    - Error code 19 (unique constraint violation) is now caught and handled gracefully rather than
      surfacing as an unhandled exception.

- **`GetAllRecursiveAsync` timeout handling (PR #3282)**
    - The method now correctly recognises `CommandTimeoutExpired` instead of hanging indefinitely.

- **XMP read/write for `ImageStabilization` tag (PR #3290)**
    - Starsky can now read and write the `ImageStabilization` XMP tag.

- **Improved `CompareStringDictionary` (PR #3259)**
    - Internal dictionary comparison logic has been made more robust.

**Front-end**

- **ExifTool pipeline closes correctly (PR #3262)**
    - The ExifTool pipeline now shuts down cleanly after processing instead of leaving a dangling
      process.

- **Read-only path setting fixed in preferences (PR #3259)**
    - The read-only path field in preferences is no longer editable when it shouldn't be.

- **File move client-side (PR #3245)**
    - Moving files from the client is more reliable.

---

### ⚠️ Upgrade Notes

1. **Windows users on versions below 0.8.2**: uninstall the existing app before installing 0.9.0.
   The new WPF-based packaging is not compatible with an in-place upgrade from the Electron version.
2. If you integrate with the ExifTool AI namespace directly, review the namespace changes in PR
   #3296 before upgrading.

---

For the full list of changes, see [history.md](https://docs.qdraw.nl/docs/advanced-options/history).

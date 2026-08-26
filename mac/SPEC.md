# Starsky Desktop (macOS) — Specification

Version: **0.8.1**  
Target: **macOS 13.0+, arm64 + x86_64 (universal)**  
Framework: **AppKit + WKWebView, Swift 5.10**

---

## 1. Purpose

Starsky Desktop is a native macOS shell that wraps the Starsky photo-management web application inside a `WKWebView` browser control. It provides:

- A **one-click install** experience with no separate server setup required (Local mode)
- A **multi-window, persistent** desktop experience on top of the existing React/ASP.NET Core web UI
- **Automatic updates** delivered through GitHub Releases via Sparkle 2
- **OS-level file integration** — download a photo from the server and open it in any local application

The app does **not** reimplement any photo-management logic; all business logic lives in the bundled or remote Starsky ASP.NET Core backend.

---

## 2. Technology Stack

| Component | Technology |
|---|---|
| Shell | AppKit, macOS 13.0+, arm64 + x86_64 |
| Embedded browser | `WebKit.WKWebView` |
| Auto-updates | Sparkle 2 (`SPUUpdater`) |
| Logging | `OSLog` + custom `DailyFileLogger` |
| Build system | Xcode + `xcodegen` (`project.yml`) |
| Test framework | XCTest |

---

## 3. Connection Modes

The app operates in one of two modes, switchable at runtime in Settings without restarting the app.

### 3.1 Local Mode (default)

1. Finds a free TCP port (`PortFinder.findFreePort()`)
2. Launches the bundled `starsky` backend as a child process (`Foundation.Process`)
3. Polls `/api/health` until the backend is ready (60-second timeout)
4. Checks version compatibility via `/api/health/version`
5. Displays the web UI pointed at `http://localhost:{port}`

The bundled backend is expected at:
- `<app bundle>/Contents/MacOS/runtime-starsky-osx-arm64/starsky` (Apple Silicon)
- `<app bundle>/Contents/MacOS/runtime-starsky-osx-x64/starsky` (Intel)

This directory is populated at build time by an Xcode "Copy Runtime" Run Script build phase.

Before launching, `BackendService` clears Gatekeeper quarantine on the backend binary:
```bash
xattr -rd com.apple.quarantine <backend-path>
codesign --force --deep -s - <backend-path>
```

The backend is configured entirely via environment variables:

| Environment Variable | Value |
|---|---|
| `ASPNETCORE_URLS` | `http://localhost:{port}` |
| `app__appsettingspath` | `~/Library/Application Support/starsky/appsettings.json` |
| `app__appsettingslocalpath` | `~/Library/Application Support/starsky/appsettings.local.json` |
| `app__databaseConnection` | `Data Source=~/Library/Application Support/starsky/starsky.db` |
| `app__tempFolder` | `~/Library/Caches/starsky/tempFolder/` |
| `app__thumbnailTempFolder` | `~/Library/Application Support/starsky/thumbnailTempFolder/` |
| `app__NoAccountLocalhost` | `true` |
| `app__UseLocalDesktop` | `true` |
| `app__AccountRegisterDefaultRole` | `Administrator` |
| `app__ThumbnailGenerationIntervalInMinutes` | `300` |
| `app__Verbose` | `false` |

**Backend restart:** If the backend process exits unexpectedly and the app is not shutting down, it is automatically restarted once after a 2-second delay. Quarantine clearing is applied again before restart.

### 3.2 Remote Mode

Connects to an existing Starsky server at a user-configured URL. The bundled backend is not started. Full authentication applies (the web UI handles login). The URL must be validated via `RemoteUrlValidator` before it is saved.

---

## 4. Application Startup Sequence

```
AppDelegate.applicationDidFinishLaunching(_:)
  │
  ├─ 1. ApplicationPaths.ensureDirectories()
  ├─ 2. Init DailyFileLogger
  ├─ 3. Load settings (SettingsService.load())
  ├─ 4. Construct services
  ├─ 5. Show SplashWindowController
  │
  ├─ 6. [Local]  findFreePort → BackendService.start(port:) → waitForHealth (60 s)
  │               → checkVersion
  │    [Remote]  Validate remoteBaseUrl is set (else ErrorWindowController + terminate)
  │
  ├─ 7. FileWatcherService.start()
  ├─ 8. WindowManager.restoreWindows()
  ├─ 9. SplashWindowController.close()
  │
  └─ 10. (after 5 s) UpdateService.checkAsync()
         → if true: show UpdateWindowController
```

### Shutdown (`applicationWillTerminate(_:)`)

1. `FileWatcherService.stop()`
2. `WindowManager.closeAll()`
3. `BackendService.stop()` — kills the backend process, waits up to 5 seconds

---

## 5. Windows

### 5.1 SplashWindowController

- Fixed 320×180, dark background (`#1a1a2e`), no title bar (`NSPanel` with `.borderless` style), `level = .floating`
- Shown during backend startup; displays status messages via a centred `NSTextField`
- Closed after `restoreWindows()` completes

### 5.2 MainWindowController

Default size: **1200×800**. Position: saved and restored per window index.

#### Layout

```
┌─────────────────────────────────────────────────────────┐
│  [macOS system title bar + traffic lights]              │
├─────────────────────────────────────────────────────────┤
│                                                         │
│                  WKWebView (full area)                  │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

macOS menu bar is screen-level (set in `AppDelegate`).

#### Menu Bar

| Menu | Item | Shortcut | Action |
|---|---|---|---|
| File | New Window | Cmd+N | Open another MainWindowController at `?f=/` |
| File | Reload All | Cmd+Shift+R / F5 | Reload all open windows |
| File | Edit File in Editor | Cmd+E | Local: inject Cmd+E keydown; Remote: download + open |
| Starsky | Connection Settings… | — | Open SettingsWindowController |
| Starsky | Application Settings | Cmd+Shift+K | Inject Cmd+Shift+K into web app |
| View | Developer Tools | Cmd+Opt+I | Enable WKWebView inspector, open devtools |
| View | Open in Browser | — | `NSWorkspace.shared.open(currentURL)` |
| Help | Documentation | — | Open `https://qdraw.nl/special/starsky/docs/` |
| Help | Release Overview | — | Open `https://github.com/qdraw/starsky/releases` |

#### Navigation Rules

Navigation events are intercepted by `WKNavigationDelegate.webView(_:decidePolicyFor:decisionHandler:)`:

- **Allowed:** `localhost` (any port), or the configured remote server origin (host + scheme + port must match)
- **Blocked + opened in system browser:** any other origin

`target="_blank"` link clicks are handled by `WKUIDelegate.webView(_:createWebViewWith:for:windowFeatures:)`:

- Allowed origin → open a new `MainWindowController` at that route
- External origin → open in `NSWorkspace.shared.open(_:)`

#### Route Persistence

`WKNavigationDelegate.webView(_:didFinish:)` triggers on each page load:
1. Extracts `path + query + fragment` from the current URL
2. Saves it along with the current window frame to `RoutePersistenceService`

On window close (`windowWillClose`), the window's route entry is removed.

#### User Agent

```swift
configuration.applicationNameForUserAgent = "starsky/0.8.1"
```

#### Edit File in Editor

- **Local mode:** Injects `keydown` event for Cmd+E into the web app via `WKWebView.evaluateJavaScript`
- **Remote mode:** Reads the `?f=` query parameter → calls `FileDownloadService.downloadAndOpen(path:baseUrl:openFile:)`

### 5.3 SettingsWindowController

Fixed 380×480, not resizable, shown as a sheet on the focused main window (or as a standalone window if no main window is open).

| Control | Behaviour |
|---|---|
| **Local** radio button | Saves `Mode = .local`; if switching from Remote → calls `WindowManager.reopenAll()` |
| **Remote** radio button | Saves `Mode = .remote`; enables URL controls |
| **Server URL** text field | Editable only in Remote mode |
| **Save URL** button | Calls `RemoteUrlValidator.validate(urlString:)` → if valid, saves URL and calls `reopenAll()`; shows green "Setting is saved" or red error message |
| **Check for updates** checkbox | Toggles `updateCheckEnabled` in settings; saved immediately |

### 5.4 ErrorWindowController

Fixed 440×220 modal sheet or `NSAlert`. Displays a user-facing error message for:
- Missing remote URL on startup
- Backend startup failure / timeout
- WKWebView initialisation failure
- File download failure

### 5.5 UpdateWindowController

Shown 5 seconds after startup when `UpdateService.checkAsync()` returns `true`.

| Button | Behaviour |
|---|---|
| Update Now | Calls `UpdateService.applyUpdate()` → Sparkle downloads and restarts |
| Close | Calls `UpdateService.recordWarningShown()` (suppresses for 4 days) → closes window |

---

## 6. Services

### 6.1 BackendService

Manages the lifecycle of the bundled `starsky` child process (`Foundation.Process`).

| Method | Behaviour |
|---|---|
| `start(port:)` | Clears quarantine, injects env vars, starts process with stdout/stderr piped to logger |
| `stop()` | Sets `isShuttingDown = true`, terminates process, waits up to 5 s |
| `findBackendExe()` | Returns path to `starsky` binary in runtime dir; `nil` if missing |
| `setEnvironment(_:port:)` | Writes all required env vars into `Process.environment` |
| _(private)_ `onProcessExited` | If not shutting down and not yet restarted: waits 2 s, clears quarantine, calls `start(port:)` again (one restart only) |

### 6.2 NavigationService

URL policy for `WKWebView`.

| Method | Behaviour |
|---|---|
| `isAllowedOrigin(_:baseUrl:)` | Returns `true` if host is `localhost`, or if host+scheme+port match `baseUrl` |
| `buildStartUrl(baseUrl:route:)` | Concatenates `baseUrl` (trailing slash stripped) with `route`; defaults route to `?f=/` |
| `getEffectiveBaseUrl(localPort:)` | Returns `http://localhost:{port}` in Local mode, or `remoteBaseUrl` in Remote mode |

### 6.3 SettingsService

Reads and writes `DesktopSettings` as JSON.

| Method | Behaviour |
|---|---|
| `load()` | Reads from settings file; falls back to defaults on missing or corrupt file |
| `save()` / `save(_:)` | Serialises to indented JSON; logs on write failure (best-effort) |

Default settings file: `~/Library/Application Support/starsky/settings.json`  
Path overridable via constructor (used in tests).

### 6.4 RoutePersistenceService

Persists per-window URL routes + geometry through `SettingsService`.

| Method | Behaviour |
|---|---|
| `getRoutes()` | Returns `settings.current.windows` list |
| `saveRoute(index:route:geometry:)` | Grows list if needed; copies geometry if provided; saves |
| `removeRoute(index:)` | Removes entry at index; saves |
| `clearAll()` | Clears entire list; saves |

### 6.5 PortFinder

`findFreePort() -> Int` — binds a `ServerSocket` / `Network.NWListener` to port 0 (OS assigns a free port), reads the assigned port, closes the listener, returns the port number.

Implementation: Use `Foundation` socket APIs — bind `SOCK_STREAM` to port 0, call `getsockname` to read assigned port, close socket.

### 6.6 FileWatcherService

Watches `~/Library/Caches/starsky/tempFolder/` for file changes using `DispatchSource.makeFileSystemObjectSource`.

| Behaviour | Detail |
|---|---|
| Monitored | File-descriptor–based watch on the temp folder directory |
| Skip filter | Files with `.tmp` extension are ignored |
| Debounce | 500 ms `DispatchWorkItem`; reset on rapid successive events |
| On fire | Logs `"File changed in workspace: {path}"` |
| `start()` | Creates temp folder if missing, opens directory fd, attaches source |
| `stop()` / `dispose()` | Cancels dispatch source, closes fd, cancels debounce timers |

### 6.7 FileDownloadService

Downloads a photo from the Starsky server and opens it locally.

**Steps:**

1. `GET {baseUrl}/starsky/api/index?f={encodedPath}` — validate file exists
2. `GET {baseUrl}/starsky/api/download-sidecar?f={encodedPath}` — download XMP sidecar (best-effort)
3. `GET {baseUrl}/starsky/api/download-photo?isThumbnail=false&f={encodedPath}&cache=false` — download original
4. Write to `{tempFolder}/{parentDir}/{filename}.tmp`, rename to final path
5. `NSWorkspace.shared.open(finalURL)` — open with default application (if `openFile = true`)

**URLSession:** 60-second timeout. Injected via constructor.

### 6.8 RemoteUrlValidator

Validates a user-entered remote server URL.

**Steps:**

1. Trim trailing slash
2. `URL(string:)` — reject if not a valid absolute URL
3. Reject if scheme is not `http` or `https`
4. `GET {url}/api/health` — accept HTTP 200 or 503; reject all others or exceptions

Returns `UrlValidationResult(success: Bool, error: String?)`.  
URLSession: 10-second timeout. Injected via constructor.

### 6.9 UpdateService

Wraps Sparkle 2 `SPUUpdater` for GitHub-hosted auto-updates.

| Method | Behaviour |
|---|---|
| `checkAsync() async -> Bool` | Returns `false` if: `updateCheckEnabled = false`, OR last warning < 4 days ago (`suppressMinutes = 5760`), OR Sparkle unavailable, OR no update found |
| `applyUpdate()` | Calls `SPUUpdater.checkForUpdates()` — Sparkle handles download + restart |
| `recordWarningShown()` | Sets `lastUpdateWarningShown = Date()`, saves settings |

Appcast URL: configured in `Info.plist` `SUFeedURL` key.  
Gracefully degrades when Sparkle is unavailable (sets `updater = nil`).

### 6.10 WindowManager

Manages the collection of open `MainWindowController` instances.

| Method | Behaviour |
|---|---|
| `setLocalPort(_:)` | Stores port for use in `getEffectiveBaseUrl` |
| `openMainWindow(route:geometry:)` | Creates and shows `MainWindowController`; cascades by `count × 24 px`; default 1200×800 at (100,100) |
| `restoreWindows()` | Opens one window per saved `SavedWindowState`; opens one default window if nothing saved |
| `closeAll()` | Closes all tracked windows |
| `reopenAll()` | Clears saved routes → closes all → opens default window at `?f=/` |
| `reloadAll()` | Calls `reload()` on each open window (dispatched to main thread) |

When the last window closes, `NSApplication.shared.terminate(nil)` is called.  
Dock-click (`applicationShouldHandleReopen`): if no windows open, call `openMainWindow()`.

### 6.11 DailyFileLogger

Custom logger writing to date-stamped files: `~/Library/Application Support/starsky/logs/starsky-{yyyy-MM-dd}.log`

- Minimum level: `info`
- Format: `yyyy-MM-dd HH:mm:ss [Level] Category: Message`
- Exception info on next line
- Thread-safe via `NSLock`; write failures silently ignored

---

## 7. Data Model

### 7.1 DesktopSettings

Persisted to `~/Library/Application Support/starsky/settings.json` as indented JSON.

| Property | Type | Default | Description |
|---|---|---|---|
| `mode` | `RuntimeMode` | `.local` | Local or Remote |
| `remoteBaseUrl` | `String` | `""` | URL of remote server (Remote mode only) |
| `updateCheckEnabled` | `Bool` | `true` | Whether to check for updates on startup |
| `lastUpdateWarningShown` | `Date?` | `nil` | UTC timestamp of last update prompt |
| `windows` | `[SavedWindowState]` | `[]` | Per-window route + geometry |

### 7.2 SavedWindowState

| Property | Type | Default | Description |
|---|---|---|---|
| `route` | `String` | `"?f=/"` | URL path+query+fragment |
| `x` | `Double` | `100` | Window x position |
| `y` | `Double` | `100` | Window y position |
| `width` | `Double` | `1200` | Window width |
| `height` | `Double` | `800` | Window height |
| `isMaximized` | `Bool` | `false` | Whether window was maximized (zoomed) |

### 7.3 UrlValidationResult

```swift
struct UrlValidationResult {
    let success: Bool
    let error: String?
}
```

### 7.4 RuntimeMode

```swift
enum RuntimeMode: Int, Codable {
    case local = 0
    case remote = 1
}
```

---

## 8. File System Layout

| Path | Purpose |
|---|---|
| `~/Library/Application Support/starsky/settings.json` | Desktop app settings |
| `~/Library/Application Support/starsky/appsettings.json` | Starsky backend config (Local mode) |
| `~/Library/Application Support/starsky/appsettings.local.json` | Machine-specific backend overrides |
| `~/Library/Application Support/starsky/starsky.db` | SQLite database (Local mode) |
| `~/Library/Application Support/starsky/logs/starsky-{date}.log` | Daily log files |
| `~/Library/Application Support/starsky/thumbnailTempFolder/` | Thumbnail cache |
| `~/Library/Caches/starsky/tempFolder/` | Downloaded files |
| `<bundle>/Contents/MacOS/runtime-starsky-osx-arm64/starsky` | Bundled backend (Apple Silicon) |
| `<bundle>/Contents/MacOS/runtime-starsky-osx-x64/starsky` | Bundled backend (Intel) |

---

## 9. Build System

### Project Generation

The Xcode project is generated from `mac/project.yml` using [xcodegen](https://github.com/yonaskolb/XcodeGen):

```bash
brew install xcodegen
cd mac && xcodegen generate
```

### Targets

| Target | Type | Description |
|---|---|---|
| `starsky` | macOS App | Main application |
| `starskyTests` | Unit Test Bundle | XCTest service tests |

Universal binary: `ARCHS = arm64 x86_64`  
Minimum deployment: macOS 13.0

### Runtime Copy Build Phase (Run Script in `starsky` target)

```bash
for ARCH in arm64 x64; do
  RUNTIME_SRC="$SRCROOT/../starskydesktop/runtime-starsky-mac-${ARCH}"
  RUNTIME_DST="$BUILT_PRODUCTS_DIR/$PRODUCT_NAME.app/Contents/MacOS/runtime-starsky-osx-${ARCH}"
  if [ -d "$RUNTIME_SRC" ]; then
    mkdir -p "$RUNTIME_DST"
    cp -R "$RUNTIME_SRC/." "$RUNTIME_DST/"
  else
    echo "warning: Runtime not found at $RUNTIME_SRC"
  fi
done
```

### Publish (CLI)

```bash
cd mac && xcodegen generate
xcodebuild archive \
  -project starsky.xcodeproj \
  -scheme starsky \
  -configuration Release \
  -archivePath ../build/starsky.xcarchive \
  ARCHS="arm64 x86_64"

xcodebuild -exportArchive \
  -archivePath ../build/starsky.xcarchive \
  -exportPath ../build/ \
  -exportOptionsPlist ExportOptions.plist
```

---

## 10. Code Signing & Notarization

### Entitlements (`starsky.entitlements`)

```xml
<key>com.apple.security.app-sandbox</key><false/>
<key>com.apple.security.network.client</key><true/>
<key>com.apple.security.cs.allow-jit</key><true/>
```

No App Sandbox (required to spawn backend subprocess and access arbitrary file paths).

### Build Settings

```
CODE_SIGN_IDENTITY = Developer ID Application
ENABLE_HARDENED_RUNTIME = YES
OTHER_CODE_SIGN_FLAGS = --options runtime
DEVELOPMENT_TEAM = <team-id>
```

### Notarization Flow

```bash
# Submit for notarization
xcrun notarytool submit build/starsky.dmg \
  --apple-id "$APPLE_ID" \
  --team-id "$TEAM_ID" \
  --password "$NOTARYTOOL_APP_PASSWORD" \
  --wait

# Staple ticket to DMG
xcrun stapler staple build/starsky.dmg
```

`ExportOptions.plist` sets `signingStyle = manual`, `signingCertificate = Developer ID Application`.

---

## 11. Test Suite

**Target:** `starskyTests`  
**Framework:** XCTest, macOS 13.0+  
**Total target:** ≥ 52 tests (matching Windows coverage)

### Test Infrastructure

**`FakeURLProtocol`** — `URLProtocol` subclass. Configured with a queue of `(Data, HTTPURLResponse)` pairs; dequeues one per request. Used to test HTTP-dependent services without network access.

**`CreateFakeStarskyBin`** — writes a minimal shell script (`#!/bin/sh\nexit 0`) as the fake backend binary for `BackendService` tests.

### Test Classes

| Class | What is covered |
|---|---|
| `ApplicationPathsTests` | AppSupport/Caches folder mapping; path structure |
| `BackendServiceTests` | stop/dispose on unstarted service; all env var keys; findBackendExe (found/not found) |
| `DesktopSettingsTests` | Default property values; JSON round-trip |
| `FileDownloadServiceTests` | Happy path writes file; sidecar failure still downloads; photo error propagates |
| `FileWatcherServiceTests` | start/stop/dispose lifecycle; temp folder creation |
| `NavigationServiceTests` | `isAllowedOrigin` (localhost, matching remote, different host); `buildStartUrl` |
| `PortFinderTests` | Returns positive port; port is bindable |
| `RemoteUrlValidatorTests` | Empty string; invalid scheme; HTTP 200; HTTP 503; other status; exception; trailing slash |
| `RoutePersistenceServiceTests` | Empty list; save entry; save with geometry; list expansion; remove; clear all |
| `SettingsServiceTests` | Missing file (defaults); valid JSON; corrupt JSON (defaults); round-trip |
| `UpdateServiceTests` | Disabled; recent warning suppresses; recordWarningShown; applyUpdate without update |

### Running Tests

```bash
xcodebuild test \
  -project mac/starsky.xcodeproj \
  -scheme starskyTests \
  -destination 'platform=macOS'
```

---

## 12. CI/CD

### `desktop-macos-pr-build.yml`

Triggered on push/PR to `master` when `mac/**` files change.  
Runner: `macos-latest`.

| Step | Command |
|---|---|
| Install xcodegen | `brew install xcodegen` |
| Generate project | `cd mac && xcodegen generate` |
| Build | `xcodebuild build -scheme starsky -configuration Debug` |
| Test | `xcodebuild test -scheme starskyTests -destination 'platform=macOS'` |
| Upload results | `actions/upload-artifact` → `test-results-macos` |
| Publish (on tag) | archive → export → notarize → staple → upload DMG |

Concurrency group: one run per branch; in-progress runs cancelled on new push.

---

## 13. Keyboard Shortcuts Reference

| Shortcut | Scope | Action |
|---|---|---|
| Cmd+N | MainWindow | Open new window |
| Cmd+Shift+R | MainWindow | Reload all windows |
| F5 | MainWindow | Reload all windows |
| Cmd+E | MainWindow | Edit current file in editor |
| Cmd+Shift+K | MainWindow | Open application settings (injected into web app) |
| Cmd+Opt+I | MainWindow | Open WKWebView Developer Tools |

---

## 14. Security Considerations

| Concern | Mechanism |
|---|---|
| External navigation | `decidePolicyFor:` cancels non-allowlisted navigations and opens in system browser |
| External new-window | `createWebViewWith:` intercepts `target="_blank"`; only allowed origins open in-app |
| Local backend auth bypass | `app__NoAccountLocalhost=true` — safe because backend binds to `127.0.0.1` |
| Remote URL validation | Scheme must be `http`/`https`; server must respond to `/api/health` |
| Credential storage | No credentials stored; Remote mode relies on web app's cookie-based session |
| Update integrity | Sparkle verifies packages via EdDSA signature |
| Gatekeeper | Backend binary quarantine cleared on first launch and crash-restart |

---

## 15. Known Limitations & Non-Goals

| Item | Note |
|---|---|
| Windows / Linux | Not supported; AppKit and WKWebView are macOS-only |
| Menu bar / status item | Not implemented; app exits when last window closes |
| macOS notifications | No `UNUserNotificationCenter`; update prompt uses a native window |
| UI testing | Window controllers not covered by automated tests (require display) |
| `FileWatcherService` debounce | Tested only for no-throw behaviour; file-change callback is logging-only |
| Multi-user | Single user per installation |
| App Sandbox | Disabled — required to spawn backend subprocess |

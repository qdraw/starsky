# Starsky Desktop (Windows) — Specification

Version: **0.8.1**  
Target: **Windows 10/11, x64**  
Framework: **.NET 10, WPF (`net10.0-windows`)**

---

## 1. Purpose

Starsky Desktop is a native Windows shell that wraps the Starsky photo-management web application inside a `Microsoft.Web.WebView2` browser control. It provides:

- A **one-click install** experience with no separate server setup required (Local mode)
- A **multi-window, persistent** desktop experience on top of the existing React/ASP.NET Core web UI
- **Automatic updates** delivered through GitHub Releases via Velopack
- **OS-level file integration** — download a photo from the server and open it in any local application

The app does **not** reimplement any photo-management logic; all business logic lives in the bundled or remote Starsky ASP.NET Core backend.

---

## 2. Technology Stack

| Component | Technology |
|---|---|
| Shell | WPF, `net10.0-windows`, `win-x64` |
| Embedded browser | `Microsoft.Web.WebView2` 1.0.4129.50 |
| Auto-updates | `Velopack` 0.0.988 |
| Logging | `Microsoft.Extensions.Logging` 10.0.0 |
| Build system | .NET SDK (`Microsoft.NET.Sdk`), MSBuild |
| Test framework | xUnit 2.9.2, `net10.0-windows` |

---

## 3. Connection Modes

The app operates in one of two modes, switchable at runtime in Settings without restarting the app.

### 3.1 Local Mode (default)

1. Finds a free TCP port (`PortFinder.FindFreePort`)
2. Launches the bundled `starsky.exe` backend as a child process
3. Polls `/api/health` until the backend is ready (60-second timeout)
4. Checks version compatibility via `/api/health/version`
5. Displays the web UI pointed at `http://localhost:{port}`

The bundled backend is expected at `<exe dir>\runtime-starsky-win-x64\starsky.exe` (or `starsky` without extension as fallback). This directory is populated at build time by an MSBuild `CopyStarskyRuntime` target.

The backend is configured entirely via environment variables injected by `BackendService.SetEnvironment`:

| Environment Variable | Value |
|---|---|
| `ASPNETCORE_URLS` | `http://localhost:{port}` |
| `app__appsettingspath` | `%AppData%\starsky\appsettings.json` |
| `app__appsettingslocalpath` | `%AppData%\starsky\appsettings.local.json` |
| `app__databaseConnection` | `Data Source=%AppData%\starsky\starsky.db` |
| `app__tempFolder` | `%LocalAppData%\starsky\tempFolder\` |
| `app__thumbnailTempFolder` | `%AppData%\starsky\thumbnailTempFolder\` |
| `app__NoAccountLocalhost` | `true` (skip login for localhost) |
| `app__UseLocalDesktop` | `true` (enable desktop-specific API behaviour) |
| `app__AccountRegisterDefaultRole` | `Administrator` |
| `app__ThumbnailGenerationIntervalInMinutes` | `300` |
| `app__Verbose` | `false` |

**Backend restart:** If the backend process exits unexpectedly and the app is not shutting down, it is automatically restarted once after a 2-second delay.

### 3.2 Remote Mode

Connects to an existing Starsky server at a user-configured URL. The bundled backend is not started. Full authentication applies (the web UI handles login). The URL must be validated via `RemoteUrlValidator` before it is saved.

---

## 4. Application Startup Sequence

```
Program.Main()
  │
  ├─ VelopackApp.Build().Run()     ← handle installer lifecycle events
  │
  └─ App.OnStartup()
       │
       ├─ 1. EnsureDirectories()   ← create AppData folders
       ├─ 2. Init logging          ← console + daily file
       ├─ 3. Load settings         ← %AppData%\starsky\settings.json
       ├─ 4. Construct services
       ├─ 5. Show SplashWindow
       │
       ├─ 6. [Local]  FindFreePort → StartAsync(port) → WaitForHealthAsync (60 s)
       │               → CheckVersionAsync
       │    [Remote]  Validate RemoteBaseUrl is set (else ErrorWindow + Shutdown)
       │
       ├─ 7. FileWatcherService.Start()
       ├─ 8. WindowManager.RestoreWindows()
       ├─ 9. SplashWindow.Close()
       │
       └─ 10. (after 5 s) UpdateService.CheckAsync()
              → if update available: show UpdateWindow
```

### Shutdown Sequence (`App.OnExit`)

1. `FileWatcherService.Stop()`
2. `WindowManager.CloseAll()`
3. `BackendService.StopAsync()` — kills the backend process, waits up to 5 seconds

---

## 5. Windows

### 5.1 SplashWindow

- Fixed 320×180, dark background (`#1a1a2e`), topmost, no window chrome
- Shown during backend startup; displays status messages ("Starting backend…", "Waiting for backend…", etc.)
- Closed after `RestoreWindows()` completes

### 5.2 MainWindow

Default size: **1200×800**. Position: saved and restored per window index.

#### Layout

```
┌─────────────────────────────────────────────────────────┐
│  File │ Settings │ View │ Help         [native title bar]│
├─────────────────────────────────────────────────────────┤
│                                                         │
│                  WebView2 (full area)                   │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

#### Menu Bar

| Menu | Item | Shortcut | Action |
|---|---|---|---|
| File | New Window | Ctrl+N | Open another MainWindow at `?f=/` |
| File | Reload All | Ctrl+Shift+R / F5 | Reload all open windows |
| File | Edit File in Editor | Ctrl+E | Local: forward keystroke to web app; Remote: download file + open locally |
| Settings | Connection Settings… | — | Open SettingsWindow |
| Settings | Application Settings | Ctrl+Shift+K | Inject `Ctrl+Shift+K` keyboard event into web app |
| View | Developer Tools | F12 | Open WebView2 DevTools window |
| View | Open in Browser | — | Open current URL in default system browser |
| Help | Documentation | — | Open `https://qdraw.nl/special/starsky/docs/` in system browser |
| Help | Release Overview | — | Open `https://github.com/qdraw/starsky/releases` in system browser |

#### Navigation Rules

Navigation events in the WebView2 are intercepted by `CoreWebView2_NavigationStarting`:

- **Allowed:** `localhost` (any port), or the configured remote server origin (host + scheme + port must match)
- **Blocked + opened in system browser:** any other origin

`target="_blank"` link clicks are handled by `CoreWebView2_NewWindowRequested`:

- Allowed origin → open a new `MainWindow` at that route
- External origin → open in system browser

#### Route Persistence

Every source change in the WebView2 triggers `CoreWebView2_SourceChanged`, which:
1. Extracts `path + query + fragment` from the current URL
2. Saves it along with the current window geometry (position, size, maximized state) to `SettingsService` via `RoutePersistenceService`

On close, the window's route entry is removed from the saved list.

#### User Agent

The WebView2 user agent is augmented: `{default user agent} starsky/0.8.1`

#### Edit File in Editor

Behaviour differs by mode:

- **Local mode:** Injects `Ctrl+E` as a `KeyboardEvent` into the web app's DOM (the web app handles the actual editor open via its own API)
- **Remote mode:** Reads the `?f=` query parameter from the current URL → calls `FileDownloadService.DownloadAndOpenAsync` → downloads the file to `%LocalAppData%\starsky\tempFolder\` → opens it with `Process.Start(UseShellExecute = true)`

### 5.3 SettingsWindow

Fixed 380×480, not resizable, centered on owner. Contains:

| Control | Behaviour |
|---|---|
| **Local** radio button | Saves `Mode = Local`; if switching from Remote → calls `WindowManager.ReopenAll()` to reconnect all windows |
| **Remote** radio button | Saves `Mode = Remote`; enables URL controls |
| **Server URL** text box | Editable only in Remote mode |
| **Save URL** button | Calls `RemoteUrlValidator.ValidateAsync(url)` → if valid, saves URL and calls `ReopenAll()`; shows green "Setting is saved" or red error message |
| **Check for updates** checkbox | Toggles `UpdateCheckEnabled` in settings; saved immediately |

### 5.4 ErrorWindow

Fixed 440×220 modal dialog. Displays a user-facing error message. Shown for:
- Missing remote URL on startup
- Backend startup failure / timeout
- WebView2 initialisation failure
- File download failure

### 5.5 UpdateWindow

Shown 5 seconds after startup when `UpdateService.CheckAsync()` returns `true`.

| Button | Behaviour |
|---|---|
| Update Now | Calls `UpdateService.ApplyUpdateAsync()` → downloads update → restarts app via Velopack |
| Close | Calls `UpdateService.RecordWarningShown()` (suppresses for 4 days) → closes window |

If `ApplyUpdateAsync` throws, shows a native `MessageBox` with the error and re-enables the button.

---

## 6. Services

### 6.1 BackendService

Manages the lifecycle of the bundled `starsky.exe` child process.

| Method | Behaviour |
|---|---|
| `StartAsync(port)` | Locates the exe, injects env vars, starts process with stdout/stderr redirected and logged |
| `StopAsync()` | Sets `_isShuttingDown = true`, kills process, waits up to 5 s |
| `Dispose()` | Best-effort kill + dispose |
| _(internal)_ `FindBackendExe(dir)` | Checks for `starsky.exe` then `starsky` in the runtime dir |
| _(internal static)_ `SetEnvironment(env, port)` | Writes all required env vars into the `ProcessStartInfo.Environment` dictionary |
| _(private)_ `OnProcessExited` | If not shutting down and not yet restarted: waits 2 s, calls `LaunchAsync()` again (one restart only) |

### 6.2 NavigationService

URL policy for the WebView2.

| Method | Behaviour |
|---|---|
| `IsAllowedOrigin(uri, baseUrl)` | Returns `true` if host is `localhost`, or if host+scheme+port match the configured `baseUrl` |
| `BuildStartUrl(baseUrl, route)` | Concatenates `baseUrl` (trailing slash stripped) with `route`; defaults route to `?f=/` |
| `GetEffectiveBaseUrl(localPort?)` | Returns `http://localhost:{port}` in Local mode, or `RemoteBaseUrl` in Remote mode |

### 6.3 SettingsService

Reads and writes `DesktopSettings` as JSON.

| Method | Behaviour |
|---|---|
| `Load()` | Reads from `_settingsFile`; falls back to defaults on missing or corrupt file |
| `Save()` / `Save(settings)` | Serialises to indented JSON; logs on write failure (best-effort) |

Default settings file: `%AppData%\starsky\settings.json`  
The path can be overridden via constructor (used in tests).

### 6.4 RoutePersistenceService

Persists per-window URL routes + geometry through `SettingsService`.

| Method | Behaviour |
|---|---|
| `GetRoutes()` | Returns `_settings.Current.Windows` list |
| `SaveRoute(index, route, geometry?)` | Grows list if needed; copies geometry fields if provided; saves |
| `RemoveRoute(index)` | Removes entry at index; saves |
| `ClearAll()` | Clears entire list; saves |

### 6.5 PortFinder

`FindFreePort()` — binds a `TcpListener` to port 0 (OS assigns a free port), reads the assigned port, closes the listener, returns the port number.

### 6.6 FileWatcherService

Watches `%LocalAppData%\starsky\tempFolder\` for file changes.

| Behaviour | Detail |
|---|---|
| Watched events | `Created` + `Changed` (recursive) |
| Skip filter | `.tmp` files are ignored (still being written) |
| Debounce | 500 ms per-path `System.Timers.Timer`; resets on rapid successive events |
| On fire | Logs `"File changed in workspace: {Path}"` (no upload/sync logic yet) |
| `Start()` | Creates the temp folder if missing, attaches handlers |
| `Stop()` / `Dispose()` | Disposes `FileSystemWatcher` and all debounce timers |

### 6.7 FileDownloadService

Downloads a photo from the Starsky server and opens it locally.

**Steps:**

1. `GET {baseUrl}/starsky/api/index?f={encodedPath}` — fetch file info (validates file exists)
2. `GET {baseUrl}/starsky/api/download-sidecar?f={encodedPath}` — download XMP sidecar if available (best-effort; failure is logged and ignored)
3. `GET {baseUrl}/starsky/api/download-photo?isThumbnail=false&f={encodedPath}&cache=false` — download original file bytes
4. Write bytes to `{TempFolder}/{parentDir}/{filename}.tmp`, then rename to final path (atomic on Windows NTFS)
5. `Process.Start(finalPath, UseShellExecute = true)` — open with default application (optional; controllable via `openFile` parameter)

**HttpClient:** 60-second timeout. Injected via constructor (defaults to `new HttpClient` if not provided).

### 6.8 RemoteUrlValidator

Validates a user-entered remote server URL.

**Steps:**

1. Trim trailing slash
2. `Uri.TryCreate` — reject if not a valid absolute URI
3. Reject if scheme is not `http` or `https`
4. `GET {url}/api/health` — accept HTTP 200 or 503 as valid (both mean the server is reachable); reject any other status or exception

Returns `UrlValidationResult(bool Success, string? Error)`.

**HttpClient:** 10-second timeout. Injected via constructor.

### 6.9 UpdateService

Wraps Velopack for GitHub-hosted auto-updates.

| Method | Behaviour |
|---|---|
| `CheckAsync()` | Returns `false` if: `UpdateCheckEnabled = false`, OR last warning was shown < 4 days ago (`SuppressMinutes = 5760`), OR Velopack is unavailable (running outside installer), OR no update found |
| `ApplyUpdateAsync()` | Downloads update via Velopack, then calls `ApplyUpdatesAndRestart` to restart the app into the new version |
| `RecordWarningShown()` | Sets `LastUpdateWarningShown = UtcNow`, saves settings |

Source: `https://github.com/qdraw/starsky` (GitHub Releases).  
Gracefully degrades when running outside the Velopack installer (constructor catches the exception and sets `_updateManager = null`).

### 6.10 WebViewEnvironmentService

Thread-safe singleton that creates the `CoreWebView2Environment` exactly once.

- Uses a `SemaphoreSlim(1,1)` lock to prevent concurrent initialisation
- User data folder: `%AppData%\starsky\webview2\`
- The environment object is shared across all `MainWindow` instances

### 6.11 WindowManager

Manages the collection of open `MainWindow` instances.

| Method | Behaviour |
|---|---|
| `SetLocalPort(port)` | Stores the port for use in `GetEffectiveBaseUrl` |
| `OpenMainWindow(route, geometry?)` | Creates and shows a new `MainWindow`; cascades position by `count × 24 px`; default size 1200×800 at (100,100) |
| `RestoreWindows()` | Opens one window per saved `SavedWindowState`; opens one default window if nothing saved |
| `CloseAll()` | Closes all tracked windows (best-effort) |
| `ReopenAll()` | Clears all saved routes → closes all windows → opens one default window at `?f=/` |
| `ReloadAll()` | Calls `MainWindow.Reload()` on each open window (dispatched to UI thread) |

When the last window closes, `Application.Current.Shutdown()` is called.

### 6.12 DailyFileLoggerProvider / DailyFileLogger

Custom `ILoggerProvider` that writes log lines to date-stamped files:  
`%AppData%\starsky\logs\starsky-{yyyy-MM-dd}.log`

- Minimum level: `Information`
- Format: `yyyy-MM-dd HH:mm:ss [LogLevel] Category: Message`
- Exception stack traces appended on the next line
- Thread-safe via a static `Lock`; write failures are silently ignored

---

## 7. Data Model

### 7.1 DesktopSettings

Persisted to `%AppData%\starsky\settings.json` as indented JSON.

| Property | Type | Default | Description |
|---|---|---|---|
| `Mode` | `RuntimeMode` | `Local` | `Local` (0) or `Remote` (1) |
| `RemoteBaseUrl` | `string` | `""` | URL of the remote server (Remote mode only) |
| `UpdateCheckEnabled` | `bool` | `true` | Whether to check for updates on startup |
| `LastUpdateWarningShown` | `DateTime?` | `null` | UTC timestamp of last update prompt (suppression window) |
| `Windows` | `List<SavedWindowState>` | `[]` | Per-window route + geometry state |

### 7.2 SavedWindowState

| Property | Type | Default | Description |
|---|---|---|---|
| `Route` | `string` | `"?f=/"` | URL path+query+fragment shown in this window |
| `Left` | `double` | `100` | Window left position (px) |
| `Top` | `double` | `100` | Window top position (px) |
| `Width` | `double` | `1200` | Window width (px) |
| `Height` | `double` | `800` | Window height (px) |
| `IsMaximized` | `bool` | `false` | Whether window was maximized |

### 7.3 UrlValidationResult

`record(bool Success, string? Error)` — returned by `RemoteUrlValidator.ValidateAsync`.

### 7.4 RuntimeMode

```csharp
public enum RuntimeMode { Local = 0, Remote = 1 }
```

---

## 8. File System Layout

### Velopack install tree (default)

| Path | Purpose |
|---|---|
| `%LocalAppData%\Starsky.Desktop\` | Velopack root — created by the installer |
| `%LocalAppData%\Starsky.Desktop\Starsky.Desktop.exe` | Velopack execution stub (launches `current\Starsky.Desktop.exe`) |
| `%LocalAppData%\Starsky.Desktop\current\` | Active version — all application files including `runtime-starsky-win-x64\` |
| `%LocalAppData%\Starsky.Desktop\packages\` | Velopack nupkg cache used by the update mechanism |

The installer defaults to `%LocalAppData%\Starsky.Desktop`. To install to a custom location, pass `--installto` on the command line:

```powershell
starsky-win-x64-desktop.exe --installto "D:\Apps\Starsky"
```

### Application data (all paths are fixed regardless of install location)

| Path | Purpose |
|---|---|
| `%AppData%\starsky\settings.json` | Desktop app settings |
| `%AppData%\starsky\appsettings.json` | Starsky backend config (Local mode) |
| `%AppData%\starsky\appsettings.local.json` | Machine-specific backend config overrides |
| `%AppData%\starsky\starsky.db` | SQLite database (Local mode) |
| `%AppData%\starsky\logs\starsky-{date}.log` | Daily log files |
| `%AppData%\starsky\webview2\` | WebView2 user profile (cache, cookies, etc.) |
| `%AppData%\starsky\thumbnailTempFolder\` | Thumbnail cache |
| `%LocalAppData%\starsky\tempFolder\` | Downloaded files (file-download feature) |
| `<exe dir>\runtime-starsky-win-x64\starsky.exe` | Bundled ASP.NET Core backend |

---

## 9. Build System

### Project Files

| File | Purpose |
|---|---|
| `windows/starsky-desktop-windows.slnx` | Solution (main app + tests) |
| `windows/Starsky.Desktop.csproj` | Main application project |
| `windows/starsky.Tests/starsky.Tests.csproj` | Test project |
| `windows/AssemblyInfo.cs` | `[assembly: InternalsVisibleTo("starsky.Tests")]` |

### MSBuild Targets (in `Starsky.Desktop.csproj`)

**`CopyStarskyRuntime`** (runs after build):  
Copies the compiled backend binary from one of these locations (in order):
1. `$(RepoRoot)\starsky\win-x64\`
2. `$(RepoRoot)\starskydesktop\runtime-starsky-win-x64\`

Output: `$(OutDir)runtime-starsky-win-x64\`

**`WarnMissingRuntime`**:  
Emits a build warning if the runtime directory does not exist after `CopyStarskyRuntime`.

### Publish

Self-contained, single-directory publish for `win-x64`:

```powershell
dotnet publish windows/Starsky.Desktop.csproj -c Release -r win-x64 --self-contained true -o ./publish-win
```

---

## 10. Test Suite

**Project:** `windows/starsky.Tests/starsky.Tests.csproj`  
**Framework:** xUnit 2.9.2, `net10.0-windows`, `UseWPF=true`  
**Total:** 52 tests, all passing

### Test Infrastructure

**`Helpers/FakeHttpMessageHandler`** — queue-based `HttpMessageHandler`. Accepts one or more `HttpResponseMessage` objects at construction; dequeues one per `SendAsync` call. Used to test HTTP-dependent services without network access.

### Test Classes

| Class | Tests | What is covered |
|---|---|---|
| `ApplicationPathsTests` | 5 | AppData/LocalAppData folder mapping; path structure; file extensions |
| `BackendServiceTests` | 5 | `StopAsync` / `Dispose` on unstarted service; all environment variable keys and values; `FindBackendExe` (found / not found) |
| `DesktopSettingsTests` | 2 | Default property values; full JSON round-trip serialisation |
| `FileDownloadServiceTests` | 3 | Happy path writes file to disk; sidecar failure still downloads main file; photo HTTP error propagates |
| `FileWatcherServiceTests` | 6 | Start/Stop/Dispose lifecycle permutations; temp folder creation |
| `NavigationServiceTests` | 6 | `IsAllowedOrigin` (localhost, matching remote, different host); `BuildStartUrl` (route appending, default route, trailing-slash trimming) |
| `PortFinderTests` | 3 | Returns positive port; port is bindable; successive calls return valid (not necessarily different) ports |
| `RemoteUrlValidatorTests` | 7 | Empty string; invalid scheme (`ftp://`); HTTP 200; HTTP 503; other HTTP status; request exception; trailing slash stripped |
| `RoutePersistenceServiceTests` | 6 | Empty list; save entry; save with geometry; list expansion with blanks; remove entry; clear all |
| `SettingsServiceTests` | 4 | Missing file (defaults); valid JSON; corrupt JSON (defaults); save-then-load round-trip |
| `UpdateServiceTests` | 4 | `UpdateCheckEnabled = false`; recent warning suppresses; `RecordWarningShown` persists timestamp; `ApplyUpdateAsync` throws without pending update |

### Running Tests

```powershell
# Run all tests
dotnet test windows/starsky.Tests/starsky.Tests.csproj -c Release

# With code coverage
dotnet test windows/starsky.Tests/starsky.Tests.csproj -c Release `
  --collect:"XPlat Code Coverage" `
  --results-directory TestResults/
```

---

## 11. CI/CD

### `desktop-wpf-pr-build-win.yml`

Triggered on push/PR to `master` when `windows/**` files change.  
Runner: `windows-latest` (.NET 10.0.400).

| Step | Command |
|---|---|
| Build | `dotnet build windows/starsky.Tests/starsky.Tests.csproj -c Release` |
| Test | `dotnet test ... --logger "trx;LogFileName=test-results.trx"` |
| Upload results | `actions/upload-artifact` → `test-results-windows` (`.trx` file) |
| Publish (verify) | `dotnet publish Starsky.Desktop.csproj -c Release -r win-x64 --self-contained true` |

Concurrency group: one run per branch; in-progress runs are cancelled on new push.

### `desktop-wpf-sonarqube-net.yml`

Adds OpenCover code-coverage collection and SonarQube static analysis on top of the same build/test steps.

### `desktop-release-on-tag-net-electron.yml`

Triggered on version tag pushes. Builds and publishes the desktop release binaries (Electron + WPF) as GitHub Release assets.

---

## 12. Keyboard Shortcuts Reference

| Shortcut | Scope | Action |
|---|---|---|
| Ctrl+N | MainWindow | Open new window |
| Ctrl+Shift+R | MainWindow | Reload all windows |
| F5 | MainWindow | Reload all windows |
| Ctrl+E | MainWindow | Edit current file in editor |
| Ctrl+Shift+K | MainWindow | Open application settings (injected into web app) |
| F12 | MainWindow | Open WebView2 Developer Tools |

---

## 13. Security Considerations

| Concern | Mechanism |
|---|---|
| External navigation | `NavigationStarting` cancels non-allowlisted navigations and redirects to system browser |
| External new-window | `NewWindowRequested` intercepts `target="_blank"`; only allowed origins open in-app |
| Local backend auth bypass | `app__NoAccountLocalhost=true` — only safe because the backend binds to `127.0.0.1`; no remote access |
| Remote URL validation | Scheme must be `http`/`https`; server must respond to `/api/health` |
| Credential storage | No credentials stored; Remote mode relies on the web app's own cookie-based session |
| Update integrity | Velopack validates packages via signatures from GitHub Releases |

---

## 14. Known Limitations & Non-Goals

| Item | Note |
|---|---|
| macOS / Linux | Not supported; WPF and WebView2 are Windows-only |
| System tray icon | Not implemented; app exits when last window closes |
| Windows notifications | No `ToastNotificationManager`; update prompt uses a WPF window |
| Direct P/Invoke | No Win32 API calls; all OS integration via .NET and WPF abstractions |
| UI testing | `MainWindow`, `SettingsWindow`, etc. are not covered by automated tests (require UI thread / FlaUI) |
| `WebViewEnvironmentService` | Not covered by automated tests (requires WebView2 Runtime installed) |
| `DailyFileLoggerProvider` | Not covered by automated tests |
| `FileWatcherService` debounce | Tested only for no-throw behaviour; actual file-change callback is logging-only (no upload/sync yet) |
| Multi-user | Single user per desktop installation; no account switching in the app |

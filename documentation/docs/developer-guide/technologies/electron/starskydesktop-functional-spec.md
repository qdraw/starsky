# Starsky Desktop Functional Specification

## Summary

- Starsky Desktop is an desktop shell around the Starsky web application.
- It supports two runtime modes: `local` and `remote`.
- In `local` mode it starts a bundled Starsky backend process, waits for it to become reachable, and then opens the UI against `http://localhost:{dynamicPort}`.
- In `remote` mode it skips the local backend and opens the UI against a configured remote Starsky server after validating the server URL.
- It persists window routes and reopens them on the next launch.
- It adds desktop-only behavior on top of the web app: splash/warmup flow, native menus, settings window, update warning window, local cache and logs, and opening indexed files in the operating system.
- It does not implement automatic in-place updates. When a version is outdated it only warns the user and points them to the release page.

## 1. Product purpose

Starsky Desktop exists to let users run or access Starsky as a native desktop application on Windows and macOS, with Linux packaging also present in the build configuration.

Functionally, it combines three concerns:

1. It hosts the existing Starsky web UI inside Electron browser windows.
2. It manages a local Starsky backend runtime when the app is used in local mode.
3. It adds desktop integrations that the browser version does not have.

The desktop app is not a separate product with different photo-management features. The actual catalog, browsing, search, metadata, import, and other domain behavior still comes from the Starsky backend and web UI. The desktop layer is responsible for bootstrapping, routing, persistence, operating-system integration, and local process management.

## 2. Runtime architecture

### 2.1 Processes

The application has these runtime pieces:

1. Electron main process.
2. One or more Electron renderer windows for the main UI.
3. A renderer window for settings.
4. Optional renderer windows for splash, error messages, and outdated-version warnings.
5. In local mode only: a spawned Starsky backend executable.

### 2.2 Local backend process

In local mode, the Electron main process starts a bundled Starsky executable.

Expected behavior:

- A free localhost port is selected at startup.
- The chosen port is stored in shared in-memory state for the current app session.
- The backend is launched detached, with a working directory based on the executable location.
- Standard output and error are written to the desktop logger.
- If the child process closes unexpectedly, the desktop app attempts to restart it.
- On app quit, the desktop app explicitly terminates the child process before exiting.

The backend path differs by environment:

- Development mode loads a runtime from a sibling `starsky` build output folder such as `osx-x64`, `osx-arm64`, `win-x64`, or `linux-x64`.
- Packaged mode loads a runtime from Electron `extraResources` under `runtime-starsky-{os}-{arch}`.

### 2.3 Local backend environment

When launching the bundled backend, the desktop app injects runtime-specific environment variables so the desktop session is self-contained.

Observed responsibilities of these variables:

- Bind the backend to `http://localhost:{dynamicPort}`.
- Store the database in the desktop app data folder.
- Store app settings in the desktop app data folder.
- Store temp and thumbnail-temp folders in the desktop app data folder.
- Enable localhost mode without account friction.
- Mark the backend as a desktop-local run.
- Reduce background thumbnail generation in development.
- Increase log verbosity in development.

The local database is stored as `starsky.db` in the desktop app data folder.

## 3. Startup flow

### 3.1 Main-process startup sequence

At startup, the main process performs these actions before the app is ready for interaction:

1. Create the log folder.
2. Register IPC handlers.
3. Configure desktop settings storage.
4. Start child-process management.
5. Create the temp folder.
6. Initialize the file watcher.

When Electron emits `ready`:

1. Build the application menu.
2. Build the macOS dock menu if supported.
3. Determine whether the app is in remote mode.
4. Show a splash window.
5. If local mode is active, wait for the local backend health endpoint to respond.
6. Restore the previous main window set.
7. Close the splash window.
8. Schedule the outdated-version warning check.

### 3.2 Splash and warmup behavior

The desktop app always shows a splash window during startup.

Behavior:

- The splash is small, frameless, transparent, and always on top.
- A hidden helper window is also created for Windows-specific behavior.
- In local mode, the splash stays visible while the app waits for the backend health endpoint.
- In remote mode, the splash is closed immediately after window restoration because there is no local backend to warm up.

### 3.3 Main-window warmup page

Each main window first loads a local HTML redirect page instead of going directly to Starsky.

That page is responsible for:

1. Asking the main process whether the app is in local or remote mode.
2. Asking for the app version.
3. Asking for the effective base URL.
4. Polling the target `/api/health` endpoint until the service is ready.
5. Running a version compatibility check.
6. Redirecting the browser window to the target Starsky route.

If the target service never becomes reachable, the warmup page shows an inline error.

If the version check fails with an outdated-client response, the warmup page shows an inline upgrade message instead of continuing.

## 4. Local mode and remote mode

### 4.1 Local mode

In local mode:

- The desktop app uses the bundled Starsky backend.
- The effective base URL is `http://localhost:{dynamicPort}`.
- The warmup logic waits for the local backend to pass a health check.
- The settings window disables direct editing of the URL field.
- The file watcher is configured for the local temp/download workspace.

### 4.2 Remote mode

In remote mode:

- No bundled backend warmup is required.
- The effective base URL comes from persisted settings.
- The settings window enables editing of the URL field.
- The app validates the provided remote URL before saving it.
- After a valid remote URL is saved, all main windows are closed and recreated so they reconnect cleanly.

### 4.3 Remote URL validation rules

The app accepts these remote URL patterns:

- A URL matching the configured URL regex.
- A URL matching the configured IP regex.
- Any URL starting with `http://localhost:`.

Validation behavior:

1. Remove a trailing slash before saving.
2. Request the target `/api/health` endpoint.
3. Treat HTTP `200` and `503` as acceptable validation responses.
4. Persist the URL only if validation succeeds.
5. Return a validation result object to the settings window.

If validation fails, the URL is not saved and the settings UI shows a failure message.

### 4.4 Switching modes

Switching between local and remote mode is destructive to current browser state on purpose.

When the mode changes:

1. The mode flag is saved.
2. The file watcher is reinitialized.
3. Remembered window routes are cleared.
4. Existing main windows are closed.
5. A fresh main window is created.
6. Settings windows are shown again once the new main window is ready.

## 5. Main windows and navigation

### 5.1 Main-window characteristics

Main windows are standard Electron browser windows with these functional properties:

- Window size and position are restored across launches.
- A dedicated persistent browser partition is used.
- Context isolation is enabled.
- A preload script exposes a limited IPC bridge.
- Spellcheck is enabled.
- A custom user-agent suffix `starsky/{major.minor}` is added.

### 5.2 Multiple windows

The app supports multiple main windows.

Users can open a new main window from the menu with `CmdOrCtrl+N`.

When multiple windows are restored, their positions are slightly offset so they do not fully overlap.

### 5.3 Remembered routes

The app remembers the last non-file URL visited in each main window.

Behavior:

- The remembered value is the path and query portion, not the full origin.
- The route is updated on normal navigations and hash/in-page navigations.
- The remembered entry is removed when the window closes.
- On next launch, the app recreates windows for the remembered routes.
- If no remembered route exists, the default route is `?f=/`.

This means the desktop layer does not persist full page state. It only persists enough routing information to reopen the same Starsky location.

### 5.4 Redirect behavior

After warmup, the redirect page rebuilds the final URL by combining:

- The effective base URL.
- The remembered relative route if present and syntactically safe.

The final browser navigation is then performed with `window.location.assign`.

### 5.5 Secondary windows

The main web UI is allowed to open additional windows. These windows reuse the same persistent session partition and preload bridge.

## 6. Settings window

### 6.1 Purpose

The settings window is a separate desktop-native renderer page used for desktop-specific settings, not general Starsky application settings.

It is opened from the application menu through `Settings -> Connection Settings` and uses its own remembered window geometry.

### 6.2 Implemented settings

The settings UI currently implements these behaviors:

1. Toggle between `Local` and `Remote` runtime mode.
2. View the current effective remote URL.
3. Edit and validate a new remote URL when remote mode is selected.
4. Enable or disable update checks.

### 6.3 Local and remote toggle behavior

At load time, the settings page queries the current mode through IPC.

Behavior:

- Local selected: the URL field is disabled.
- Remote selected: the URL field is enabled and immediately populated from settings.
- All radio buttons are initially disabled and only become interactive once the initial state is returned.

### 6.4 Remote URL field behavior

The remote URL field:

- Requests the current value on load.
- Sends a save request on change.
- Displays `Setting is saved` on successful validation.
- Displays `FAIL setting is not valid and NOT saved` on failed validation.

### 6.5 Update policy toggle behavior

The settings page exposes a simple enabled or disabled switch for update checks.

Behavior:

- When no value exists yet, the app behaves as enabled by default.
- Changing the switch immediately persists the new value.
- The controls are disabled until the current value is received from IPC.

## 7. Menus and desktop commands

### 7.1 Application menu

The app defines a native application menu with platform-aware labeling and behavior.

High-level functions exposed through the menu:

1. Open a new main window.
2. Edit the current file in an editor.
3. Open the connection settings window.
4. Forward a shortcut into the web app for app-specific settings.
5. Reload all windows.
6. Open developer tools.
7. Open the current page in the external browser.
8. Open documentation.
9. Open the release overview page.

### 7.2 Desktop shortcut forwarding

Some menu items do not implement behavior directly in Electron. Instead they synthesize keypresses into the current web contents so the embedded Starsky UI handles them.

Observed forwarded shortcuts:

- `CmdOrCtrl+E` in local mode forwards the key for in-app edit behavior.
- `CmdOrCtrl+Shift+K` forwards the keybinding for Starsky app settings inside the web UI.

### 7.3 External links

The app can open these links through the OS shell:

- Documentation website.
- GitHub latest release page.
- The currently focused Starsky page in the system browser.

## 8. Desktop-only file opening workflow

### 8.1 Purpose

In remote mode, the desktop app can retrieve the currently viewed indexed file and open it on the local machine in the default operating-system application.

This is desktop-only functionality layered on top of the web UI.

### 8.2 Trigger behavior

When the user chooses `Edit file in Editor` from the menu:

- In local mode, the app forwards the `CmdOrCtrl+E` shortcut into the web UI instead of using the desktop download/open workflow.
- In remote mode, the app executes the desktop workflow below.

### 8.3 Download and open workflow

The remote-mode desktop workflow is:

1. Read the current Starsky file path from the focused window URL query parameter `f`.
2. Request detail data from `/starsky/api/index?f={path}`.
3. Validate that the response is a detail-view payload.
4. Create parent folders in the desktop temp workspace for the file's parent directory.
5. If sidecar data exists, download the first sidecar file from `/starsky/api/download-sidecar?f={path}`.
6. Download the binary file from `/starsky/api/download-photo?isThumbnail=false&f={path}&cache=false`.
7. Save the binary first as `.tmp`, then rename it to its final name.
8. Open the downloaded file with the operating system default handler.

If opening the file fails, the app shows an error window.

### 8.4 Local workspace for downloaded files

The file download workspace mirrors the Starsky parent directory structure under a desktop-managed local folder.

This workspace is also the target of the file watcher described below.

## 9. File watcher and desktop sync behavior

The desktop app initializes a file watcher against the local workspace used for downloaded and edited files.

Observed behavior:

- Existing watcher registrations are removed before a new watcher is installed.
- Parent folders are created before watch startup.
- The watch target is based on the resolved local parent-disk path.
- The watcher reacts to changed files only.
- On change, it delegates to a dedicated action handler.

Functionally, this means the desktop app is designed to notice when a locally opened file changes after the user edits it and then perform follow-up handling through the change-action pipeline.

## 10. Update and version behavior

### 10.1 Warmup version compatibility check

Before redirecting to Starsky, the warmup page posts to `/api/health/version?version={desktopVersion}`.

Observed behavior:

- HTTP `200`: continue startup.
- HTTP `400`: show an inline upgrade message inside the warmup page.
- Other failures: show an alert and stop.

### 10.2 Outdated-version warning window

Separately from the inline warmup version check, the app may show an outdated-version warning window after startup.

The warning is based on `/api/health/check-for-updates?currentVersion={desktopVersion}`.

Observed behavior:

- HTTP `202` means an update warning should be shown.
- Any other status means no warning is shown.
- The warning window tells the user to open `Help -> Release overview` and manually download a new version.

### 10.3 Update policy rules

The update warning is suppressed when either of these conditions is true:

1. The user explicitly disabled update checks.
2. The user already closed the update warning within the last 5760 minutes, which is 4 days.

When the warning window closes, the app stores the current timestamp as the latest checked date.

### 10.4 No automatic updater

The current desktop app does not include an auto-update engine, background installer, or silent patching mechanism.

The only supported update flow is manual download and reinstall.

## 11. Persistence and storage

### 11.1 Settings storage

Desktop settings are stored through `electron-settings` in a prettified JSON file named `starksy-app-settings.json`.

Persisted settings include at least:

- Local or remote mode flag.
- Remote base URL.
- Update policy enabled or disabled.
- Last update-warning display timestamp.
- Remembered routes per open window.

### 11.2 App data folder

The desktop app stores its data in an OS-specific per-user application data directory.

Resolved base folders:

- macOS: `~/Library/Application Support/starsky`
- Windows: `%USERPROFILE%/AppData/Roaming/starsky`
- Linux: `~/.config/starsky`

This base folder is used for:

- `logs/`
- `starsky.db`
- `appsettings.json`
- `appsettings.local.json`
- `tempFolder/`
- `thumbnailTempFolder/`

### 11.3 Temp folder

The desktop app also creates an OS temp folder under the Electron app name for additional transient desktop usage.

## 12. Logging and diagnostics

The desktop app writes logs to both console and file.

Observed behavior:

- Log level is `info`.
- Warnings are also written.
- Daily file logs are written under the desktop `logs` folder.
- Child-process stdout and stderr are captured into the logger.

## 13. Security model

### 13.1 Renderer isolation

The desktop app enables context isolation and exposes only a narrow preload API to renderer code.

The exposed API allows only whitelisted IPC channels.

### 13.2 Allowed IPC channels

The preload bridge currently exposes these channels:

- location mode
- location URL
- app version
- update policy
- default image application

### 13.3 Navigation restrictions

The app intercepts renderer navigation and prevents arbitrary origin changes.

Allowed navigations are limited to:

- Local file URLs used by desktop-owned renderer pages.
- The configured remote origin.
- Any `http://localhost:` origin.

All other navigations are prevented.

### 13.4 External content model

The main window intentionally loads a remote HTTP origin when the user connects to a server. The desktop security boundary is therefore based on origin restriction, preload isolation, and explicit IPC whitelisting rather than on fully local-only content.

## 14. Windows and dialogs

The desktop app uses distinct window types for different responsibilities:

1. Main windows for Starsky itself.
2. A settings window for desktop configuration.
3. A splash window for startup.
4. An error window for desktop workflow failures, such as failing to open a downloaded file.
5. An outdated-version warning window.

Each type is tracked separately so the main process can manage lifecycle and restore behavior.

## 15. Packaging and distribution behavior

### 15.1 Build targets

Configured targets include:

- macOS DMG
- Windows NSIS installer
- Linux ZIP

### 15.2 Artifact naming

Artifacts are named with the pattern `starsky-{os}-{arch}-desktop.{ext}`.

### 15.3 Bundled resources

Packaged builds include an external Starsky runtime under `runtime-starsky-{os}-{arch}`.

### 15.4 Code-signing and trust model

The repository documentation explicitly states that Apple certificates are not included.

User-visible implication:

- macOS users will receive Gatekeeper warnings.
- The update story is manual.
- Trust is based on open-source distribution and manual installation steps rather than signed auto-updating binaries.

## 16. Known implementation gaps and dormant behavior

These are important if the app is being rewritten and needs parity with what actually works today rather than with what some source files suggest.

### 16.1 Default image application setting is not functionally complete

The codebase contains a preload IPC channel and renderer script for a `DEFAULT_IMAGE_APPLICATION` setting.

However, in the current implementation:

- The settings HTML does not render the expected UI elements for this feature.
- The main-process IPC bridge does not register a handler for that channel.

Result:

- This should be treated as dormant or incomplete behavior, not as a working feature that must be preserved exactly.

### 16.2 The desktop app is a shell, not the domain implementation

Most user-visible photo-management behavior is still provided by Starsky itself after the browser window redirects to the web UI.

Any rewrite must preserve both:

1. The desktop shell behavior described in this document.
2. Compatibility with the Starsky HTTP endpoints and routes that the shell expects.

## 17. Rewrite requirements checklist

If this desktop app is rewritten in another tool, the replacement should preserve these externally visible behaviors:

1. Support local mode with an embedded Starsky runtime.
2. Support remote mode with a validated user-configured base URL.
3. Remember and restore one or more open Starsky routes across launches.
4. Show a startup/warmup flow until the target service is reachable.
5. Block startup when the desktop and server versions are incompatible.
6. Provide a desktop settings UI for connection mode, remote URL, and update policy.
7. Provide native menus for opening windows, settings, docs, release notes, and developer tools.
8. Provide desktop-only file retrieval and open-in-default-application behavior for the active Starsky item.
9. Watch the local edited-file workspace for changes.
10. Store desktop data, logs, DB, and temp files in a per-user app-data location.
11. Restrict renderer navigation to approved origins.
12. Preserve manual-update warning behavior even if automatic updating is not introduced.

## 18. Endpoint contract expected by the desktop shell

The current desktop shell depends on these backend HTTP endpoints:

1. `GET /api/health`
2. `POST /api/health/version?version={desktopVersion}`
3. `GET /api/health/check-for-updates?currentVersion={desktopVersion}`
4. `GET /starsky/api/index?f={path}`
5. `GET /starsky/api/download-sidecar?f={path}`
6. `GET /starsky/api/download-photo?isThumbnail=false&f={path}&cache=false`

These endpoints form the minimum server contract required by the current desktop shell.
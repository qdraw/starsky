# Starsky Desktop — Windows (WPF)

A native Windows desktop shell that wraps the Starsky photo-management web app inside a WebView2 browser control.

## List of [Starsky](../readme.md) Projects
* [starsky (sln)](../starsky/readme.md) _database photo index & import project_
* [starsky-tools](../starsky-tools/readme.md) _nodejs add-on tasks_
* __[windows](../windows/readme.md) Windows WPF Desktop Application__
* [Changelog](../history.md) _Release notes and history_

## Requirements

| Requirement | Version |
|---|---|
| Windows | 10 / 11 (x64) |
| .NET SDK | 10.0 |
| WebView2 Runtime | Any recent version |

## Build

### 1. Build the Starsky backend first

```powershell
../starsky/build.sh --Runtime win-x64
```

This produces `starsky/win-x64/` which the MSBuild target copies into the desktop app's output directory automatically.

### 2. Build the desktop app

```powershell
dotnet build windows/Starsky.Desktop.csproj -c Release
```

### 3. Publish (self-contained, single directory)

```powershell
dotnet publish windows/Starsky.Desktop.csproj -c Release -r win-x64 --self-contained true -o ./publish-win
```

### 4. Package with Velopack (optional, for installers)

```powershell
./windows/build-velopack.ps1
```

## Run Tests

```powershell
dotnet test windows/starsky.Tests/starsky.Tests.csproj -c Release
```

With code coverage:

```powershell
dotnet test windows/starsky.Tests/starsky.Tests.csproj -c Release `
  --collect:"XPlat Code Coverage" `
  --results-directory TestResults/
```

## Connection Modes

**Local (default):** Launches the bundled `starsky.exe` backend on a free port; no server setup required.

**Remote:** Connects to an existing Starsky server at a user-configured URL. Switch modes in Settings without restarting.

## Data Locations

| Path | Purpose |
|---|---|
| `%AppData%\starsky\settings.json` | Desktop app settings |
| `%AppData%\starsky\appsettings.json` | Backend config (Local mode) |
| `%AppData%\starsky\starsky.db` | SQLite database (Local mode) |
| `%AppData%\starsky\logs\starsky-{date}.log` | Daily log files |
| `%AppData%\starsky\webview2\` | WebView2 user profile |
| `%LocalAppData%\starsky\tempFolder\` | Downloaded files (remote edit) |

## Install Location (Velopack)

The installer defaults to `%LocalAppData%\Starsky.Desktop`. To install elsewhere:

```powershell
starsky-win-x64-desktop.exe --installto "D:\Apps\Starsky"
```

## Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| Ctrl+N | Open new window |
| Ctrl+Shift+R / F5 | Reload all windows |
| Ctrl+E | Edit current file in editor |
| Ctrl+Shift+K | Open application settings |
| F12 | Open WebView2 Developer Tools |

## Architecture

See [SPEC.md](SPEC.md) for the full technical specification covering services, data model, startup/shutdown sequence, and CI/CD configuration.

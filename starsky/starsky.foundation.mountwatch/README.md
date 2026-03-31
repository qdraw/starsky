# Starsky Mount Watcher Implementation

## Overview

The Mount Watcher is a cross-platform CLI tool that monitors external drive and camera mount events,
automatically detecting camera storage (DCIM folders) and triggering the Starsky importer service.

## Architecture

### Foundation Library: `starsky.foundation.mountwatch`

The foundation library provides core abstractions and platform-specific implementations:

#### Interfaces

- **`IMountWatcher`**: Abstraction for OS-specific mount detection
    - `Start()`: Begin listening for mount events
    - `Stop()`: Stop listening for mount events
    - `GetMountedVolumes()`: Get currently mounted volumes
    - `event MountDetected`: Fired when a new mount is detected

- **`IMountDetector`**: Detects camera storage on mounted volumes
    - `HasCameraStorage(string mountPath)`: Check if mount has DCIM folder
    - `GetCameraStoragePaths(string mountPath)`: Get camera storage paths

- **`IMountWatcherFactory`**: Creates OS-specific mount watchers
    - `CreateMountWatcher()`: Return appropriate watcher for OS

- **`IServiceInstaller`**: Installs / uninstalls the watcher as an OS service
    - `InstallAsync(string executablePath)`: Write launchd plist / systemd unit / Windows Service
    - `UninstallAsync()`: Remove the OS service definition

#### Platform-Specific Implementations

1. **MacMountWatcher** (`MacMountWatcher.cs`)
    - Uses **DiskArbitration** framework via P/Invoke
    - `DARegisterDiskAppearedCallback` for instant event-driven notifications
    - `CFRunLoop` keeps the event loop alive on a background thread
    - Falls back to polling `/Volumes` if DiskArbitration is unavailable

2. **WindowsMountWatcher** (`WindowsMountWatcher.cs`)
    - Uses **WMI** (`ManagementEventWatcher`) for event-driven drive detection
    - Subscribes to `Win32_VolumeChangeEvent WHERE EventType = 2`
    - Falls back to polling `DriveInfo.GetDrives()` if WMI is unavailable
    - WMI calls are guarded with `[SupportedOSPlatform("windows")]`

3. **LinuxMountWatcher** (`LinuxMountWatcher.cs`)
    - Uses **udev** (libudev.so.1) P/Invoke for event-driven block device notifications
    - Monitors `block` subsystem via `udev_monitor`
    - Falls back to polling `/proc/mounts` if libudev is unavailable

#### Core Services

- **`MountDetector`**: Detects camera storage by looking for DCIM folders
    - Handles both uppercase and lowercase folder names
    - Returns empty list on errors (permissions, IO exceptions)

- **`MountWatcherFactory`**: Uses `OperatingSystem` to select appropriate watcher
    - Creates a new instance for each call (no caching)

- **`ServiceInstaller`**: Installs the watcher as an OS service
    - macOS: writes `~/Library/LaunchAgents/nl.qdraw.mountwatcher.plist`
    - Windows: calls `sc.exe create` to register a Windows Service
    - Linux: writes `/etc/systemd/system/starsky-mountwatcher.service`
        - Falls back to `~/.config/systemd/user/` when root is unavailable

- **`MountWatcherCli`**: Orchestrates mount watching and importing
    - Handles `--install`, `--uninstall`, `--help` args before starting the watcher
    - Listens for mount detected events
    - Checks for camera storage using `IMountDetector`
    - Prevents duplicate imports using path-based HashSet
    - Delegates import work to existing `ImportCli` service

### CLI Project: `starskymountwatchercli`

```csharp
public static async Task Main(string[] args)
{
    // Setup DI, AppSettings, database…
    var serviceInstaller = new ServiceInstaller(console, webLogger);

    var service = new MountWatcherCli(
        import, appSettings, console, webLogger,
        exifToolDownload, geoFileDownload,
        mountDetector, mountWatcherFactory,
        cameraStorageDetector, serviceInstaller);

    if (!await service.StartWatcher(args))
        throw new WebApplicationException("Mount watcher failed to start");
}
```

## CLI Usage

```bash
# Run the watcher
starskymountwatchercli --verbose

# Install as OS service (writes launchd/systemd/Windows Service config)
starskymountwatchercli --install

# Remove the OS service
starskymountwatchercli --uninstall

# Show help
starskymountwatchercli --help
```

## OS-Specific Setup

### macOS (launchd) — via `--install`

`--install` writes `~/Library/LaunchAgents/nl.qdraw.mountwatcher.plist` and prints:

```
LaunchAgent installed: /Users/<you>/Library/LaunchAgents/nl.qdraw.mountwatcher.plist
To load now: launchctl load <path>
Note: Grant Full Disk Access to the executable in System Preferences.
```

Generated plist content:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>nl.qdraw.mountwatcher</string>
    <key>ProgramArguments</key>
    <array>
        <string>/path/to/starskymountwatchercli</string>
        <string>--verbose</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
    <key>StandardOutPath</key>
    <string>~/Library/Logs/starsky/mountwatcher.log</string>
    <key>StandardErrorPath</key>
    <string>~/Library/Logs/starsky/mountwatcher.error.log</string>
</dict>
</plist>
```

**Load the service after install:**

```bash
launchctl load ~/Library/LaunchAgents/nl.qdraw.mountwatcher.plist
```

Permissions required: **Full Disk Access** (System Preferences → Privacy & Security)

---

### Windows Service — via `--install`

`--install` calls `sc.exe create` and prints:

```
Windows Service installed: nl.qdraw.mountwatcher
To start: sc start nl.qdraw.mountwatcher
```

Start/stop manually:

```cmd
sc start nl.qdraw.mountwatcher
sc stop nl.qdraw.mountwatcher
```

---

### Linux (systemd) — via `--install`

`--install` writes `/etc/systemd/system/starsky-mountwatcher.service`
(or `~/.config/systemd/user/starsky-mountwatcher.service` if run without root) and prints:

```
systemd unit installed: /etc/systemd/system/starsky-mountwatcher.service
To enable and start:
  sudo systemctl daemon-reload
  sudo systemctl enable starsky-mountwatcher
  sudo systemctl start starsky-mountwatcher
```

Generated unit file:

```ini
[Unit]
Description=Starsky Mount Watcher
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/starskymountwatchercli --verbose
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
```

---

### Uninstalling

```bash
# All platforms
starskymountwatchercli --uninstall
```

Or manually:

```bash
# macOS
launchctl unload ~/Library/LaunchAgents/nl.qdraw.mountwatcher.plist
rm ~/Library/LaunchAgents/nl.qdraw.mountwatcher.plist

# Linux
sudo systemctl stop starsky-mountwatcher
sudo systemctl disable starsky-mountwatcher
sudo rm /etc/systemd/system/starsky-mountwatcher.service
sudo systemctl daemon-reload
```

## Complexity Metrics

All methods maintain low cyclomatic and cognitive complexity (SonarQube standards):

- **MountDetector methods**: Complexity ≤ 5
- **OS-specific watchers**: Complexity ≤ 10
- **ServiceInstaller methods**: Complexity ≤ 5
- **MountWatcherCli**: Event handlers complexity ≤ 10

## Duplicate Prevention

The `MountWatcherCli` prevents duplicate imports using:

```csharp
private readonly HashSet<string> _processedPaths = new();
private const int DuplicateCheckWindowSeconds = 60;
```

## Integration with ImportCli

The mount watcher delegates actual import work to the existing `ImportCli` service:

```csharp
var importCli = new ImportCli(
    _importService, _appSettings, _console, _logger,
    _exifToolDownload, _geoFileDownload, _cameraStorageDetector);

var result = await importCli.Importer(new[] { cameraPath, "--recursive" });
```

## Testing

Comprehensive unit tests in `starskytest`:

- **MountDetectorTest**: Camera storage detection (filesystem)
- **MacMountWatcherTest**: macOS-specific watcher
- **WindowsMountWatcherTest**: Windows-specific watcher
- **LinuxMountWatcherTest**: Linux-specific watcher
- **MountWatcherFactoryTest**: Factory creates correct watcher per OS
- **MountWatcherCliTest**: `--install`, `--uninstall`, `--help`, watcher startup
- **ServiceInstallerTest**: plist / unit file generation; install/uninstall on real OS

Mock implementations:

- **FakeMountDetector**: Returns no camera storage
- **FakeMountWatcherFactory**: Returns a no-op fake watcher
- **FakeServiceInstaller**: Captures install/uninstall calls

## Logging

```csharp
_logger.LogInformation($"Mount detected: {e.MountPath}");
_logger.LogInformation($"Camera storage detected on {e.MountPath}");
_logger.LogInformation($"Starting import from {cameraPath}");
_logger.LogError($"Import failed for {cameraPath}: {ex.Message}");
```

## Project Files

```
starsky.foundation.mountwatch/
├── Interfaces/
│   ├── IMountWatcher.cs
│   ├── IMountDetector.cs
│   └── IServiceInstaller.cs
├── Services/
│   ├── MountDetector.cs
│   ├── MacMountWatcher.cs        ← DiskArbitration (P/Invoke) + polling fallback
│   ├── WindowsMountWatcher.cs    ← WMI events + polling fallback
│   ├── LinuxMountWatcher.cs      ← udev (P/Invoke) + polling fallback
│   ├── MountWatcherFactory.cs
│   ├── IMountWatcherFactory.cs
│   ├── MountWatcherCli.cs        ← --install / --uninstall / --help
│   └── ServiceInstaller.cs       ← launchd / systemd / Windows Service
└── starsky.foundation.mountwatch.csproj

starskymountwatchercli/
├── Program.cs
├── Properties/
│   └── default-init-launchSettings.json
└── starskymountwatchercli.csproj

starskytest/
├── starsky.foundation.mountwatch/
│   └── Services/
│       ├── MountDetectorTest.cs
│       ├── MacMountWatcherTest.cs
│       ├── WindowsMountWatcherTest.cs
│       ├── LinuxMountWatcherTest.cs
│       ├── MountWatcherFactoryTest.cs
│       ├── MountWatcherCliTest.cs
│       └── ServiceInstallerTest.cs
└── FakeMocks/
    ├── FakeMountDetector.cs
    ├── FakeMountWatcherFactory.cs
    └── FakeServiceInstaller.cs
```

## Building and Running

```bash
# Build
dotnet build starsky.foundation.mountwatch/starsky.foundation.mountwatch.csproj
dotnet build starskymountwatchercli/starskymountwatchercli.csproj

# Test
dotnet test starskytest/starskytest.csproj --filter "FullyQualifiedName~MountWatch"

# Run (foreground watcher)
dotnet run --project starskymountwatchercli -- --verbose

# Install as OS service
dotnet run --project starskymountwatchercli -- --install

# Uninstall
dotnet run --project starskymountwatchercli -- --uninstall
```

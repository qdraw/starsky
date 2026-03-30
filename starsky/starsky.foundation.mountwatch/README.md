# Starsky Mount Watcher Implementation

## Overview

The Mount Watcher is a cross-platform CLI tool that monitors external drive and camera mount events, automatically detecting camera storage (DCIM folders) and triggering the Starsky importer service.

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

#### Platform-Specific Implementations

1. **MacMountWatcher** (`MacMountWatcher.cs`)
   - Polls `/Volumes` directory for mount changes
   - Uses polling interval: 2 seconds
   - Scans existing volumes on startup

2. **WindowsMountWatcher** (`WindowsMountWatcher.cs`)
   - Uses `DriveInfo.GetDrives()` for drive detection
   - Filters by `IsReady` status
   - Polling interval: 2 seconds

3. **LinuxMountWatcher** (`LinuxMountWatcher.cs`)
   - Reads `/proc/mounts` for mount point changes
   - Filters out system mounts (`/sys`, `/proc`, `/dev`, etc.)
   - Includes user mount paths (`/media`, `/mnt`, `/home`)
   - Polling interval: 2 seconds

#### Core Services

- **`MountDetector`**: Detects camera storage by looking for DCIM folders
  - Handles both uppercase and lowercase folder names
  - Returns empty list on errors (permissions, IO exceptions)

- **`MountWatcherFactory`**: Uses `OperatingSystem` to select appropriate watcher
  - Creates new instance for each call (no caching)

- **`MountWatcherCli`**: Orchestrates mount watching and importing
  - Listens for mount detected events
  - Checks for camera storage using `IMountDetector`
  - Prevents duplicate imports using path-based HashSet
  - Delegates import work to existing `ImportCli` service

### CLI Project: `starskymountwatchercli`

Entry point for the mount watcher application:

```csharp
public static async Task Main(string[] args)
{
    // Setup
    new ArgsHelper().SetEnvironmentByArgs(args);
    var services = new ServiceCollection();
    services = await SetupAppSettings.FirstStepToAddSingleton(services);
    RegisterDependencies.Configure(services);
    
    // Services
    var import = serviceProvider.GetRequiredService<IImport>();
    var mountDetector = serviceProvider.GetRequiredService<IMountDetector>();
    var mountWatcherFactory = serviceProvider.GetRequiredService<IMountWatcherFactory>();
    
    // Start watching
    var service = new MountWatcherCli(...);
    if (!await service.StartWatcher(args))
    {
        throw new WebApplicationException("Mount watcher failed to start");
    }
}
```

## Complexity Metrics

All methods maintain low cyclomatic and cognitive complexity:

- **MountDetector methods**: Complexity ≤ 5
- **OS-specific watchers**: Main loop complexity ≤ 8
- **MountWatcherCli**: Event handlers complexity ≤ 10

Code follows single-responsibility principle with extracted helper methods.

## Duplicate Prevention

The `MountWatcherCli` prevents duplicate imports using:

```csharp
private readonly HashSet<string> _processedPaths = new();
private const int DuplicateCheckWindowSeconds = 60;

// Remove from set after 60 seconds to allow re-import if needed
_ = Task.Delay(TimeSpan.FromSeconds(DuplicateCheckWindowSeconds))
    .ContinueWith(_ => _processedPaths.Remove(cameraPath));
```

## Integration with ImportCli

The mount watcher delegates actual import work to the existing `ImportCli` service:

```csharp
var importCli = new ImportCli(
    _importService,
    _appSettings,
    _console,
    _logger,
    _exifToolDownload,
    _geoFileDownload,
    _cameraStorageDetector);

var result = await importCli.Importer(importArgs);
```

This ensures consistency with the existing import pipeline and avoids code duplication.

## Testing

Comprehensive unit tests in `starskytest`:

- **MountDetectorTest**: Tests camera storage detection with real filesystem
- **MacMountWatcherTest**: Tests macOS-specific watcher
- **WindowsMountWatcherTest**: Tests Windows-specific watcher
- **LinuxMountWatcherTest**: Tests Linux-specific watcher
- **MountWatcherFactoryTest**: Tests factory creates correct watcher per OS
- **MountWatcherCliTest**: Tests CLI service initialization

Mock implementations:
- **FakeMountDetector**: Returns no camera storage
- **FakeMountWatcherFactory**: Returns fake watcher that does nothing

## Logging

All mount detection events are logged to `IWebLogger`:

```csharp
_logger.LogInformation($"Mount detected: {e.MountPath}");
_logger.LogInformation($"Camera storage detected on {e.MountPath}");
_logger.LogInformation($"Starting import from {cameraPath}");
_logger.LogError($"Import failed for {cameraPath}: {ex.Message}");
```

## OS-Specific Setup

### macOS (launchd)

Create `~/Library/LaunchAgents/com.starsky.mountwatcher.plist`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.starsky.mountwatcher</string>
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
    <string>/var/log/starsky-mountwatch.log</string>
    <key>StandardErrorPath</key>
    <string>/var/log/starsky-mountwatch-error.log</string>
</dict>
</plist>
```

Permissions required: **Full Disk Access**

### Windows Service

Can be installed as Windows Service with appropriate configuration.

### Linux (systemd)

Create `/etc/systemd/system/starsky-mountwatcher.service`:

```ini
[Unit]
Description=Starsky Mount Watcher
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/starskymountwatchercli --verbose
Restart=on-failure
User=starsky

[Install]
WantedBy=multi-user.target
```

## Performance Considerations

- **Polling interval**: 2 seconds balances responsiveness vs. CPU usage
- **No-blocking design**: Events are handled async; import runs in background
- **Memory efficient**: Uses HashSet for O(1) duplicate detection
- **Path normalization**: Prevents duplicate processing of same path

## Error Handling

- **Permissions errors**: Caught and logged; watcher continues
- **IO errors**: Caught during mount detection; defaults to no camera storage
- **Import failures**: Logged but don't stop watcher
- **Directory access**: Suppressed exceptions during scanning

## Future Enhancements

1. **macOS DiskArbitration**: Replace polling with native event notifications
2. **Linux udev**: Integrate udev rules for event-driven detection
3. **Windows WMI**: Use WMI events instead of polling
4. **Configurable paths**: Allow custom DCIM path detection
5. **Retry logic**: Implement exponential backoff for failed imports
6. **Move/delete after import**: Optional automatic cleanup after import

## Project Files

```
starsky.foundation.mountwatch/
├── Interfaces/
│   ├── IMountWatcher.cs
│   └── IMountDetector.cs
├── Services/
│   ├── MountDetector.cs
│   ├── MacMountWatcher.cs
│   ├── WindowsMountWatcher.cs
│   ├── LinuxMountWatcher.cs
│   ├── MountWatcherFactory.cs
│   ├── IMountWatcherFactory.cs
│   └── MountWatcherCli.cs
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
│       └── MountWatcherCliTest.cs
└── FakeMocks/
    ├── FakeMountDetector.cs
    └── FakeMountWatcherFactory.cs
```

## Building and Running

```bash
# Build
dotnet build starsky.foundation.mountwatch/starsky.foundation.mountwatch.csproj
dotnet build starskymountwatchercli/starskymountwatchercli.csproj

# Test
dotnet test starskytest/starskytest.csproj --filter "FullyQualifiedName~MountWatch"

# Run
dotnet run --project starskymountwatchercli -- --verbose
```


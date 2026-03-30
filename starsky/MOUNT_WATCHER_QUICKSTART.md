# 🚀 Starsky Mount Watcher - Quick Start Guide

## Overview

The Mount Watcher automatically imports photos from external drives/camera storage by:
1. Monitoring for new mount events
2. Detecting camera storage (DCIM folders)
3. Triggering the import process
4. Preventing duplicate imports

## Building

```bash
cd /Users/dion/data/git/starsky/starsky

# Build foundation library
dotnet build starsky.foundation.mountwatch/starsky.foundation.mountwatch.csproj

# Build CLI application
dotnet build starskymountwatchercli/starskymountwatchercli.csproj

# Build all tests
dotnet build starskytest/starskytest.csproj
```

**Result**: ✅ All projects compile with 0 errors

## Testing

```bash
# Run mount watcher tests
dotnet test starskytest/starskytest.csproj \
  --filter "FullyQualifiedName~starsky.foundation.mountwatch"

# Test specific component
dotnet test starskytest/starskytest.csproj \
  --filter "FullyQualifiedName~MountDetectorTest"
```

## Running the Watcher

### Development (Verbose Mode)

```bash
dotnet run --project starskymountwatchercli -- --verbose
```

This will:
- Start listening for mount events
- Print mount detection events to console
- Log all operations to console
- Keep running until manually stopped (Ctrl+C)

### Production Setup

#### macOS (launchd)

1. **Build self-contained app**:
```bash
dotnet publish starskymountwatchercli/ \
  -c Release \
  -r osx-arm64 \
  --self-contained
```

2. **Create launchd configuration**:
```bash
mkdir -p ~/Library/LaunchAgents
nano ~/Library/LaunchAgents/com.starsky.mountwatcher.plist
```

3. **Paste configuration**:
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

4. **Grant Full Disk Access**:
   - System Preferences → Security & Privacy → Full Disk Access
   - Add `starskymountwatchercli` executable

5. **Enable service**:
```bash
launchctl load ~/Library/LaunchAgents/com.starsky.mountwatcher.plist
```

#### Linux (systemd)

1. **Build self-contained app**:
```bash
dotnet publish starskymountwatchercli/ \
  -c Release \
  -r linux-arm64 \
  --self-contained
```

2. **Create systemd service**:
```bash
sudo nano /etc/systemd/system/starsky-mountwatcher.service
```

3. **Paste configuration**:
```ini
[Unit]
Description=Starsky Mount Watcher
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/starskymountwatchercli --verbose
Restart=on-failure
RestartSec=10
User=starsky
StandardOutput=append:/var/log/starsky-mountwatch.log
StandardError=append:/var/log/starsky-mountwatch-error.log

[Install]
WantedBy=multi-user.target
```

4. **Enable service**:
```bash
sudo systemctl daemon-reload
sudo systemctl enable starsky-mountwatcher
sudo systemctl start starsky-mountwatcher
```

5. **Monitor logs**:
```bash
journalctl -u starsky-mountwatcher -f
```

#### Windows

1. **Build self-contained app**:
```cmd
dotnet publish starskymountwatchercli/ ^
  -c Release ^
  -r win-x64 ^
  --self-contained
```

2. **Install as Windows Service**:
```powershell
# Using SC.exe
sc create "StarskyMountWatcher" ^
  binPath= "C:\path\to\starskymountwatchercli.exe --verbose" ^
  start= auto

# Or using PowerShell
New-Service -Name "StarskyMountWatcher" `
  -BinaryPathName "C:\path\to\starskymountwatchercli.exe --verbose" `
  -DisplayName "Starsky Mount Watcher" `
  -StartupType Automatic
```

3. **Start service**:
```powershell
Start-Service -Name "StarskyMountWatcher"
```

## How It Works

### Flow Diagram
```
┌─────────────────────────────────┐
│ Mount Watcher Started           │
│ (Registers OS listener)         │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│ Scan Existing Volumes           │
│ (Establish baseline)            │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│ Polling Loop (2s interval)      │
│ Check for mount changes         │
└────────────┬────────────────────┘
             │
             ▼
         New Mount? ──NO──► Continue polling
             │
            YES
             │
             ▼
┌─────────────────────────────────┐
│ Check for Camera Storage        │
│ (Look for DCIM folder)          │
└────────────┬────────────────────┘
             │
         Has DCIM? ──NO──► Continue polling
             │
            YES
             │
             ▼
┌─────────────────────────────────┐
│ Check if Already Processed      │
│ (Deduplication check)           │
└────────────┬────────────────────┘
             │
         Already Processed? ──YES──► Continue polling
             │
            NO
             │
             ▼
┌─────────────────────────────────┐
│ Run ImportCli Service           │
│ (Start import in background)    │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│ Mark Path as Processed          │
│ (Add to HashSet)                │
└────────────┬────────────────────┘
             │
             ▼
┌─────────────────────────────────┐
│ Wait 60 seconds                 │
│ (Allow re-import if needed)     │
└────────────┬────────────────────┘
             │
             ▼
        Remove from HashSet
        Continue polling
```

### Example Output

```
Mount watcher started. Listening for camera mounts...
Mount detected: /Volumes/SD-CARD
Camera storage detected on /Volumes/SD-CARD
Starting import from /Volumes/SD-CARD/DCIM
Importing from /Volumes/SD-CARD/DCIM
Done Importing 42
Time: 12.5 sec. or 0.2 min.
Failed: 0
Skip due already imported: 0
Import completed for /Volumes/SD-CARD/DCIM: True
```

## Troubleshooting

### "No camera storage detected"
- Ensure the external drive/SD card has a `DCIM` folder
- Check folder name (case-insensitive, so `dcim`, `DCIM`, `Dcim` all work)
- Try ejecting and re-inserting the device

### Import not starting
- Check that `starskyimportercli` and its dependencies are available
- Verify ExifTool and Geo database can be downloaded
- Check logs for specific error messages

### Watcher crashes
- Check system logs for crashes
- Restart the service: `launchctl restart com.starsky.mountwatcher` (macOS)
- Or: `systemctl restart starsky-mountwatcher` (Linux)

### High CPU usage
- Mount watcher uses 2-second polling interval
- This is a trade-off between responsiveness and CPU usage
- Can be tuned in future versions

### Permissions denied
- macOS: Grant Full Disk Access to the executable
- Linux: Ensure user has read access to `/proc/mounts`
- Windows: Run as administrator if needed

## Monitoring

### View Logs

**macOS**:
```bash
# From launchd
tail -f /var/log/starsky-mountwatch.log

# Or from system log
log stream --level debug --predicate 'eventMessage contains[c] "starsky"'
```

**Linux**:
```bash
# From systemd
journalctl -u starsky-mountwatcher -f

# Or from log file
tail -f /var/log/starsky-mountwatch.log
```

**Windows**:
```powershell
Get-EventLog -LogName Application -Source "StarskyMountWatcher" -Newest 50
```

### Check Service Status

**macOS**:
```bash
launchctl list | grep mountwatcher
```

**Linux**:
```bash
systemctl status starsky-mountwatcher
```

**Windows**:
```powershell
Get-Service -Name "StarskyMountWatcher"
```

## Configuration Options

### Command Line Arguments

```bash
starskymountwatchercli --verbose          # Enable verbose logging
starskymountwatchercli --help             # Show help
```

### Environment Variables

Set via `AppSettings.json` or environment:

- `STARSKY_STRUCTURE`: Directory structure (date-based, camera-based, etc.)
- `STARSKY_IMPORT_*`: Various import settings

## Integration with ImportCli

The mount watcher delegates actual import work to `ImportCli`:

```csharp
var importCli = new ImportCli(
    importService, appSettings, console, logger,
    exifToolDownload, geoFileDownload, cameraStorageDetector);

// Runs with mount-detected paths
await importCli.Importer(new[] { cameraPath, "--recursive", "--verbose" });
```

This ensures:
- Consistent import behavior
- All existing import features work
- Proper logging and error handling

## Uninstalling

### macOS
```bash
launchctl unload ~/Library/LaunchAgents/com.starsky.mountwatcher.plist
rm ~/Library/LaunchAgents/com.starsky.mountwatcher.plist
```

### Linux
```bash
sudo systemctl stop starsky-mountwatcher
sudo systemctl disable starsky-mountwatcher
sudo rm /etc/systemd/system/starsky-mountwatcher.service
sudo systemctl daemon-reload
```

### Windows
```powershell
Stop-Service -Name "StarskyMountWatcher"
Remove-Service -Name "StarskyMountWatcher"
```

## Support Files

- **README**: `/starsky.foundation.mountwatch/README.md`
- **Implementation Details**: `/MOUNT_WATCHER_IMPLEMENTATION.md`
- **Checklist**: `/MOUNT_WATCHER_CHECKLIST.md`

---

**Ready to use!** 🎉


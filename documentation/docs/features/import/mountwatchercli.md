# Mount Watcher CLI to Auto Import

The **starskymountwatchercli** feature runs as a background watcher for mounted camera media.
When a camera SD card (or similar storage) is connected, it can automatically trigger import.

This is useful when you want an always-on ingest flow without manually starting each import.

## Overview

- Listens for mount events.
- Detects whether a mounted volume is likely camera storage.
- Starts import only for matching camera volumes.
- Supports installation as an OS service.

## How camera detection works

The mount watcher uses the same camera-storage heuristics as the importer:

1. Volume is ready and accessible.
2. Filesystem is camera-friendly (for example FAT or exFAT variants).
3. Camera folder structure is detected (for example `DCIM` or similar patterns).

Only when checks pass, import starts.

## CLI options

```bash
starskymountwatchercli --help
starskymountwatchercli --install
starskymountwatchercli --uninstall
starskymountwatchercli --status

```

Additional flags:

- `--help` or `-h` to print usage and platform-specific service notes.

## Platform support

- macOS: launchd LaunchAgent install/uninstall flow.
- Linux: systemd service install/uninstall flow (with user-level fallback).
- Windows: Windows Service install/uninstall flow via `sc.exe`.

## Typical workflow

1. Install and start the service.
2. Insert an SD card or connect camera storage.
3. Watcher receives mount event.
4. Camera checks pass.
5. Import starts automatically.

## Operational notes

- [macOS may require Full Disk Access for stable operation on external media.](troubleshooting-access-to-path-macos.md)
- Linux logs are visible with `journalctl` for the mount watcher service.
- Windows logs are available in Event Viewer.

## Delete after import

When importing from mounted camera storage (for example via the mount watcher), Starsky can optionally delete the original files after a successful import.

Set the option in `appsettings.json`:

```json
"ImportMountWatcher" : {
    "DeleteAfter": "false"
}
```

Or override with an environment variable:

```json
"app__ImportMountWatcher__DeleteAfter": "true"
```

When `DeleteAfter` is `true`, successfully imported files on the mounted device will be removed. Use this with caution — enable only when you want files removed from the source device after import.

## Related

- [Import options](import.md)
- [Cloud Import](cloudimport.md)
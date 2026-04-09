---
slug: new-feature-starskymountwatchercli
title: "New feature: starskymountwatchercli"
authors: dion
tags: [photo management, automation, cli]
date: 2026-04-09
image: https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/02_starsky_v052_kl1k.jpg
---

# New feature: starskymountwatchercli

We added a new CLI service called **starskymountwatchercli**. It watches for mounted storage devices (like SD cards), checks whether they look like camera media, and starts importing automatically.

This gives you a hands-free ingest flow: plug in card, import starts.

<!-- truncate -->

## What it does

- Watches mount events continuously.
- Detects camera storage with the same heuristics used by the importer.
- Starts an import only when the mounted volume is a likely camera card.
- Supports service mode per operating system.

## Why this helps

If you import frequently, this removes repeated manual steps:

1. Open the app.
2. Find the mounted path.
3. Start import.

With starskymountwatchercli running as a service, the process becomes event-driven.

## Camera storage detection

The watcher does not import every drive. It applies camera-oriented checks first:

- Mounted volume is ready and accessible.
- Filesystem is camera-friendly (for example FAT-based media).
- Camera-like folder structure exists (such as `DCIM` or other known patterns).

Only after these checks pass, the importer runs.

## Service support by platform

The CLI includes service install and uninstall flows:

- macOS: launchd LaunchAgent
- Linux: systemd (system-level, with user-level fallback)
- Windows: Windows Service via `sc.exe`

This means the watcher can run in the background without manual startup each time.

## CLI usage

```bash
starskymountwatchercli --help
starskymountwatchercli --install
starskymountwatchercli --uninstall
```

Extra options:

- `--verbose` or `-v` for verbose logging
- `--help` or `-h` for usage and platform-specific hints

## Typical workflow

1. Install and start the service.
2. Insert camera SD card or connect camera storage.
3. Watcher receives mount event.
4. Camera storage checks pass.
5. Import starts automatically.

## Notes for production use

- On macOS, Full Disk Access may be required for stable background operation.
- On Linux, service logs are available through `journalctl`.
- On Windows, service logs can be inspected in Event Viewer.

## Final thoughts

starskymountwatchercli is a small feature with a large practical impact: less clicking, fewer missed imports, and a cleaner ingest pipeline for high-volume photo workflows.

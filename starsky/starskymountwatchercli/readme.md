# starskymountwatchercli

## List of [Starsky](../../readme.md) Projects

* [By App documentation](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
        * [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md) _import command line
      interface_
    * __[starskyMountWatcherCli](../../starsky/starskymountwatchercli/readme.md) watch sd cards__
    * [starskyDemoSeedCli](../../starsky/starskydemoseedcli/readme.md) _demo seed data_
    * [starskyDependenciesDownloadCli](../../starsky/starskydependenciesdownloadcli/readme.md) _make
      sure external dependencies are installed_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md) _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a
      content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp
      service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  manage user accounts
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes
      are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by
      generating smaller images_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic
      libraries (.NET)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests (for .NET)_
* [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
* [Starsky Desktop](../../starskydesktop/readme.md) _Desktop Application_
    * [Download Desktop App](https://docs.qdraw.nl/download/) _Windows and macOS version_
* [Changelog](../../history.md) _Release notes and history_

## Mount Watcher CLI Options

`starskymountwatchercli` is a background CLI service that watches mount events and
starts import automatically when a mounted volume looks like camera storage.

## What it does

- Listens for mount events.
- Detects camera media using the shared camera detection logic.
- Runs import on detected camera volumes.
- Supports install/uninstall as an OS service.

## CLI options

```bash
starskymountwatchercli --help
starskymountwatchercli --install
starskymountwatchercli --uninstall
starskymountwatchercli --verbose
```

## Service support

- macOS: launchd (LaunchAgents)
- Linux: systemd (system-level, with user-level fallback)
- Windows: Windows Service (`sc.exe`)

## Typical flow

1. Install service with `--install`.
2. Insert SD card / connect camera storage.
3. Mount is detected.
4. Camera checks pass.
5. Import starts automatically.

## Notes

- macOS may require Full Disk Access for stable external drive access.
- Linux logs can be viewed with `journalctl` for the mount watcher service.
- Windows logs are available in Event Viewer.

# Demo seed cli

## List of [Starsky](../../readme.md) Projects

* [By App documentation](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
        * [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line
      interface_
    * [starskyMountWatcherCli](../../starsky/starskymountwatchercli/readme.md)  _watch sd cards_
    * __[starskyDemoSeedCli](../../starsky/starskydemoseedcli/readme.md) demo seed data__
    * [starskyDependenciesDownloadCli](../../starsky/starskydependenciesdownloadcli/readme.md)
      _make sure external dependencies are installed_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a
      content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp
      service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  manage user accounts
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes
      are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by
      generating smaller images_
    * [starskyThumbnailMetaCli](../../starsky/starskythumbnailmetacli/readme.md)  _create small thumbnails from the embedded preview_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic
      libraries (.NET)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests (for .NET)_
* [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
* [Starsky Desktop](../../readme-desktop.md) _Desktop Application_
    * [Download Desktop App](https://docs.qdraw.nl/download/) _Windows and macOS version_
* [Changelog](../../history.md) _Release notes and history_

## starskyDemoSeedCli docs

Downloads the demo photos into the storage folder (`StorageFolder`) and then runs a sync of the
whole storage folder, so the files are available in the database. Use this to set up a demo
environment. See `DemoData` and `DemoUnsafeDeleteStorageFolder` in the
[web application settings](../../starsky/starsky/readme.md#recommend-settings).

```sh
./starskydemoseedcli --help
```

The only options are `--help` and `--verbose`, plus the
[shortcut arguments](/docs/getting-started/configuration/config-options) that
overwrite settings, such as `--basepath` and `--connection`.

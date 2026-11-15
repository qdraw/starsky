# Thumbnail Meta Cli

## List of [Starsky](../../readme.md) Projects

* [By App documentation](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
        * [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line
      interface_
    * [starskyMountWatcherCli](../../starsky/starskymountwatchercli/readme.md)  _watch sd cards_
    * [starskyDemoSeedCli](../../starsky/starskydemoseedcli/readme.md)  _demo seed data_
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
    * __[starskyThumbnailMetaCli](../../starsky/starskythumbnailmetacli/readme.md)  create small thumbnails from the embedded preview__
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic
      libraries (.NET)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests (for .NET)_
* [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
* [Starsky Desktop](../../readme-desktop.md) _Desktop Application_
    * [Download Desktop App](https://docs.qdraw.nl/download/) _Windows and macOS version_
* [Changelog](../../history.md) _Release notes and history_

## starskyThumbnailMetaCli docs

Creates the small thumbnails (`MetaThumbnail`) from the preview that is embedded in the photo
(the Exif thumbnail), without decoding the full image. This is very fast and gives you something
to show in the web interface before the regular thumbnails are generated. Afterwards the
thumbnail status of the files is updated in the database. The same step runs automatically after
an import when `MetaThumbnailOnImport` is enabled.

Use one of `-p`, `-s` or `-g` to select what to process.

### To get help:

```sh
./starskythumbnailmetacli --help
```

### The StarskyThumbnailMetaCli --Help window:

```
--path or -p == parameter: (string) ; 'full path', only child items of the database folder are supported,search and replace first part of the filename, '/', use '-p' for current directory
--subpath or -s == parameter: (string) ; relative path in the database
--subpathrelative or -g == Overwrite sub-path to use relative days to select a folder, use for example '1' to select yesterday. (structure is required)
-p, -s, -g == select one of those before starting
recursive is enabled by default
--verbose or -v == verbose, more detailed info
  use -v -help to show settings:
```

The [shortcut arguments](/docs/getting-started/configuration/config-options#command-line-options)
that overwrite settings, such as `--basepath` and `--connection`, can be used as well.

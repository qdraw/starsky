# Starsky Synchronize Cli
## List of [Starsky](../../readme.md) Projects
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * __[starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  check if disk changes are updated in the database__
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _Desktop Application_
    * [Download Desktop App](https://qdraw.github.io/starsky/assets/download/download.html) _Windows and Mac OS version_
 * [Changelog](../../history.md) _Release notes and history_

## Synchronize Cli Options

### Starsky Sync Indexer:
With this command line tool it possible to manual sync the filesystem with the database, update one file in the database. The goal of this wrapper is to get command line access to the photo index database.

### Before you start

When you start this application at first please update the `appsettings.json`

```json
{
  "App": {
    "ThumbnailTempFolder": "Y:\\data\\photodirectory\\temp",
    "StorageFolder": "Y:\\data\\photodirectory\\storage",
    "DatabaseType": "mysql",
    "DatabaseConnection": "Server=mysqlserver.nl;database=dbname;uid=username;pwd=password;",
    "ExifToolPath": "C:\\exiftool.exe",
    "Structure": "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext",
    "ReadOnlyFolders": ["/2015","/2018"],
    "AddMemoryCache": "true"
  }
}
```

>    TIP: When using a boolean in the json add quotes. Booleans without quotes are ignored. So use `"true"` instead of `true`

>   TIP: For windows use double escape backslashes to avoid crashes

#### Appsettings Notes
1.  The `Structure`-setting is used by the `StarskyImporterCli` and the `/import` endpoint. This always uses slash as directory marker.
2.  The settings: `ExifToolPath`, `ThumbnailTempFolder` and  `StorageFolder` uses the system path.
    When using Windows please escape the backslash, otherwise the application will crash.
3.  The `AddMemoryCache` setting is ignored in the console/cli applications


### To the help dialog:
```sh
./starskysynccli --help
```

### The Sync --Help window:
```
Starksy Sync Cli ~ Help:
--help or -h == help (this window)
--path or -p == parameter: (string) ; 'full path', only child items of the database folder are supported,search and replace first part of the filename, '/', use '-p' for current directory
--subpath or -s == parameter: (string) ; relative path in the database
--subpathrelative or -g == Overwrite subpath to use relative days to select a folder, use for example '1' to select yesterday. (structure is required)
-p, -s, -g == you need to select one of those tags
--index or -i == parameter: (bool) ; enable indexing, default true
--verbose or -v == verbose, more detailed info
--databasetype or -d == Overwrite EnvironmentVariable for DatabaseType
--basepath or -b == Overwrite EnvironmentVariable for StorageFolder
--connection or -c == Overwrite EnvironmentVariable for DatabaseConnection
--thumbnailtempfolder or -f == Overwrite EnvironmentVariable for ThumbnailTempFolder
--exiftoolpath or -e == Overwrite EnvironmentVariable for ExifToolPath
--verbose or -v == verbose, more detailed info
  use -v -help to show settings:
```

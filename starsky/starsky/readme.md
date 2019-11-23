# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * __[starsky](../../starsky/starsky/readme.md) web api application / interface__
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskySyncCli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskycore](../../starsky/starskycore/readme.md) _business logic (netstandard 2.0)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starsky/starsky docs

### Structure configuration:

When setup Starksy there are two options to configure the installation.
There is a list of required settings. First the `appsettings.json` is loaded and the environment variables are overwriting features.
The command line arguments are shortcuts to set an in-app environment variable.

### The order of reading settings
You could use machine specific configuration files: appsettings.{machinename}.json _(and replace {machinename} with your computer name in lowercase)_
1.  You can use `appsettings.json` inside the application folder to set base settings. The order of this files is used to get the values from the appsettings
    -    `/bin/Debug/netcoreapp2.1/appsettings.patch.json`
    -    `/bin/Debug/netcoreapp2.1/appsettings.computername.patch.json`
    -    `/bin/Debug/netcoreapp2.1/appsettings.json`
    -    `/bin/Debug/netcoreapp2.1/appsettings.computername.json`

2.  Use Environment variables to overwrite those base settings
   For `ThumbnailTempFolder` use `app__ThumbnailTempFolder` ([source](https://github.com/aspnet/Configuration/commit/cafd2e53eb71a6d0cecc60a9e38ea1df2dafb916))  
3.  Command line argumements in the Cli applications to set in-app environment variables

### Required settings to start
1.  `ExifToolPath` - A path to Exiftool.exe
### Recommend settings
2.  `ThumbnailTempFolder` - For storing thumbnails (default: `./bin/Debug/netcoreapp2.0/thumbnailTempFolder`)
3.  `StorageFolder` - For the main photo directory (default: `./bin/Debug/netcoreapp2.0/storageFolder`)
4.  `DatabaseType` - `mysql`, `sqlite` or  `inmemorydatabase` are supported (default: `sqlite`)
5.  `DatabaseConnection` - The connection-string to the database (default: `./bin/Debug/netcoreapp2.0/data.db`)
6.  `CameraTimeZone` - The timezone of the Camera, for example `Europe/Amsterdam` (defaults to your local timezone)
### Optional settings
1.  `Structure` - The structure that will be used when you import files, has a default fallback.
2.  `ReadOnlyFolders` - Accepts a list of folders that never may be edited, defaults a empty list
3.  `AddMemoryCache` - Enable caching _(default true)_
4.  `AddHttp2Optimizations`  - Enable HTTP2 Optimizations _(default true)_
5.  `AddSwagger` - To show a user interface to show al REST-services _(default false)_
6.  `ExifToolImportXmpCreate` - is used to create at import time a xmp file based on the raw image _(default false)_
7.  `AddSwaggerExport` - _Temporary disabled due known issue_ To Export Swagger defentions on startup _(default false)_
8.  `AddLegacyOverwrite`- Read Only value for ("Mono.Runtime") _(default false)_
9.  `Verbose` - show more console logging  _(default false)_

### Appsettings.json example
```json
{
  "App": {
    "ThumbnailTempFolder": "Y:\\data\\photodirectory\\temp",
    "StorageFolder": "Y:\\data\\photodirectory\\storage",
    "DatabaseType": "mysql",
    "DatabaseConnection": "Server=mysqlserver.nl;database=dbname;uid=username;pwd=password;",
    "ExifToolPath": "C:\\exiftool.exe",
    "Structure": "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext",
    "ReadOnlyFolders": ["/2015","/2018"],
    "AddMemoryCache": "true",
    "CameraTimeZone": "America/New_York"
  }
}
```

> Note: When using a boolean in the json add quotes. Booleans without quotes are ignored

> Tip: When using the `mysql`-setting, make sure the database uses `utf8mb4` and as collate `utf8mb4_unicode_ci` to avoid encoding errors.

#### Appsettings Notes
1.  Structure uses slash as directory separators for Linux and Windows
2.  The settings: `ExifToolPath`, `ThumbnailTempFolder` and  `StorageFolder` uses the system path directory separators
3.  When using Windows please double escape (`\\`) system path's

### Warmup script
The default behavior of .NET is to load everything first. To be sure that the application is warm before someone arrives, please check `tools/starsky-warmup.sh`.


### Rest API documentation
Starsky has a Json and Razorview restful API. Please read the documentation

### Swagger
Swagger is an open-source software framework backed by a large ecosystem of tools that helps developers design, build, document, and consume RESTful Web services. There is an swagger definition. You could enable this by setting the following values:

By default this feature is disabled, please use the `AddSwagger` definition in the AppSettings or use the following environment variable:

```
app__AddSwagger=true
```

This is the default location of the swagger documentation
```
http://localhost:5000/swagger
```

#### Rest API Table of contents
- [Get PageType	"Archive" ](readme_api.md#get-pagetype-archive)
- [Get PageType	"DetailView"](readme_api.md#get-pagetype-detailview)
- [Exif Info](readme_api.md#exif-info)
- [Exif Update](readme_api.md#exif-update)
- [Rename](readme_api.md#rename)
- [File Delete](readme_api.md#file-delete)
- [Thumbnail](readme_api.md#thumbnail)
- [Thumbnail Json](readme_api.md#thumbnail-json)
- [Download Photo](readme_api.md#download-photo)
- [Direct import](readme_api.md#direct-import)
- [Form import](readme_api.md#form-import)
- [Import Exif Overwrites (shared feature)](readme_api.md#import-exif-overwrites-shared-feature)
- [Search](readme_api.md#search)
- [Remove cache](readme_api.md#remove-cache)
- [Environment info](readme_api.md#environment-info)

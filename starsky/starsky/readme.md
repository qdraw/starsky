# Starsky Web API application
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
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _Desktop Application (Pre-alpha code)_

## starsky/starsky docs

### Structure configuration:

When setup Starksy there are two options to configure the installation.
There is a list of required settings. First the `appsettings.json` is loaded and the environment variables are overwriting features.
The command line arguments are shortcuts to set an in-app environment variable.

### The order of reading settings
You could use machine specific configuration files: appsettings.{machinename}.json _(and replace {machinename} with your computer name in lowercase)_
1.  You can use `appsettings.json` inside the application folder to set base settings. The order of this files is used to get the values from the appsettings
    -    `/bin/Debug/netcoreapp3.1/appsettings.patch.json`
    -    `/bin/Debug/netcoreapp3.1/appsettings.computername.patch.json`
    -    `/bin/Debug/netcoreapp3.1/appsettings.json`
    -    `/bin/Debug/netcoreapp3.1/appsettings.computername.json`

2.  Use Environment variables to overwrite those base settings
   For `ThumbnailTempFolder` use `app__ThumbnailTempFolder` ([source](https://github.com/aspnet/Configuration/commit/cafd2e53eb71a6d0cecc60a9e38ea1df2dafb916))  
3.  Command line argumements in the Cli applications to set in-app environment variables

### Required settings to start
1. There are __no__ settings required
### Recommend settings
1.  `ThumbnailTempFolder` - For storing thumbnails (default: `./bin/Debug/netcoreapp3.1/thumbnailTempFolder`)
2.  `StorageFolder` - For the main photo directory (default: `./bin/Debug/netcoreapp3.1/storageFolder`)
3.  `DatabaseType` - `mysql`, `sqlite` or  `inmemorydatabase` are supported (default: `sqlite`)
4.  `DatabaseConnection` - The connection-string to the database (default: `./bin/Debug/netcoreapp3.1/data.db`)
5.  `CameraTimeZone` - The timezone of the Camera, for example `Europe/Amsterdam` (defaults to your local timezone)
### Optional settings
1.  `Structure` - The structure that will be used when you import files, has a default fallback.
2.  `ReadOnlyFolders` - Accepts a list of folders that never may be edited, defaults a empty list
3.  `AddMemoryCache` - Enable caching _(default true)_
4.  `IsAccountRegisterOpen`  - Keep registrations always open. The only 2 build-in exceptions are when there are no accounts or you already logged in _(default false)_
5.  `AddSwagger` - To show a user interface to show al REST-services _(default false)_
6.  `ExifToolImportXmpCreate` - is used to create at import time a xmp file based on the raw image _(default false)_
7.  `AddSwaggerExport` - _Temporary disabled due known issue_ To Export Swagger defentions on startup _(default false)_
8.  `AddLegacyOverwrite`- Read Only value for ("Mono.Runtime") _(default false)_
9.  `Verbose` - show more console logging  _(default false)_
10. `WebFtp` - used by starskyWebFtpCli
11. `PublishProfiles` - used by starskyWebHtmlCli
12.  `ExifToolPath` - A path to Exiftool.exe _to ignore the included ExifTool_

### Appsettings.json example
```json
{
  "App": {
    "ThumbnailTempFolder": "Y:\\data\\photodirectory\\temp",
    "StorageFolder": "Y:\\data\\photodirectory\\storage",
    "DatabaseType": "mysql",
    "DatabaseConnection": "Server=mysqlserver.nl;database=dbname;uid=username;pwd=password;",
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

### Search Docs
Advanced queries are supported by the basic search engine.

__All text (not number or date) driven search queries use a contain search__


| Search options       | example                            | description                   |
|----------------------|------------------------------------|-------------------------------|
| __-tags__            | test                               | default option                |
| -tags                | -tags="testtag"                    |                               |
| -tags                | -test apple                        | Ignore the keyword test       |
| -tags                | -Tags-"test"                       | Ignore the keyword test       |
| -tags                | apple banana                       | search for apple or banana    |
| __-title__           | -title="example"                   |                               |
| __-filepath__        | -filepath="/path"                  | -inurl is the same            |
| __-filename__        | -filename="file.jpg"               |                               |
| __-filehash__        | -filehash=3DB75N7JJ6FDOPZY4YHGX4TL |                               |
| __-parentdirectory__ | -parentdirectory="/2019"           |                               |
| __-description__     | -description="search"              |                               |
| __-imageformat__     | -ImageFormat="jpg"                 | include `jpeg`                |
| -imageformat         | -ImageFormat="tiff"                | including `dng`               |
| -imageformat         | -ImageFormat="bmp"                 |                               |
| -imageformat         | -ImageFormat="gif"                 |                               |
| -imageformat         | -ImageFormat="png"                 |                               |
| -imageformat         | -ImageFormat="xmp"                 |                               |
| -imageformat         | -ImageFormat="gpx"                 |                               |
| __-datetime__        | -datetime=1                        | search for yesterday          |
| -datetime            | -datetime>12 -datetime<2           | between 2 and 12 days ago     |
| -datetime            | -datetime=2020-01-01               | between 00:00:00 and 23:59:59 |
| -datetime            | -datetime=2020-01-01T14:35:29      | on this exact time            |
| __-addtodatabase__   | -addtodatabase=1                   | search for yesterday          |
| -addtodatabase       | -addtodatabase>12 -addtodatabase<2 | between 2 and 12 days ago     |
| -addtodatabase       | -addtodatabase=2020-01-01          | between 00:00:00 and 23:59:59 |
| -addtodatabase       | -addtodatabase=2020-01-01T14:35:29 | on this exact time            |
| __-lastedited__      | -lastedited=1                      | search for yesterday          |
| -lastedited          | -lastedited>12 -lastedited<2       | between 2 and 12 days ago     |
| -lastedited          | -lastedited=2020-01-01             | between 00:00:00 and 23:59:59 |
| -lastedited          | -lastedited=2020-01-01T14:35:29    | on this exact time            |
| __-isdirectory__     | -isdirectory=true                  | search for folders            |
| -isdirectory         | -isdirectory=false                 | search for items              |
| __-make__            | -make=Apple                        | brand name of the camera      |
| __-model__           | -model="iPhone SE"                 | search for camera model       |

### Rest API documentation
Starsky has a Json restful API. Please read the documentation

> Tip: Breaking changes are documentated in `./history.md`

### Swagger / OpenAPI
Swagger is an open-source software framework backed by a large ecosystem of tools that helps developers design, build, document, and consume RESTful Web services. There is an swagger definition. You could enable this by setting the following values:

By default this feature is disabled, please use the `AddSwagger` definition in the AppSettings or use the following environment variable:

```
app__AddSwagger=true
```

This is the default location of the swagger documentation
```
http://localhost:5000/swagger
```

### Known 'There are critical errors in the following components:'
When the UI starts there is an Health API check to make sure that some important components works good

#### Disk Space errors
- __Storage_StorageFolder__ There is not enough disk space available on the storage folder location 
- __Storage_ThumbnailTempFolder__ There is not enough disk space available on the thumbnails folder location 
- __Storage_TempFolder__ There is not enough disk space available on the temp folder location 

#### Not exist errors
- __Exist_StorageFolder__ The Storage Folder does not exist, please create it first.
- __Exist_TempFolder__ The Temp Folder does not exist, please create it first.
- __Exist_ExifToolPath__ ExifTool is not linked, you need this to write meta data to files.ExifTool.
    Try to remove the _temp folder_ and run the Application again.
- __Exist_ThumbnailTempFolder__ The Thumbnail cache Folder does not exist, please create it first.

#### DbContext, Mysql or Sqlite
There is also a check to make sure the database runs good

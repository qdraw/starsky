# Starsky Web API application
## List of [Starsky](../../readme.md) Projects
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * __[starsky](../../starsky/starsky/readme.md) web api application / interface__
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _Desktop Application_
    * [Download Desktop App](https://qdraw.github.io/starsky/assets/download/download.html) _Windows and Mac OS version_
 * [Changelog](../../history.md) _Release notes and history_

## starsky/starsky docs

### Structure configuration:

When setup Starksy there are two options to configure the installation.
There is a list of required settings. First the `appsettings.json` is loaded and the environment variables are overwriting features.
The command line arguments are shortcuts to set an in-app environment variable.

### The order of reading settings
You could use machine specific configuration files: appsettings.{machinename}.json _(and replace {machinename} with your computer name in lowercase)_
1.  You can use `appsettings.json` inside the application folder to set base settings.
    The order of this files is used to get the values from the appsettings
    -    `/bin/Debug/netcoreapp3.1/appsettings.patch.json`
    -    `/bin/Debug/netcoreapp3.1/appsettings.computername.patch.json`
    -    `/bin/Debug/netcoreapp3.1/appsettings.json`
    -    `/bin/Debug/netcoreapp3.1/appsettings.computername.json`

2.  Use Environment variables to overwrite those base settings
   For `ThumbnailTempFolder` use `app__ThumbnailTempFolder`
   ([source](https://github.com/aspnet/Configuration/commit/cafd2e53eb71a6d0cecc60a9e38ea1df2dafb916))  
3.  Command line argumements in the Cli applications to set in-app environment variables

### Required settings to start
1. To start it is __not__ mandatory to adjust any settings.

### Recommend settings
1.  `ThumbnailTempFolder` - For storing thumbnails (default: `./bin/Debug/netcoreapp3.1/thumbnailTempFolder`)
2.  `StorageFolder` - For the main photo directory (default: `./bin/Debug/netcoreapp3.1/storageFolder`)
3.  `DatabaseType` - `mysql`, `sqlite` or  `inmemorydatabase` are supported (default: `sqlite`)
4.  `DatabaseConnection` - The connection-string to the database (default: `./bin/Debug/netcoreapp3.1/data.db`)
5.  `CameraTimeZone` - The timezone of the Camera, for example `Europe/Amsterdam` (defaults to your local timezone)

### Optional settings
1. `Structure` - The structure that will be used when you import files, has a default fallback.
2. `ReadOnlyFolders` - Accepts a list of folders that never may be edited, defaults a empty list
3. `AddMemoryCache` - Enable caching _(default true)_
     The only 2 build-in exceptions are when there are no accounts or you already logged in _(default false)_
4. `AddSwagger` - To show a user interface to show al REST-services _(default false)_
5. `ExifToolImportXmpCreate` - is used to create at import time a xmp file based on the raw image _(default false)_
6. `AddSwaggerExport` - To Export Swagger definitions on startup _(default false)_
7. `AddLegacyOverwrite`- Read Only value for ("Mono.Runtime") _(default false)_
8. `Verbose` - show more console logging  _(default false)_
9. `WebFtp` - ftp path, this is used by starskyWebFtpCli
10. `PublishProfiles` - settings to configure publish output, used by starskyWebHtmlCli and publish button
11. `ExifToolPath` - A path to Exiftool.exe _to ignore the included ExifTool_
12. `isAccountRegisterOpen` - Allow everyone to register an account _(default false)_
13. `AccountRegisterDefaultRole` When a user is new and register an account, give it the role User or Administrator _(default User)_
14. `applicationInsightsInstrumentationKey` - Track telementry with Microsoft Application Insights _(default disabled)_
15. `useHttpsRedirection` - Redirect users to https page. You should enable before going to production.
     This toggle is always disabled in debug/develop mode _(default false)_
16. `Name` Name of the application, does not have much effect _(default Starsky)_
17. `AppSettingsPath` To store the settings by user in the AppData folder _(default empty string)_
18. `UseRealtime` Update the user interface realtime _default true_
19. `UseDiskWatcher` Watch the disk for changes and update the database _default false (but will change)_
20. `CheckForUpdates` Check if there are updates on github and notify the user _default true_
21. `SyncIgnore` Ignore pattern to not include disk items while running sync, uses always unix style and startsWith _default list with: /lost+found_
22. `ImportIgnore` ImportIgnore filter  _default list with: lost+found_

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
    "CameraTimeZone": "America/New_York",
    "ImportIgnore": ["lost+found"]
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
The default behavior of .NET is to load everything first.
To be sure that the application is warm before someone arrives, please check `tools/starsky-warmup.sh`.

### Search Docs
Advanced queries are supported by the basic search engine.

__All text (not number or date) driven search queries use a contain search__

#### Search operators documentation

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
| __-colorclass__      | -colorclass=1                      | search for colorClass         |
| -colorclass          | -colorclass=0                      | No Color / None               |
| -colorclass          | -colorclass=1                      | Purple / Winner               |
| -colorclass          | -colorclass=2                      | Red / WinnerAlt               |
| -colorclass          | -colorclass=3                      | Orange / Superior             |
| -colorclass          | -colorclass=4                      | Yellow / SuperiorAlt          |
| -colorclass          | -colorclass=5                      | Green / Typical               |
| -colorclass          | -colorclass=6                      | Azure / TypicalAlt            |
| -colorclass          | -colorclass=7                      | Blue / Extras                 |
| -colorclass          | -colorclass=8                      | Grey / No name                |
| __software__         | -software:"photoshop"              | Last edited this app          |

### Rest API documentation
Starsky has a Json restful API. Please read the documentation

> Tip: Breaking changes are documented in `./history.md`


| Path                               | Type  | Description                                                                |
|------------------------------------|-------|----------------------------------------------------------------------------|
| /api/account/status                | GET   | Check the account status of the login                                      |
| /account/login                     | GET   | Login form page (HTML)                                                     |
| /account/login                     | HEAD  | Login form page (HTML)                                                     |
| /api/account/login                 | POST  | Login the current HttpContext in                                           |
| /api/account/logout                | POST  | Logout the current HttpContext and redirect to login                       |
| /account/logout                    | GET   | Logout the current HttpContext and redirect to login                       |
| /api/account/change-secret         | POST  | Update password for current user                                           |
| /api/account/register              | POST  | Create a new user (you need a AF-token first)                              |
| /api/account/register/status       | GET   | Is the register form open                                                  |
| /api/account/permissions           | GET   | List of current permissions                                                |
| /api/allowed-types/mimetype/sync   | GET   | A (string) list of allowed MIME-types ExtensionSyncSupportedList           |
| /api/allowed-types/mimetype/thumb  | GET   | A (string) list of allowed ExtensionThumbSupportedList MimeTypes           |
| /api/allowed-types/thumb           | GET   | Check if IsExtensionThumbnailSupported                                     |
| /api/env                           | HEAD  | Show the runtime settings (dont allow AllowAnonymous)                      |
| /api/env                           | GET   | Show the runtime settings (dont allow AllowAnonymous)                      |
| /api/env                           | POST  | Show the runtime settings (dont allow AllowAnonymous)                      |
| /api/cache/list                    | GET   | Get Database Cache (only the cache)                                        |
| /api/remove-cache                  | GET   | Delete Database Cache (only the cache)                                     |
| /api/remove-cache                  | POST  | Delete Database Cache (only the cache)                                     |
| /api/delete                        | DELETE| Remove files from the disk, but the file must contain the !delete! tag     |
| /api/download-sidecar              | GET   | Download sidecar file for example image.xmp                                |
| /api/download-photo                | GET   | Select manually the original or thumbnail                                  |
| /error                             | GET   | Return Error page                                                          |
| /api/export/create-zip             | POST  | Export source files to an zip archive                                      |
| /api/export/zip/{f}.zip            | GET   | Get the exported zip, but first call 'createZip'use for example this url...|
| /api/geo/status                    | GET   | Get Geo sync status                                                        |
| /api/geo/sync                      | POST  | Reverse lookup for Geo Information and/or add Geo location based on a GP...|
| /api/health                        | GET   | Check if the service has any known errors and return only a stringPublic...|
| /api/health/details                | GET   | Check if the service has any known errorsFor Authorized Users only         |
| /api/health/application-insights   | GET   | Add Application Insights script to user context                            |
| /api/health/version                | POST  | Check if Client/App version has a match with the API-versionthe paramete...|
| /api/health/check-for-updates      | GET   | Check if Client/App version has a match with the API-version               |
| /search                            | POST  | Redirect to search GET page (HTML)                                         |
| /search                            | GET   | Search GET page (HTML)                                                     |
| /trash                             | GET   | Trash page (HTML)                                                          |
| /import                            | GET   | Import page (HTML)                                                         |
| /preferences                       | GET   | Preferences page (HTML)                                                    |
| /account/register                  | GET   | View the Register form (HTML)                                              |
| /api/import                        | POST  | Import a file using the structure format                                   |
| /api/import/thumbnail              | POST  | Upload thumbnail to ThumbnailTempFolderMake sure that the filename is co...|
| /api/import/fromUrl                | POST  | Import file from web-url (only whitelisted domains) and import this file...|
| /api/import/history                | GET   | Today's imported files                                                     |
| /api/index                         | GET   | The database-view of a directory                                           |
| /api/info                          | GET   | Get realtime (cached a few minutes) about the file                         |
| /api/update                        | POST  | Update Exif and Rotation API                                               |
| /api/replace                       | POST  | Search and Replace text in meta information                                |
| /api/publish                       | GET   | Get all publish profilesTo see the entire config check appSettings         |
| /api/publish/create                | POST  | Publish                                                                    |
| /api/publish/exist                 | GET   | To give the user UI feedback when submitting the itemNameTrue is not to ...|
| /redirect/sub-path-relative        | GET   | Redirect or view path to relative paths using the structure-config (see ...|
| /api/search                        | GET   | Gets the list of search results (cached)                                   |
| /api/search/relative-objects       | GET   | Get relative paths in a search queryDoes not cover multiple pages (so it...|
| /api/search/trash                  | GET   | List of files with the tag: !delete!Caching is disabled on this api call   |
| /api/search/remove-cache           | POST  | Clear search cache to show the correct results                             |
| /api/suggest                       | GET   | Gets the list of search results (cached)                                   |
| /api/suggest/all                   | GET   | Show all items in the search suggest cache                                 |
| /api/suggest/inflate               | GET   | To fill the cache with the data (only if cache is not already filled)      |
| /api/sync/mkdir                    | POST  | Make a directory (-p)                                                      |
| /api/sync                          | POST  | Do a file sync in a background process (replace with /api/synchronize)     |
| /api/sync/rename                   | POST  | Rename file/folder and update it in the database                           |
| /api/synchronize                   | POST  | Faster API to Check if directory is changed (not recursive)                |
| /api/synchronize                   | GET   | Faster API to Check if directory is changed (not recursive)                |
| /api/thumbnail/small/{f}           | GET   | Get thumbnail for index pages (300 px or 150px or 1000px (based on whats...|
| /api/thumbnail/list-sizes/{f}      | GET   | Get overview of what exists by name                                        |
| /api/thumbnail/{f}                 | GET   | Get thumbnail with fallback to original source image.Return source image...|
| /api/thumbnail/zoom/{f}@{z}        | GET   | Get zoomed in image by fileHash.At the moment this is the source image     |
| /api/thumbnail-generation          | POST  | Create thumbnails for a folder in the background                           |
| /api/upload                        | POST  | Upload to specific folder (does not check if already has been imported)U...|
| /api/upload-sidecar                | POST  | Upload sidecar file to specific folder (does not check if already has be...|

### Swagger / OpenAPI
Swagger is an open-source software framework backed by a large ecosystem
of tools that helps developers design, build, document, and consume RESTful Web services.
There is an swagger definition. You could enable this by setting the following values:

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

#### Folder or file not exist errors
- __Exist_StorageFolder__ The Storage Folder does not exist, please create it first.
- __Exist_TempFolder__ The Temp Folder does not exist, please create it first.
- __Exist_ExifToolPath__ ExifTool is not linked, you need this to write meta data to files.ExifTool.
    Try to remove the _temp folder_ and run the Application again.
- __Exist_ThumbnailTempFolder__ The Thumbnail cache Folder does not exist, please create it first.

#### Date issues
- __DateAssemblyHealthCheck__  this setting checks if your current datetime is newer than when this application is build

#### ApplicationDbContext, Mysql or Sqlite
There is also a check to make sure the database runs good

#### Application Insights
Health issues are also reported to Microsoft Application Insights This only is when a valid key is configured.

### Known issues

#### DiskWatcher in combination with child folders that have no access
When using `useDiskwatcher: true` and there are child folders that are not allowed to read
For example the `lost+found` folder
```
drwx------ 2 root root  16K Apr 16  2018 lost+found
```
Then DiskWatcher is stopping and retry 20 times before the state will be disabled
```
[DiskWatcher] (catch-ed) Access to the path '/mnt/external_disk/lost+found' is denied
```

Solution: make sure that all child folder are accessible
# Web API application
## List of [Starsky](../../readme.md) Projects
 * [By App documentation](../../starsky/readme.md) _database photo index & import index project_
    * __[starsky](../../starsky/starsky/readme.md) web api application / interface__
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (.NET)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests (for .NET)_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [Starsky Desktop](../../starskydesktop/readme.md) _Desktop Application_
    * [Download Desktop App](https://docs.qdraw.nl/download/) _Windows and Mac OS version_
 * [Changelog](../../history.md) _Release notes and history_

## starsky/starsky docs

### Structure configuration:

When setup Starksy there are two options to configure the installation.
There is a list of required settings. First the `appsettings.json` is loaded and the environment variables are overwriting features.
The command line arguments are shortcuts to set an in-app environment variable.

### The order of reading settings
You could use machine specific configuration files: appsettings.machinename.json _(and replace machinename with your computer name in lowercase)_
1.  You can use `appsettings.json` inside the application folder to set base settings.
    The order of this files is used to get the values from the appsettings
    -    `/bin/Debug/net6.0/appsettings.patch.json`
    -    `/bin/Debug/net6.0/appsettings.default.json`
    -    `/bin/Debug/net6.0/appsettings.computername.patch.json`
    -    `/bin/Debug/net6.0/appsettings.json`
    -    `/bin/Debug/net6.0/appsettings.computername.json`

2.  Use Environment variables to overwrite those base settings
   For `ThumbnailTempFolder` use `app__ThumbnailTempFolder`
   ([source](https://github.com/aspnet/Configuration/commit/cafd2e53eb71a6d0cecc60a9e38ea1df2dafb916))  
    Dictionaries can be used this way: `app__accountRolesByEmailRegisterOverwrite__test@mail.be`
3.  Command line arguments in the Cli applications to set in-app environment variables

### Required settings to start
1. To start it is __not__ mandatory to adjust any settings.

### Recommend settings
1.  `ThumbnailTempFolder` - For storing thumbnails (default: `./bin/Debug/net6.0/thumbnailTempFolder`)
2.  `StorageFolder` - For the main photo directory (default: `./bin/Debug/net6.0/storageFolder`)
3.  `DatabaseType` - `mysql`, `sqlite` or  `inmemorydatabase` are supported (default: `sqlite`)
4.  `DatabaseConnection` - The connection-string to the database (default: `./bin/Debug/net6.0/data.db`)
5.  `CameraTimeZone` - The timezone of the Camera, for example `Europe/Amsterdam` (defaults to your local timezone)

### Optional settings
1. `Structure` - The structure that will be used when you import files, _has a default fallback_.
2. `DependenciesFolder` - where store the data of external dependencies used _default folder in project_
3. `ReadOnlyFolders` - Accepts a list of folders that never may be edited, _defaults a empty list_
4. `AddMemoryCache` - Enable caching _(default true)_
     The only 2 build-in exceptions are when there are no accounts or you already logged in _(default false)_
5. `AddSwagger` - To show a user interface to show al REST-services _(default false)_
6. `ExifToolImportXmpCreate` - is used to create at import time a xmp file based on the raw image _(default false)_
7. `AddSwaggerExport` - To Export Swagger definitions on startup _(default false)_
8. `AddLegacyOverwrite`- Read Only value for ("Mono.Runtime") _(default false)_
9. `Verbose` - show more console logging  _(default false)_
10. `WebFtp` - ftp path, this is used by starskyWebFtpCli
11. `PublishProfiles` - settings to configure publish output, used by starskyWebHtmlCli and publish button
12. `ExifToolPath` - A path to Exiftool.exe _to ignore the included ExifTool_
13. `isAccountRegisterOpen` - Allow everyone to register an account _(default false)_
14. `AccountRegisterDefaultRole` When a user is new and register an account, give it the role User or Administrator _(default User)_
15. `ApplicationInsightsConnectionString` - Track Telemetry with Microsoft Application Insights (use connection string instead of Instrumentation key) _(default disabled)_
16. `ApplicationInsightsDatabaseTracking` - Track database dependencies (need to have InstrumentationKey) _(default disabled)_
17. `ApplicationInsightsLog` - Add WebLogger output to Application Insights (need to have InstrumentationKey) _(default enabled, when key is provided)_
18. `useHttpsRedirection` - Redirect users to https page. You should enable before going to production.
     This toggle is always disabled in debug/develop mode _(default false)_
19. `httpsOn` Set all cookies in https Mode. You should enable before going to production. _(default false)_
20. `Name` Name of the application, does not have much effect _(default Starsky)_
21. `AppSettingsPath` To store the settings by user in the AppData folder _(default empty string)_
22. `UseRealtime` Update the user interface realtime _default true_
23. `UseDiskWatcher` Watch the disk for changes and update the database _default true_
24. `CheckForUpdates` Check if there are updates on github and notify the user _default true_
25. `SyncIgnore` Ignore pattern to not include disk items while running sync, uses always unix style and startsWith _default list with: /lost+found_
26. `ImportIgnore` ImportIgnore filter  _default list with: "lost+found" ".Trashes"_
27. `MaxDegreesOfParallelism` Number of jobs running in background _default 6_
28. `MetaThumbnailOnImport` Create small thumbnails after import, is very fast _default true_
29. `EnablePackageTelemetry` Telemetry is send for service improvement _default true_
30. `EnablePackageTelemetryDebug` Debug Telemetry _default false_
31. `AddSwaggerExportExitAfter` Quit application after exporting swagger files, should have `AddSwagger` and `AddSwaggerExport` enabled _default false_
32. `NoAccountLocalhost` No login needed when on localhost, used in Desktop App
33. `VideoUseLocalTime` Use localtime by Camera make and model instead of UTC
34. `SyncOnStartup` Sync Database on changes since latest start _default true_
35. `ThumbnailGenerationIntervalInMinutes` Interval to generate thumbnails, to disable use value lower than 3 _default 15_ 
36. `GeoFilesSkipDownloadOnStartup` Skip download of GeoFiles on startup, _recommend to keep this false or null_ - _default false_
37. `ExiftoolSkipDownloadOnStartup` Skip download of Exiftool on startup, _recommend to keep this false or null_ - _default false_
38. `AccountRolesByEmailRegisterOverwrite` Overwrite the default role for a user by email address, _default empty list_

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

| Search options       | example                              | description                   |
|----------------------|--------------------------------------|-------------------------------|
| __-tags__            | test                                 | default option                |
| -tags                | -tags="testtag"                      |                               |
| -tags                | -test apple                          | Ignore the keyword test       |
| -tags                | -Tags-"test"                         | Ignore the keyword test       |
| -tags                | apple banana                         | search for apple or banana    |
| __-title__           | -title="example"                     |                               |
| __-filepath__        | -filepath="/path"                    | -inurl is the same            |
| __-filename__        | -filename="file.jpg"                 |                               |
| __-filehash__        | -filehash=3DB75N7JJ6FDOPZY4YHGX4TL   |                               |
| __-parentdirectory__ | -parentdirectory="/2019"             |                               |
| __-description__     | -description="search"                |                               |
| __-imageformat__     | -ImageFormat="jpg"                   | include `jpeg`                |
| -imageformat         | -ImageFormat="tiff"                  | including `dng`               |
| -imageformat         | -ImageFormat="bmp"                   |                               |
| -imageformat         | -ImageFormat="gif"                   |                               |
| -imageformat         | -ImageFormat="png"                   |                               |
| -imageformat         | -ImageFormat="xmp"                   |                               |
| -imageformat         | -ImageFormat="gpx"                   |                               |
| __-datetime__        | -datetime=1                          | search for yesterday          |
| -datetime            | -datetime\>12 -datetime\<2           | between 2 and 12 days ago     |
| -datetime            | -datetime=2020-01-01                 | between 00:00:00 and 23:59:59 |
| -datetime            | -datetime=2020-01-01T14:35:29        | on this exact time            |
| __-addtodatabase__   | -addtodatabase=1                     | search for yesterday          |
| -addtodatabase       | -addtodatabase\>12 -addtodatabase\<2 | between 2 and 12 days ago     |
| -addtodatabase       | -addtodatabase=2020-01-01            | between 00:00:00 and 23:59:59 |
| -addtodatabase       | -addtodatabase=2020-01-01T14:35:29   | on this exact time            |
| __-lastedited__      | -lastedited=1                        | search for yesterday          |
| -lastedited          | -lastedited\>12 -lastedited\<2       | between 2 and 12 days ago     |
| -lastedited          | -lastedited=2020-01-01               | between 00:00:00 and 23:59:59 |
| -lastedited          | -lastedited=2020-01-01T14:35:29      | on this exact time            |
| __-isdirectory__     | -isdirectory=true                    | search for folders            |
| -isdirectory         | -isdirectory=false                   | search for items              |
| __-make__            | -make=Apple                          | brand name of the camera      |
| __-model__           | -model="iPhone SE"                   | search for camera model       |
| __-colorclass__      | -colorclass=1                        | search for colorClass         |
| -colorclass          | -colorclass=0                        | No Color / None               |
| -colorclass          | -colorclass=1                        | Purple / Winner               |
| -colorclass          | -colorclass=2                        | Red / WinnerAlt               |
| -colorclass          | -colorclass=3                        | Orange / Superior             |
| -colorclass          | -colorclass=4                        | Yellow / SuperiorAlt          |
| -colorclass          | -colorclass=5                        | Green / Typical               |
| -colorclass          | -colorclass=6                        | Azure / TypicalAlt            |
| -colorclass          | -colorclass=7                        | Blue / Extras                 |
| -colorclass          | -colorclass=8                        | Grey / No name                |
| __software__         | -software:"photoshop"                | Last edited this app          |

### Rest API documentation
Starsky has a Json restful API. There is a Swagger documentation available at `/swagger/index.html` 
and in the documentation there is a API chapter

> Tip: Breaking changes are documented in `./history.md`

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
http://localhost:4000/swagger
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

#### DiskWatcher in combination with Mac OS APFS Disk

When you set `/System/Volumes/Data` to watch for changes this makes the application crash with
`System.ArgumentOutOfRangeException` when a single file is changed. There is currently no solution for this problem other then don't use the Diskwatcher with this location.

```c#
Unhandled exception. System.ArgumentOutOfRangeException: Specified argument was out of the range of valid values.
   at System.IO.FileSystemWatcher.RunningInstance.ProcessEvents(Int32 numEvents, Byte** eventPaths, FSEventStreamEventFlags* eventFlags, UInt64* eventIds, FileSystemWatcher watcher)
   at System.IO.FileSystemWatcher.RunningInstance.<>c__DisplayClass14_0.<FileSystemEventCallback>b__0(Object o)
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location where exception was thrown ---
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
   at System.IO.FileSystemWatcher.RunningInstance.FileSystemEventCallback(IntPtr streamRef, IntPtr clientCallBackInfo, IntPtr numEvents, Byte** eventPaths, FSEventStreamEventFlags* eventFlags, UInt64* eventIds)
...
```

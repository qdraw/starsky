# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux [(files)](../../inotify-settings/)_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project [(files)](../../starsky/)_
    * __[starsky](../../starsky/starsky/readme.md)  _mvc application / web interface [(files)](../../starsky/starsky/)___
    * [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface [(files)](../../starsky/starskysynccli/)_
    * [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface [(files)](../../starsky/starskyimportercli/)
    * [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests [(files)](../../starsky/starskyTests)_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to html files [(files)](../../starsky/starskyTests)_
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks [(files)](../../starsky-node-client)_
 * [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

## starsky/starsky docs

### Structure configuration:

When setup Starksy there are two options to configure the installation.
There is a list of required settings. First the `appsettings.json` is loaded and the
environment variables are overwriting features.
The commandline arguments are shortcuts to set an in-app environment variable

### The order of reading settings
1.  You can use `appsettings.json` inside the application folder to set base settings
2.  Use Environment variables to overwrite those base settings
   For `ThumbnailTempFolder` use `app__ThumbnailTempFolder` ([source](https://github.com/aspnet/Configuration/commit/cafd2e53eb71a6d0cecc60a9e38ea1df2dafb916))  
3.  Command line argumements in the Cli applications to set in-app environment variables

### Required settings to start
1.  `ExifToolPath` - A path to Exiftool.exe
### Recommend settings
2.  `ThumbnailTempFolder` - For storing thumbnails (default: `./bin/Debug/netcoreapp2.0/thumbnailTempFolder`)
3.  `StorageFolder` - For the main photo directory (default: `./bin/Debug/netcoreapp2.0/storageFolder`)
4.  `DatabaseType` - `mysql`, `sqlite` or  `inmemorydatabase` are supported (default: `sqlite`)
5.  `DatabaseConnection` - The connectionstring to the database (default: `./bin/Debug/netcoreapp2.0/data.db`)
### Optional settings
1.  `Structure` - The structure that will be used when you import files, has a default fallback.
2.  `ReadOnlyFolders` - Accepts a list of folders that never may be edited, defaults a emphy list
3.  `AddMemoryCache`- Enable caching

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
    "AddMemoryCache": "true"
  }
}
```
> When using a boolean in the json add quotes. Booleans without quotes are ignored

#### Appsettings Notes
1.  Structure uses slash as directory separators for Linux and Windows
2.  The settings: `ExifToolPath`, `ThumbnailTempFolder` and  `StorageFolder` uses the system path directory separators
3.  When using Windows please double escape (`\\`) system path's


### Rest API documentation
Starsky has a Json and Razorview restfull API. Please read the documentation

#### Rest API Table of contents
- [Get PageType	"Archive" ](readme_api.md#get-pagetypearchive)
- [Get PageType	"DetailView"](readme_api.md#get-pagetypedetailview)
- [Exif Info](readme_api.md#exif-info)
- [Exif Update](readme_api.md#exif-update)
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

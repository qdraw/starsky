# Starsky
## List of Starksy Projects
 - [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux [(files)](../../inotify-settings/)_
 - [starsky (sln)](../../starsky/readme.md) _database photo index & import index project [(files)](../../starsky/)_
   - __[starsky](../../starsky/starsky/readme.md)  _mvc application / web interface [(files)](../../starsky/starsky/)___
   - [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface [(files)](../../starsky/starskysynccli/)_
   - [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface [(files)](../../starsky/starskyimportercli/)
   - [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests [(files)](../../starsky/starskyTests)_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

## starsky/starsky docs

### Structure configuration:

When setup Starksy there are two options to configure the installation.
There is a list of required settings. First the `appsettings.json` is loaded and the
environment variables are overwriting features.
The commandline arguments are shortcuts to set an in-app environment variable

### The order of reading settings
1) You can use `appsettings.json` inside the application folder to set base settings
2) Use Environment variables to overwrite those base settings
3) Command line argumements in the Cli applications to set in-app environment variables

### Required settings
1) `ThumbnailTempFolder` - For storing thumbnails
2) `StorageFolder` - For the main photo directory
3) `DatabaseType` - `mysql`, `sqlite` or  `inmemorydatabase` are supported
4) `DatabaseConnection` - The connectionstring to the database
5) `ExifToolPath` - A path to Exiftool.exe
### Optional settings
1) `Structure` - The structure that will be used when you import files, has a default fallback.
2) `ReadOnlyFolders` - Accepts a list of folders that never may be edited, defaults a emphy list
3) `AddMemoryCache`- Enable caching

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
    "AddMemoryCache": false
  }
}
```
#### Appsettings Notes
1)   Structure uses slash as directory separators for Linux and Windows
2)   The settings: `ExifToolPath`, `ThumbnailTempFolder` and  `STARSKY_BASEPATH` uses the system path directory separators
3)  When using Windows please double escape (`\\`) system path's


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
- [Search](readme_api.md#search)
- [Environment info](readme_api.md#environment-info)

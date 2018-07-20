# Starsky
## List of Starksy Projects
 - [inotify-settings](../../inotify-settings) _to setup auto indexing on linux [(docs)](../../inotify-settings/readme.md)_
 - [starsky (sln)](../../starsky) _database photo index & import index project [(docs)](../../starsky/readme.md)_
   - __[starsky](../../starsky/starsky)  _mvc application / web interface [(docs)](../../starsky/starsky/readme.md)___
   - [starsky-cli](../../starsky/starsky-cli)  _database command line interface [(docs)](../../starsky/starsky-cli/readme.md)_
   - [starskyimportercli](../../starsky/starskyimportercli)  _import command line interface [(docs)](../../starsky/starskyimportercli/readme.md)
   - [starskyTests](../../starsky/starskyTests)  _mstest unit tests_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

## starsky/starsky docs

### Structure configuration:

When setup Starksy there are two options to configure the installation

1) You can use `appsettings.json` inside the application folder
2) Environment variables with the same name

### Appsettings.json example
```json
{
  "ConnectionStrings": {
    "ThumbnailTempFolder": "Y:\\data\\photodirectory\\temp",
    "STARSKY_BASEPATH": "Y:\\data\\photodirectory\\storage",
    "DatabaseType": "mysql",
    "DefaultConnection": "Server=mysqlserver.nl;database=dbname;uid=username;pwd=password;",
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
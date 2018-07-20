# Starsky
## List of Starksy Projects
 - [inotify-settings](../../inotify-settings) _to setup auto indexing on linux [(docs)](../../inotify-settings/readme.md)_
 - [starsky (sln)](../../starsky) _database photo index & import index project [(docs)](../../starsky/readme.md)_
   - [starsky](../../starsky/starsky)  _mvc application / web interface [(docs)](../../starsky/starsky/readme.md)_
   - __[starsky-cli](../../starsky/starsky-cli)  _database command line interface [(docs)](../../starsky/starsky-cli/readme.md)___
   - [starskyimportercli](../../starsky/starskyimportercli)  _import command line interface [(docs)](../../starsky/starskyimportercli/readme.md)_
   - [starskyTests](../../starsky/starskyTests)  _mstest unit tests_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

## Starsky-cli docs

### Starsky-cli Indexer Help:
The goal of this wrapper is to get command line access to the photo index database

### Before you start

When you start this application at first please update the `appsettings.json`
```json
{
  "ConnectionStrings": {
    "ThumbnailTempFolder": "Y:\\data\\photodirectory\\temp",
    "STARSKY_BASEPATH": "Y:\\data\\photodirectory\\storage",
    "DatabaseType": "mysql",
    "DefaultConnection": "Server=mysqlserver.nl;database=dbname;uid=username;pwd=password;",
    "ExifToolPath": "C:\\exiftool.exe",
    "Structure": "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext",
    "ReadOnlyFolders": ["/2015","/2018"],
    "AddMemoryCache": false
  }
}
```

#### Appsettings Notes
1)   The `Structure`-setting is used by the `StarskyImporterCli` and the `/import` endpoint. 
    This always uses slash as directory marker. 
2)   The settings: `ExifToolPath`, `ThumbnailTempFolder` and  `STARSKY_BASEPATH` uses the system path.
When using Windows please escape the backslash, otherwise the application will crash.
3)    The `AddMemoryCache` setting is ignored in the console/cli applications 


### To get help:
```sh
starskycli --help
```

### The StarskyCli --Help window:
```
--help or -h == help (this window)
--subpath or -s == parameter: (string) ; path inside the index, default '/'
--path or -p == parameter: (string) ; fullpath, search and replace first part of the filename '/'
--index or -i == parameter: (bool) ; enable indexing, default true
--thumbnail or -t == parameter: (bool) ; enable thumbnail, default false
--orphanfolder or -o == To delete files without a parent folder (heavy cpu usage), default false
--verbose or -v == verbose, more detailed info
--databasetype or -d == Overwrite EnvironmentVariable for DatabaseType
--basepath or -b == Overwrite EnvironmentVariable for STARSKY_BASEPATH
--connection or -c == Overwrite EnvironmentVariable for DefaultConnection
--thumbnailtempfolder or -f == Overwrite EnvironmentVariable for ThumbnailTempFolder
--exiftoolpath or -e == Overwrite EnvironmentVariable for ExifToolPath
  use -v -help to show settings:
```

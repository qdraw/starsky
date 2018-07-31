# Starsky
## List of Starksy Projects
 - [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux [(files)](../../inotify-settings)_
 - [starsky (sln)](../../starsky/readme.md) _database photo index & import index project [(files)](../../starsky)_
   - [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface [(files)](../../starsky/starsky)_
   - [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface [(files)](../../starsky/starskysynccli)_
   - __[starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface [(files)](../../starsky/starskyimportercli)___
   - [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests [(files)](../../starsky/starskyTests)_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

## starskyimportercli docs

## Config file (appsettings.json)
For more information about the `appsettings.json` configuration

### Structure configuration:
The default structure in `appsettings.json` is:
```json
{
  "App": {
    "ThumbnailTempFolder": "/data/photodirectory/temp",
    "StorageFolder": "/data/photodirectory/storage",
    "DatabaseType": "sqlite",
    "DatabaseConnection": "Data Source=data.db",
    "ExifToolPath": "/usr/local/bin/exiftool",
    "Structure": "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext",
    "ReadOnlyFolders": ["/2015","/2018"],
    "AddMemoryCache": false
  }
}

```
#### Appsettings Notes
1)   Structure uses slash as directory separator for Linux and Windows
2)   The settings: `ExifToolPath`, `ThumbnailTempFolder` and  `StorageFolder` uses the system path directory separators
3)    The `AddMemoryCache` setting is ignored in the console/cli applications 

### Structure configuation options:

- `dd` 	 -   The day of the month, from 01 through 31.
- `MM` 	 -   The month, from 01 through 12.
- `yyyy` 	-    The year as a four-digit number.
- `HH` 	 -   The hour, using a 24-hour clock from 00 to 23.
- `mm` 	 -   The minute, from 00 through 59.
- `ss` 	 -   The second, from 00 through 59.
- `\\`     -      (double escape sign or double backslash); to escape dd use this: \\\d\\\d
- `/`     -       (slash); is split in folder (Windows / Linux / Mac)
- `.ext`   -       (dot ext); extension for example: .jpg
- `{filenamebase}` - use the orginal filename without extension
- `*`      -     (asterisk); match anything
- `*starksy*    -   Match the folder match that contains the word 'starksy' 

Check for more date conversions:
https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings



### To get help:
```sh
starskyimportercli --help
```

### The StarskyImporterCli --Help window:
```
--help or -h == help (this window)
--path or -p == parameter: (string) ; fullpath, can be an folder or file
--move or -m == delete file after importing (default false / copy file)
--all or -a == import all files including files older than 2 years (default: false / ignore old files) 
--recursive or -r == Import Directory recursive (default: false / only the selected folder) 
--verbose or -v == verbose, more detailed info, use -v -help to show settings:
```


## Structure Examples
### Good examples
#### In the main folder
```
 input: /yyyyMMdd_HHmmss.ext
 output: /20180731_215100.jpg
```

#### In the subfolder 2018
```
 input: /yyyy/yyyyMMdd_HHmmss.ext
 output: /2018/20180731_215100.jpg
```
#### Using orginal name
```
  input: /yyyy/{filenamebase}.ext
  output: /2018/example.jpg
```
### Bad examples

#### Exception due missing starting slash
```
  input: yyyy/yyyyMMdd_HHmmss.ext
  output: (Exception dus missing starting slash)
```
#### Only an asterix
```
  input: /*/yyyyMMdd_HHmmss.ext
```
> Todo: find out what the result is

#### Exception due missing extension
```
  input: yyyy/yyyyMMdd_HHmmss
  output: (Exception dus missing missing extension)
```
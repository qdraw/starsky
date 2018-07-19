# Starsky
## List of Starksy Projects
 - [inotify-settings](../../inotify-settings) _to setup auto indexing on linux [(docs)](../../inotify-settings/readme.md)_
 - [starsky (sln)](../../starsky) _database photo index & import index project [(docs)](../../starsky/readme.md)_
   - [starsky](../../starsky/starsky)  _mvc application / web interface [(docs)](../../starsky/starsky/readme.md)_
   - [starsky-cli](../../starsky/starsky-cli)  _database command line interface [(docs)](../../starsky/starsky-cli/readme.md)_
   - __[starskyimportercli](../../starsky/starskyimportercli)  _import command line interface [(docs)](../../starsky/starskyimportercli/readme.md)___
   - [starskyTests](../../starsky/starskyTests)  _mstest unit tests_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

## starskyimportercli docs

## Config file (.config.json)
To use custom configuration place a `.config.json` file inside the: appsettings `STARSKY_BASEPATH` folder
For more information about the `appsettings.json` configuration

### Structure configuration:
The default structure in `.config.json` is:
```json
{
	"structure":  "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext"
}
```
>   Structure uses slash as directory separator for Linux and Windows

### Structure configuation options:

- dd 	 -   The day of the month, from 01 through 31.
- MM 	 -   The month, from 01 through 12.
- yyyy 	-    The year as a four-digit number.
- HH 	 -   The hour, using a 24-hour clock from 00 to 23.
- mm 	 -   The minute, from 00 through 59.
- ss 	 -   The second, from 00 through 59.
- \\\     -      (double escape sign or double backslash); to escape dd use this: \\\d\\\d
- /     -       (slash); is split in folder (Windows / Linux / Mac)
- .ext   -       (dot ext); extension for example: .jpg
- (nothing)  -   extension is forced
- {filenamebase} - use the orginal filename without extension
- \*      -     (asterisk); match anything
- \*starksy\*    -   Match the folder match that contains the word 'starksy' 

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
--verbose or -v == verbose, more detailed info, use -v -help to show settings:
```
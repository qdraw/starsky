# Starsky
## List of Starksy Projects
 - [inotify-settings](../inotify-settings/readme.md) _to setup auto indexing on linux [(files)](../inotify-settings)_
 - __[starsky (sln)](../starsky/readme.md) _database photo index & import index project [(files)](../starsky)___
   - [starsky](../starsky/starsky/readme.md)  _mvc application / web interface [(files)](../starsky/starsky)_
   - [starsky-cli](../starsky/starsky-cli/readme.md)  _database command line interface [(files)](../starsky/starsky-cli)_
   - [starskyimportercli](../starsky/starskyimportercli/readme.md)  _import command line interface [(files)](../starsky/starskyimporterclid)_
   - [starskyTests](../starsky/starskyTests/readme.md)  _mstest unit tests [(files)](../starsky/starskyTests)_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../starskyapp) _React-Native app (Pre-alpha code)_

## General Starsky (sln) docs


### Install instructions

(incomplete)

1. Download exiftool
2. Update the `appsettings.json` configuration before starting
> Windows: use double escape \\\\ in config directory paths


### Install on Raspberry Pi (Raspbian/Linux ARM)
On Linux ARM there is no SDK avaiable, but the runtime works. So you have to compile it first on your laptop and copy to your Raspberry Pi. Use the following steps to setup:

1) On your laptop (x86/x64 machine). Get the `dotnet` 2.0 or newer SDK. To get the 'Build apps - SDK' .NET Core from https://www.microsoft.com/net/download

2) Compile the Starsky-installation on your x86/x64 machine first. I use the following wrapper to build all projects for Linux ARM [`publish-linux-arm.sh`](publish-linux-arm.sh)
    ```
    ./publish-linux-arm.sh
    ```

3) Copy contents from the `linux-arm` folder to your Raspberry Pi

4) On the Raspi, install the dependency packages first. Those are required by .NET Core
    ```
    sudo apt-get install curl libunwind8 gettext apt-transport-https
    ```

5)  Setup `appsettings.json` configuration
    This is the most basic configuration. There are more options available
    ```json
    {
      "ConnectionStrings": {
          "ThumbnailTempFolder": "/home/pi/starsky_thumbnails",
    	  "STARSKY_BASEPATH": "/home/pi/starsky_base",
          "DatabaseType": "sqlite",
          "DefaultConnection": "Data Source=data.db",
          "ExifToolPath": "/usr/bin/exiftool",
          "ReadOnlyFolders": []
    	}
    }   
    ```
6) Run `./starsky`

#### Errors
When using SQLite as databasetype without `SQLitePCLRaw.lib.e_sqlite3.linux` the following error appears:
`System.DllNotFoundException: Unable to load DLL 'e_sqlite3'`

To avoid the error: `System.IO.FileLoadException` `Microsoft.Extensions.Options, Version=2.0.2.0` the package Microsoft.EntityFrameworkCore is installed


### Bash build and configuation scripts

Those scripts are optional and used for configuation.

### pm2 `new-pm2.sh`
The script [`new-pm2.sh`](new-pm2.sh) is a script to setup Starsky using [pm2](http://pm2.keymetrics.io/).
```sh
 export ASPNETCORE_URLS="http://localhost:4823/"
 export ASPNETCORE_ENVIRONMENT="Production"
 ```

 ### Publish for platform scripts

 The scripts that are used to create a full build. (Linux has `libunwind8` and `gettext` as dependency)
  - [`publish-linux-arm.sh`](publish-linux-arm.sh) Linux ARM (Raspberry Pi 2/3)
  - [`publish-mac.sh`](publish-mac.sh) OS X 10.12+
  - [`publish-windows.sh`](publish-windows.sh) Windows 7+

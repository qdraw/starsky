# Starsky
## List of [Starsky](../readme.md) Projects
 * [inotify-settings](../inotify-settings/readme.md) _to setup auto indexing on linux_
 * __[starsky (sln)](../starsky/readme.md) database photo index & import index project__
    * [starsky](../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskysynccli](../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyimportercli](../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyTests](../starsky/starskyTests/readme.md)  _mstest unit tests_
    * [starskyWebHtmlCli](../starsky/starskywebhtmlcli/readme.md)  _publish web images to html files_
    * [starskyGeoCli](../starsky/starskygeocli/readme.md)  _gpx sync and reverse geotagging_
 * [starsky-node-client](../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## General Starsky (sln) docs


### Build instructions

1.  Clone the repo
```sh
git clone "https://bitbucket.org/qdraw/starsky.git"
```
2.  Get the `dotnet` 2.0 or newer SDK. To get the 'Build apps - SDK' .NET Core from https://www.microsoft.com/net/download

3.  Download and install [ExifTool by Phil Harvey](https://www.sno.phy.queensu.ca/~phil/exiftool/)
```sh
brew install exiftool
```
Or using Chocolatey under Windows:
```cmd
choco install exiftool
```   

4. Make a build and run the tests
from the root folder of the repository
```sh
dotnet build starsky
dotnet test starsky/*Tests
```

4.  Link `starsky/starsky/appsettings.json` to the exiftool excutable
>>   Windows: use double escape \\\\ in config directory paths
```json
{
    "App": {
        "ExifToolPath": "/usr/bin/exiftool",
    }
}   
```
5.  Run
```sh
dotnet run --project starsky/starsky
```

6.  Create a account in the starsky application. Those credentials are only required by the web application
> Security issue: Please be aware that this endpoint is always open to everyone
```
http://localhost:64556/account/register
```

### Dev-dependencies:
1.  Editorconfig (http://editorconfig.org/)
2.  Use [DB Browser for SQLite](https://sqlitebrowser.org/) to view a local SQLite database


### Build for Raspberry Pi (Raspbian/Linux ARM)
On Linux ARM there is no SDK avaiable, but the runtime works. So you have to compile it first on your laptop and copy to your Raspberry Pi. Use the following steps to setup:

1.  Clone the repo on your x86/x64 machine
```sh
git clone "https://bitbucket.org/qdraw/starsky.git"
```

2.  On your laptop (x86/x64 machine). Get the `dotnet` 2.0 or newer SDK. To get the 'Build apps - SDK' .NET Core from https://www.microsoft.com/net/download

3.  Compile the Starsky-installation on your x86/x64 machine first. I use the following wrapper to build all projects for Linux ARM [`publish-linux-arm.sh`](publish-linux-arm.sh)
```sh
./publish-linux-arm.sh
```

4.  Copy contents from the `linux-arm` folder to your Raspberry Pi

5.  On the Raspi, install the dependency packages first. Those are required by .NET Core
```sh
sudo apt-get install curl libunwind8 gettext apt-transport-https
```
6.  On the Raspi, install Exiftool
```sh
sudo apt-get install libimage-exiftool-perl
```


7.  Setup `appsettings.json` configuration
    This is the most basic configuration. There are more options available
```json
{
    "App": {
        "ExifToolPath": "/usr/bin/exiftool",
    }
}   
```
8.  Run the Starsky web interface
```sh
./starsky
```
9.  Create a account in the starsky application. Those credentials are only required by the web application
> Security issue: Please be aware that this endpoint is always open to everyone
```
http://localhost:5000/account/register
```

#### Optional steps  
10.  The script [`pm2-starksy-new.sh`](starsky/pm2-starksy-new.sh) is a script to setup Starsky using [pm2](http://pm2.keymetrics.io/).
```sh
    export ASPNETCORE_URLS="http://localhost:4823/"
    export ASPNETCORE_ENVIRONMENT="Production"
```

#### Errors
When using SQLite as databasetype without `SQLitePCLRaw.lib.e_sqlite3.linux` the following error appears:
`System.DllNotFoundException: Unable to load DLL 'e_sqlite3'`

To avoid the error: `System.IO.FileLoadException` `Microsoft.Extensions.Options, Version=2.0.2.0` the package `Microsoft.EntityFrameworkCore is installed


### Bash build and configuation scripts

Those scripts are optional and used for configuation.

### pm2 `pm2-starksy-new.sh`
The script [`pm2-starksy-new.sh`](starsky/pm2-starksy-new.sh) is a script to setup Starsky using [pm2](http://pm2.keymetrics.io/).
```sh
 export ASPNETCORE_URLS="http://localhost:4823/"
 export ASPNETCORE_ENVIRONMENT="Production"
```

### Publish-scripts for selfcontaining binaries

The scripts that are used to create a full build. (Linux has `libunwind8` and `gettext` as dependency)
  - [`publish-linux-arm.sh`](publish-linux-arm.sh) Linux ARM (Raspberry Pi 2/3)
  - [`publish-mac.sh`](publish-mac.sh) OS X 10.12+
  - [`publish-windows.sh`](publish-windows.sh) Windows 7+

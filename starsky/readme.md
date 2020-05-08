# General Starsky (sln) docs
## List of [Starsky](../readme.md) Projects
 * [inotify-settings](../inotify-settings/readme.md) _to setup auto indexing on linux_
 * __[starsky (sln)](../starsky/readme.md) database photo index & import index project__
    * [starsky](../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskySyncCli](../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [Starsky Business Logic](../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../starskyapp/readme.md) _Desktop Application (Pre-alpha code)_

## Build instructions

1.  To get started clone the repository

```sh
git clone "https://bitbucket.org/qdraw/starsky.git"
```

2.  Get the `dotnet` 3.1.201 or newer SDK. To get the 'Build apps - SDK' .NET Core from https://www.microsoft.com/net/download

3.  Download and install [ExifTool by Phil Harvey](https://www.sno.phy.queensu.ca/~phil/exiftool/)

```sh
brew install exiftool
```

Or using Chocolatey under Windows:

```cmd
choco install exiftool
```   

4. Make a build of all the projects and run the tests
from the root folder of the repository

_When using powershell:_

```powershell
    .\build.ps1
```

_Or using bash (on Linux and Mac OS)_
```sh
    ./build.sh
```

4.  Link `starsky/starsky/appsettings.json` to the exiftool executable

>>   Windows: use double escape `\\` in config directory paths

```json
{
    "App": {
        "ExifToolPath": "/usr/bin/exiftool",
    }
}   
```

>>   Tip: You could use machine specific configuration files: appsettings.{machinename}.json _(and replace {machinename} with your computer name in lowercase)_


5.  Run

```sh
dotnet run --project starsky/starsky
```

6.  Create a account in the Starsky application. Those credentials are only required by the web application
> Security issue: Please be aware that this endpoint is always open to everyone

```
http://localhost:64556/account/register
```

## Dev-dependencies:
1.  Editorconfig (http://editorconfig.org/)
2.  Use [DB Browser for SQLite](https://sqlitebrowser.org/) to view a local SQLite database _(optional)_


## Build for Raspberry Pi (Raspbian/Linux ARM)
From .NET Core 2.1 or newer there is a SDK available for Raspberry Pi (only ARMv7 or newer, so no ARMv6). We use NET Core 3.1.201
But in this guide we build it first on your laptop and copy to your Raspberry Pi. Use the following steps to setup:

1.  Clone the repo on your x86/x64 machine

```sh
git clone "https://github.com/qdraw/starsky.git"
```

2.  On your laptop (x86/x64 machine). Get the `dotnet` 3.0.1 or newer SDK. To get the 'Build apps - SDK' .NET Core from https://www.microsoft.com/net/download

3.  Compile the Starsky-installation on your x86/x64 machine first. The cake build script with parameters can be used to build for this runtime.

_Using bash_
```sh
./build.sh --runtime="linux-arm"
```

_Using powershell_
```powershell
powershell -File build.ps1 -ScriptArgs '-runtime="linux-arm"'
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

7.  Setup `appsettings.json` configuration. This is the most basic configuration. There are more options available

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

9.  Create a account in the Starsky Web application. Those credentials are only required by the web application

> This endpoint is open to anyone when there are no accounts or for a logged in user or when you update appsettings.json

```
http://localhost:5000/account/register
```

### Optional steps  
10.  The script [`pm2-new-instance.sh`](starsky/pm2-new-instance.sh) is a script to setup Starsky using [pm2](http://pm2.keymetrics.io/).

```sh
    export ASPNETCORE_URLS="http://localhost:4823/"
    export ASPNETCORE_ENVIRONMENT="Production"
```

11. SonarQube scanner

For SonarScanner you need to additionally install:
- .NET Core 2.0 runtime: https://dotnet.microsoft.com/download/dotnet-core/2.0
- Java

To enable SonarScanner you need to set the following environment variables:
- `STARSKY_SONAR_KEY` - the public name of the project
- `STARSKY_SONAR_LOGIN` - the token to login
- `STARSKY_SONAR_ORGANISATION` - the name of the organisation
- `STARSKY_SONAR_URL` - defaults to sonarcloud.io

#### Known errors
When using SQLite as databasetype without `SQLitePCLRaw.lib.e_sqlite3.linux` the following error appears:
`System.DllNotFoundException: Unable to load DLL 'e_sqlite3'`

To avoid the error: `System.IO.FileLoadException` `Microsoft.Extensions.Options, Version=2.0.2.0` the package `Microsoft.EntityFrameworkCore is installed

> Tip: When using MariaDB or MySQL as database, make sure you use `utf8mb4` and as collate `utf8mb4_unicode_ci` to avoid encoding errors.

#### Cannot open shared object file: Permission denied

```
./starsky
Failed to load ?? error: <yourpath>/libhostfxr.so: cannot open shared object file: Permission denied
The library libhostfxr.so was found, but loading it from <yourpath>/libhostfxr.so failed
  - Installing .NET Core prerequisites might help resolve this problem.
     https://go.microsoft.com/fwlink/?LinkID=798306&clcid=0x409
```

Check your file rights in the folders, they should be 644 for files and 755 for folders.
except for the executable  files

### Bash build and configuation scripts

Those scripts are optional and used for configuation.

#### pm2 `pm2-new-instance.sh`
The script [`pm2-new-instance.sh`](starsky/pm2-new-instance.sh) is a script to setup Starsky using [pm2](http://pm2.keymetrics.io/).
```sh
 export ASPNETCORE_URLS="http://localhost:4823/"
 export ASPNETCORE_ENVIRONMENT="Production"
```

#### pm2 `pm2-deploy-on-env.sh`

To remove the content of the parent folder of this script. The following content are not deleted: app settings, temp, zip files and database files. The starsky files will get executed and need to have those rights. The pm2 instance will be restarted.

#### pm2 `pm2-warmup.sh`

To warmup the installation after a restart this bash script is provided


#### Publish-scripts for 'self containing' binaries

The scripts that are used to create a full build. (Linux has `libunwind8` and `gettext` as dependency)

The Cake script provide options to build for specific runtimes.

### To build for Mac OS

_Using bash_
```sh
./build.sh --runtime="osx.10.12-x64"
```

_Using powershell_
```powershell
powershell -File build.ps1 -ScriptArgs '-runtime="osx.10.12-x64"'
```
### To build for 32 bits Windows

_Using bash_
```sh
./build.sh --runtime="win7-x86"
```

_Using powershell_
```powershell
powershell -File build.ps1 -ScriptArgs '-runtime="win7-x86"'
```

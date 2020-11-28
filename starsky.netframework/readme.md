# Starsky .NET Framework
## List of [Starsky](../readme.md) Projects
 * [starsky (sln)](../starsky/readme.md) _database photo index & import index project)_
    * [starsky](../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](../starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](../starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../starsky/starskytest/readme.md)  _mstest unit tests_
 * __[starsky.netframework](../starsky.netframework/readme.md) Client for older machines (deprecated)__
 * [starsky-tools](../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../starskyapp/readme.md) _Desktop Application (Pre-alpha code)_

## Starsky Client for older machines

> __The Net Framework version is marked as deprecated__

This application in only useful for older Mac OS machines. For Windows it still require 'Windows 7' or newer (_[reference](https://docs.microsoft.com/en-us/dotnet/framework/get-started/system-requirements)_)

> Note: The starsky.netframework can be outdated. The focus of Starsky is on `.NET Core` and not on `.NET Framework`

For all other Machines than Mac OS X 10.11 please continue at:
- [starskysynccli](../starsky/starskysynccli/readme.md)  _database command line interface_
- [starskyImporterCli](../starsky/starskyimportercli/readme.md)  _import command line interface_

> TIP: Don't try this on Windows: You get this exception: `'System.Diagnostics.DiagnosticSource, Version=4.0.3.1`


### Install `mono`
When you have a Mac OS X 10.11 Machine install `mono` first.

```sh
brew install mono
```

- or go to the [install page of the Mono project](https://www.mono-project.com/docs/getting-started/install/mac/)  

### Build

Run the build script in the `starsky.netframework` folder to get a executable  

```sh
./build.sh
```

### Run the Sync application
To run the application can execute the following script
```sh
mono netframework-msbuild/starskySyncNetFrameworkCli.exe -h -v
```

...or within the build directory
```sh
mono starskySyncNetFrameworkCli/bin/Release/starskySyncNetFrameworkCli.exe -h -v
```


### Run the Importer application

To run the application can execute the following script
```sh
mono netframework-msbuild/starskyImporterNetFrameworkCli.exe -h -v
```

...or within the build directory
```sh
mono starskyImporterNetFrameworkCli/bin/Release/starskyImporterNetFrameworkCli.exe -h -v
```

### When running in develop mode:

Make sure that the  `appsettings.json`  and `data.db` are included in the following folder:

```
./starsky/bin/Debug
```

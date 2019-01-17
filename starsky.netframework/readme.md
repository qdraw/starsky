# Starsky
## List of [Starsky](../readme.md) Projects
 * [inotify-settings](../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../starsky/readme.md) _database photo index & import index project)_
    * [starsky](../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskycore](../starsky/starskycore/readme.md) _business logic (netstandard2.0)_
    * [starskysynccli](../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyimportercli](../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyTests](../starsky/starskyTests/readme.md)  _mstest unit tests_
    * [starskyWebHtmlCli](../starsky/starskywebhtmlcli/readme.md)  _publish web images to html files_
    * [starskyGeoCli](../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
 * __[starsky.netframework](../../starsky.netframework/readme.md) Client for older machines__
 * [starsky-node-client](../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## Starsky Client for older machines

This application in only useful for older Mac OS machines. For Windows it still require 'Windows 7' or newer (_[reference](https://docs.microsoft.com/en-us/dotnet/framework/get-started/system-requirements)_)

For all other Machines than Mac OS X 10.11 please continue at: 
- [starskysynccli](../starsky/starskysynccli/readme.md)  _database command line interface_


### Install `mono`
When you have a Mac OS X 10.11 Machine install `mono` first.

```sh
brew install mono
```

- or go to the [install page of the Mono project](https://www.mono-project.com/docs/getting-started/install/mac/)  

### Build

Run the release script in the `starsky.netframework` folder to get a executable  
```sh
./release-msbuild.sh
```


### Run
To run the application can execute the following script
```sh
mono bin/Release/starskySyncFramework.exe -h -v
```

### starsky.netframework is a wrapper for starskySyncCli
For more information please check the [starskysynccli](../starsky/starskysynccli/readme.md) documentation
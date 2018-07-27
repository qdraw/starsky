# Starsky
## List of Starksy Projects
 - [inotify-settings](inotify-settings/readme.md) _to setup auto indexing on linux [(files)](inotify-settings)_
 - [starsky (sln)](starsky/readme.md) _database photo index & import index project [(files)](starsky)_
   - [starsky](starsky/starsky/readme.md)  _mvc application / web interface [(files)](starsky/starsky)_
   - [starskysynccli](starsky/starskysynccli/readme.md)  _database command line interface [(files)](starsky/starskysynccli)_
   - [starskyimportercli](starsky/starskyimportercli/readme.md)  _import command line interface [(files)](starsky/starskyimportercli)_
   - [starskyTests](starsky/starskyTests/readme.md)  _mstest unit tests [(files)](starsky/starskyTests)_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](starskyapp) _React-Native app (Pre-alpha code)_

## Starsky   
An attempt to create a database driven photo library

The general application is Starsky (sln). You need to [install the solution](starsky/readme.md) first. The subapplications
[starskysynccli](starsky/starskysynccli/readme.md)  and [starskyimportercli](starsky/starskyimportercli/readme.md) uses the same configuation files. These projects are separately compiled using the build script.

## Build status

[![Visual Studio Team Services](https://img.shields.io/vso/build/qdraw/7bab52f1-7600-4295-a199-1bb81cc1e4d7/1.png)](https://qdraw.visualstudio.com/7bab52f1-7600-4295-a199-1bb81cc1e4d7/_apis/build/status/1) For the master branch using Windows and Visual Studio 2017

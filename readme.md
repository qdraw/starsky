# Starsky
## List of Starksy Projects
 - [inotify-settings](inotify-settings) _to setup auto indexing on linux [(docs)](inotify-settings/readme.md)_
 - [starsky (sln)](starsky) _database photo index & import index project [(docs)](starsky/readme.md)_
   - [starsky](starsky/starsky)  _mvc application / web interface [(docs)](starsky/starsky/readme.md)_
   - [starsky-cli](starsky/starsky-cli)  _database command line interface [(docs)](starsky/starsky-cli/readme.md)_
   - [starskyimportercli](starsky/starskyimportercli)  _import command line interface [(docs)](starsky/starskyimportercli/readme.md)_
   - [starskyTests](starsky/starskyTests)  _mstest unit tests_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](starskyapp) _React-Native app (Pre-alpha code)_

## Starsky   
An attempt to create a database driven photo library

The general application is Starsky (sln). You need to [install the solution](starsky/readme.md) first. The subapplications
[starsky-cli](starsky/starsky-cli/readme.md)  and [starskyimportercli](starsky/starskyimportercli/readme.md) uses the same configuation files. These projects are separately compiled using the build script.

## Build status

[![Visual Studio Team Services](https://qdraw.visualstudio.com/7bab52f1-7600-4295-a199-1bb81cc1e4d7/_apis/build/status/1)](https://qdraw.visualstudio.com/7bab52f1-7600-4295-a199-1bb81cc1e4d7/_apis/build/status/1) For the master branch using Windows and Visual Studio 2017

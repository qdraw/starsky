# Starsky
## List of __[Starsky](readme.md)__ Projects
 * [inotify-settings](inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](starsky/readme.md) _database photo index & import index project_
    * [starsky](starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskySyncCli](starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskycore](starsky/starskycore/readme.md) _business logic (netstandard 2.0)_
    * [starskyTest](starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](starskyapp/readme.md) _React-Native app (Pre-alpha code)_




## Starsky   
An attempt to create a database driven photo library

### Install instructions
The general application is Starsky (sln). You need to [install the solution](starsky/readme.md) first. The subapplications [starskysynccli](starsky/starskysynccli/readme.md)  and [starskyimportercli](starsky/starskyimportercli/readme.md) uses the same configuation files. These projects are separately compiled using the build script.

## Build status

### Windows 2019 with VS2019
[![Build Status](https://qdraw.visualstudio.com/starsky/_apis/build/status/starsky-full%20build?branchName=master)](https://qdraw.visualstudio.com/starsky/_build/latest?definitionId=5&branchName=master)

### Sonarqube
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=coverage)](https://sonarcloud.io/dashboard?id=starsky)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=bugs)](https://sonarcloud.io/dashboard?id=starsky)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=code_smells)](https://sonarcloud.io/dashboard?id=starsky)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=starsky)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=starsky)

## Changelog and history of this project

There is a version log and backlog available on the [history page](history.md)

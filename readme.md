# Project Readme
## List of __[Starsky](readme.md)__ Projects
 * [inotify-settings](inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](starsky/readme.md) _database photo index & import index project_
    * [starsky](starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskySyncCli](starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [Starsky Business Logic](starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](starskyapp/readme.md) _Desktop Application (Pre-alpha code)_

## What is Starsky?
> [Check the introduction page to learn more about the scope of the application](index.md)

## Demo app
Starsky has a demo application online. 
> Username: `demo@qdraw.nl` and Password: `demo@qdraw.nl`
>
> [Starsky Demo App](https://starskydemo.herokuapp.com)

## Project Readme   

### Install instructions for the server
This section deals with how to set up a Starsky system on-premises. You will find guides to all Starsky software for installation on-premises here.

### General Project
The general application is Starsky solution (sln). You need to [install the solution](starsky/readme.md) first.

### Command line tools
The command tools to sync the database manually use [Sync CLI](starsky/starskysynccli/readme.md) to generate thumbnail use [Sync CLI](starsky/starskysynccli/readme.md). The [Importer CLI](starsky/starskysynccli/readme.md)  can be used to copy files in a folder structure based on the creation datetime. The datetime structure can be configured.

To reverse geo code location information in images use the UI or the [Geo CLI](starsky/starskygeocli/readme.md). Use your photo to track location and match this with your camera.With this tool you add a location trail (gpx) to a folder and match the datetime to images in the folder.

To publish files generate markup and images with a logo use the [Web Html CLI](starsky/starskywebhtmlcli/readme.md). This publish web images to a content package. And when this is done you could copy a content package to a ftp service.

All these projects are separately compiled using the build script and using the same `appsettings` configuration.

## Build status

### Windows 2019 with VS2019
[![Build Status](https://qdraw.visualstudio.com/starsky/_apis/build/status/azure-pipelines-starsky?branchName=master)](https://qdraw.visualstudio.com/starsky/_build/latest?definitionId=17&branchName=master)

_See `./azure-pipelines-starsky.yml` for details_

![Starsky .NET Core (Windows)](https://github.com/qdraw/starsky/workflows/Starsky%20.NET%20Core%20(Windows)/badge.svg)
![Starsky .NET Core (Ubuntu)](https://github.com/qdraw/starsky/workflows/Starsky%20.NET%20Core%20(Ubuntu)/badge.svg)
![Starsky ClientApp (React)](https://github.com/qdraw/starsky/workflows/Starsky%20ClientApp%20(React)/badge.svg)

### End2End tests on public demo environment
![end2end on heroku-demo](https://github.com/qdraw/starsky/workflows/end2end%20on%20heroku-demo/badge.svg?branch=master)

_See `./starsky-tools/end2end` for details_

## Changelog and history of this project

There is a version log and backlog available on the [history and changelog page](history.md)

### Sonarqube
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=coverage)](https://sonarcloud.io/dashboard?id=starsky)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=bugs)](https://sonarcloud.io/dashboard?id=starsky)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=code_smells)](https://sonarcloud.io/dashboard?id=starsky)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=starsky)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=starsky)

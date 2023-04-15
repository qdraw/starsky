# [Starsky](https://docs.qdraw.nl/) &middot; [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](../LICENSE.md) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md) ![GitHub all releases](https://img.shields.io/github/downloads/qdraw/starsky/total?label=release%20downloads) ![Docker](https://img.shields.io/docker/pulls/qdraw/starsky.svg) ![GitHub Repo stars](https://img.shields.io/github/stars/qdraw/starsky?label=Give%20me%20a%20star%20please&style=social) ![GitHub issues](https://img.shields.io/github/issues-raw/qdraw/starsky) [![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/qdraw/starsky?include_prereleases)](https://github.com/qdraw/starsky/releases/)

## List of __[Starsky](readme.md)__ Projects
 * [By App documentation](starsky/readme.md) _database photo index & import index project_
    * [starsky](starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](starsky/starskybusinesslogic/readme.md) _business logic libraries (.NET)_
    * [starskyTest](starsky/starskytest/readme.md)  _mstest unit tests (for .NET)_
 * [starsky-tools](starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [Starsky Desktop](starskydesktop/readme.md) _Desktop Application_
    * [Download Desktop App](https://docs.qdraw.nl/download/) _Windows and Mac OS version_
 * [Changelog](history.md) _Release notes and history_

## What is Starsky?
> [Check the introduction page to learn more about the scope of the application](index.md)

## Demo app
Starsky has a demo application online.

[See the online demo](https://demo.qdraw.nl)

> Is not needed to login, but you can create a new account

## Project Readme   

### Install instructions for the server
This section deals with how to set up a Starsky system on-premises. You will find guides to all Starsky software for installation on-premises here.

### General Project
The general application is Starsky solution (sln). You need to [install the solution](starsky/readme.md) first.

### Command line tools
The command tools to sync the database manually use [Synchronize CLI](starsky/starskysynchronizecli/readme.md) to generate thumbnail use [Thumbnail CLI](starsky/starskythumbnailcli/readme.md). The [Importer CLI](starsky/starskyimportercli/readme.md)  can be used to copy files in a folder structure based on the creation datetime. The datetime structure can be configured.

To reverse geo code location information in images use the UI or the [Geo CLI](starsky/starskygeocli/readme.md). Use your photo to track location and match this with your camera.With this tool you add a location trail (gpx) to a folder and match the datetime to images in the folder.

To publish files generate markup and images with a logo use the [Web Html CLI](starsky/starskywebhtmlcli/readme.md). This publish web images to a content package. And when this is done you could copy a content package to a ftp service.

All these projects are separately compiled using the build script and using the same application settings (`appsettings`) configuration.

## GitHub Issues

### Pay attention

Please do not open an issue on GitHub, unless you have spotted an actual bug in Starsky.

Use [GitHub Discussions](https://github.com/qdraw/starsky/discussions) to ask questions, bring up ideas, or other general items. Issues are not the place for questions, and will either be converted to a discussion or closed.

This policy is in place to avoid bugs being drowned out in a pile of sensible suggestions for future enhancements and calls for help from people who forget to check back if they get it and so on.

If a feature request is actually going to be built, it will get its own issue with the tag: Feature Request

## Latest stable release

[![Release](https://img.shields.io/github/v/release/qdraw/starsky)](https://github.com/qdraw/starsky/releases/)

## Latest prerelease

[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/qdraw/starsky?include_prereleases)](https://github.com/qdraw/starsky/releases/)


## Build status

### Azure pipeline
[![Build Status](https://qdraw.visualstudio.com/starsky/_apis/build/status/azure-pipelines-starsky?branchName=master)](https://qdraw.visualstudio.com/starsky/_build/latest?definitionId=17&branchName=master)

_See `./pipelines/azure` for details_

### Github Actions 

#### Windows

![Starsky .NET Core (Windows)](https://github.com/qdraw/starsky/workflows/Starsky%20.NET%20Core%20(Windows)/badge.svg)

#### Ubuntu

![Starsky .NET Core (Ubuntu)](https://github.com/qdraw/starsky/workflows/Starsky%20.NET%20Core%20(Ubuntu)/badge.svg)
[![ClientApp React Linux CI](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-linux-ci.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-linux-ci.yml)

#### Docker
[![Docker buildx multi-arch CI unstable master](https://github.com/qdraw/starsky/actions/workflows/starsky-docker-buildx.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/starsky-docker-buildx.yml)

#### App on Windows and Mac OS

Without running .NET dependency. Without .NET the app can't run

![StarskyApp Electron (Missing .NET dependency)](https://github.com/qdraw/starsky/workflows/StarskyApp%20Electron%20(Missing%20.NET%20dependency)/badge.svg)

Included with .NET dependency 

[![Create Desktop Release on tag for .Net Core and Electron](https://github.com/qdraw/starsky/actions/workflows/release-on-tag-netcore-desktop-electron.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/release-on-tag-netcore-desktop-electron.yml)

_See `./.github/workflows` for details_

### End2End tests on ubuntu github actions ci environment
![end2end on ubuntu-ci](https://github.com/qdraw/starsky/actions/workflows/end2end-ubuntu-ci.yml/badge.svg)

_See `./starsky-tools/end2end` for details_

## Changelog and history of this project

There is a version log and backlog available on the [history and changelog page](history.md)

### Codecov
[![codecov](https://codecov.io/gh/qdraw/starsky/branch/master/graph/badge.svg?token=MUCQuYH99y)](https://codecov.io/gh/qdraw/starsky)

### Sonarqube
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=coverage)](https://sonarcloud.io/summary/new_code?id=starsky)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=bugs)](https://sonarcloud.io/dashboard?id=starsky)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=code_smells)](https://sonarcloud.io/dashboard?id=starsky)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=starsky)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=starsky)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=starsky)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=starsky&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=starsky)

### Licence
![MIT License](https://img.shields.io/static/v1?label=Licence&message=MIT&color=green)

## Authors
- [@qdraw](https://www.github.com/qdraw)

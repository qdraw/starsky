---
sidebar_position: 8
---

# Github Actions CI

GitHub Actions makes it easy to automate all your software workflows, now with world-class CI/CD.
Build, test, and deploy your code right from GitHub.

[View all github actions on github](https://github.com/qdraw/starsky/tree/master/.github/workflows)

## Important pipelines

- [Build desktop app (Create Desktop Release on tag for .Net Core and Electron)](#create-desktop-release-on-tag-for-net-core-and-electron)
- [Docker Hub on new version (Create Release on tag for docker hub)](#create-release-on-tag-for-docker-hub)
- [Docker unstable build (Docker buildx multi-arch CI unstable master)](#docker-buildx-multi-arch-ci-unstable-master)

## Table of Contents

1. [Create Desktop Release on tag for .Net Core and Electron](#create-desktop-release-on-tag-for-net-core-and-electron)
2. [Docker Hub on new version (Create Release on tag for Docker Hub)](#create-release-on-tag-for-docker-hub)
3. [Docker unstable build (Docker buildx multi-arch CI unstable master)](#docker-buildx-multi-arch-ci-unstable-master)
4. [Auto upgrade .NET SDK version](#auto-upgrade-net-sdk-version)
5. [Application Version Auto update](#application-version-auto-update)
6. [Auto update Nuget packages list](#auto-update-nuget-packages-list)
7. [Auto Update Swagger](#auto-update-swagger)
8. [Auto clientapp create Vite upgrade](#auto-clientapp-create-react-app-upgrade)
9. [Auto Documentation create Docusaurus upgrade](#auto-documentation-create-docusaurus-upgrade)
10. [ClientApp React Linux CI](#clientapp-react-linux-ci)
11. [ClientApp React Windows CI](#clientapp-react-windows-ci)
12. [CodeQL analysis](#codeql-analysis)
13. [Documentation to GitHub Pages](#documentation-to-github-pages)
14. [End-to-End on Ubuntu CI](#end2end-on-ubuntu-ci)
15. [End-to-End on Windows CI](#end2end-on-windows-ci)
16. [Create Release on tag for Docker Hub](#create-release-on-tag-for-docker-hub)

# All github actions used by this project

There are multiple github actions used by this project. Bellow is a list of all github actions and a
short on alphabet order.

## Auto upgrade .NET SDK version

Run weekly
Automatically upgrade the .NET SDK version and upgrade nuget packages file when a new version is
released.

[![webapp-update-dotnet-sdk-version-upgrade](https://github.com/qdraw/starsky/actions/workflows/webapp-update-dotnet-sdk-version-upgrade.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/webapp-update-dotnet-sdk-version-upgrade.yml)

```bash
cd starsky-tools/build-tools/
npm run nuget-package-list
```

> [webapp-update-dotnet-sdk-version-upgrade.yml](https://github.com/qdraw/starsky/actions/workflows/webapp-update-dotnet-sdk-version-upgrade.yml)

## Application Version Auto update

Run on push and manual trigger
Upgrade the application version in the csproj file and package.json file

[![global-update-application-version-auto](https://github.com/qdraw/starsky/actions/workflows/global-update-application-version-auto.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/global-update-application-version-auto.yml)

```bash
cd starsky-tools/build-tools/
npm run app-version-update
```

> [global-update-application-version-auto.yml](https://github.com/qdraw/starsky/actions/workflows/global-update-application-version-auto.yml)

## Auto update Nuget packages list

On push on master branch
Creates a list of nuget packages and their version in the nuget-package-list file

[![webapp-update-nuget-packages-list](https://github.com/qdraw/starsky/actions/workflows/webapp-update-nuget-packages-list.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/webapp-update-nuget-packages-list.yml)

```bash
cd starsky-tools/build-tools/
npm run nuget-package-list
```

> [webapp-update-nuget-packages-list.yml](https://github.com/qdraw/starsky/actions/workflows/webapp-update-nuget-packages-list.yml)

## Auto Update Swagger

Update json file with the latest swagger file

[![webapp-update-swagger-dotnet](https://github.com/qdraw/starsky/actions/workflows/webapp-update-swagger-dotnet.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/webapp-update-swagger-dotnet.yml)

```bash
cd starsky/starsky
export app__AddSwagger="true"
export app__AddSwaggerExport="true"
export app__AddSwaggerExportExitAfter="true"
dotnet run --no-launch-profile
cd ../../
cd starsky-tools/build-tools/
npm run nuget-package-list
```

> [webapp-update-swagger-dotnet.yml](https://github.com/qdraw/starsky/actions/workflows/webapp-update-swagger-dotnet.yml)

## auto clientapp create Vite upgrade

Bootstrap the client app with the latest Vite version

[![clientapp-update-dependencies-vite](https://github.com/qdraw/starsky/actions/workflows/clientapp-update-dependencies-vite.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/clientapp-update-dependencies-vite.yml)

```bash
cd starsky-tools/build-tools/
npm run clientapp-create-react-app-update
```

> [clientapp-update-dependencies-vite.yml](https://github.com/qdraw/starsky/actions/workflows/clientapp-update-dependencies-vite.yml)

## Auto Documentation create Docusaurus upgrade

Bootstrap the docs with the latest Docusaurus version

[![documentation-update-dependencies-create-docusaurus](https://github.com/qdraw/starsky/actions/workflows/documentation-update-dependencies-create-docusaurus.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/documentation-update-dependencies-create-docusaurus.yml)

```bash
cd starsky-tools/build-tools/
npm run documentation-create-docusaurus-update
```

> [documentation-update-dependencies-create-docusaurus.yml](https://github.com/qdraw/starsky/actions/workflows/documentation-update-dependencies-create-docusaurus.yml)

## ClientApp React Linux CI

Build for the ClientApp on linux
Runs on pull request and push on the master branch

[![clientapp-react-linux-ci](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-linux-ci.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-linux-ci.yml)

```bash
cd starsky/starsky/clientapp
npm ci
npm npm run build
npm run test:ci
```

> [clientapp-react-linux-ci.yml](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-linux-ci.yml)

## ClientApp React Windows CI

Build for the ClientApp on Windows
Runs on pull request and push on the master branch

[![ClientApp React Windows CI](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-windows-ci.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-windows-ci.yml)

```bash
cd starsky/starsky/clientapp
npm ci
npm npm run build
npm run test:ci
```

> [clientapp-react-windows-ci.yml](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-windows-ci.yml)

## CodeQL analysis

Run CodeQL analysis on push and pull request

[![global-codeql-analysis](https://github.com/qdraw/starsky/actions/workflows/global-codeql-analysis.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/global-codeql-analysis.yml)


> [global-codeql-analysis.yml](https://github.com/qdraw/starsky/actions/workflows/global-codeql-analysis.yml)

## Documentation to github pages

Deploy docs site to github pages

[![documentation-gh-pages](https://github.com/qdraw/starsky/actions/workflows/documentation-gh-pages.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/documentation-gh-pages.yml)


> [documentation-gh-pages.yml](https://github.com/qdraw/starsky/actions/workflows/documentation-gh-pages.yml)

## end2end on ubuntu-ci

Cypress end to end testing on ubuntu ci
Runs a systemd service and a cypress test.
See [Cypress Dashboard](https://cloud.cypress.io/projects/1yeai3/runs) and click on the
tag: `ubuntu-ci` for more details.

[![end2end on ubuntu-ci](https://github.com/qdraw/starsky/actions/workflows/end2end-ubuntu-ci.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/end2end-ubuntu-ci.yml)

> [end2end-ubuntu-ci.yml](https://github.com/qdraw/starsky/actions/workflows/end2end-ubuntu-ci.yml)

## end2end on windows-ci

Cypress end to end testing on windows ci
Runs a windows service and a cypress test.
See [Cypress Dashboard](https://cloud.cypress.io/projects/1yeai3/runs) and click on the
tag: `windows-ci` for more details.

[![end2end on windows-ci](https://github.com/qdraw/starsky/actions/workflows/end2end-windows-ci.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/end2end-windows-ci.yml)

> [end2end-windows-ci.yml](https://github.com/qdraw/starsky/actions/workflows/end2end-windows-ci.yml)

## Create Release on tag for docker hub

On tag push create a release for docker hub
runs on release of a new stable version

[![webapp-docker-release-on-tag-docker-hub](https://github.com/qdraw/starsky/actions/workflows/webapp-docker-release-on-tag-docker-hub.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/webapp-docker-release-on-tag-docker-hub.yml)

> [webapp-docker-release-on-tag-docker-hub.yml](https://github.com/qdraw/starsky/actions/workflows/webapp-docker-release-on-tag-docker-hub.yml)

## Create Desktop Release on tag for .Net and Electron

Build the .NET runtime for Linux, Windows and Mac OS
And build Electron
Only create release when a new tag is pushed

[![desktop-release-on-tag-net-electron](https://github.com/qdraw/starsky/actions/workflows/desktop-release-on-tag-net-electron.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/desktop-release-on-tag-net-electron.yml)

> [desktop-release-on-tag-net-electron.yml](https://github.com/qdraw/starsky/actions/workflows/desktop-release-on-tag-net-electron.yml)

## Docker buildx multi-arch CI unstable master

Build docker images for testing

[![webapp-unstable-docker-buildx](https://github.com/qdraw/starsky/actions/workflows/webapp-unstable-docker-buildx.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/webapp-unstable-docker-buildx.yml)

> [webapp-unstable-docker-buildx.yml](https://github.com/qdraw/starsky/actions/workflows/starsky-docker-buildx.yml)

## Starsky .NET (Ubuntu)

CI build for .NET Core on Ubuntu
Builds and runs unit tests in international mode and locale NL_nl due dot and comma issues

[![webapp-build-net-ubuntu](https://github.com/qdraw/starsky/actions/workflows/webapp-build-net-ubuntu.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/webapp-build-net-ubuntu.yml)

> [webapp-build-net-ubuntu](https://github.com/qdraw/starsky/actions/workflows/webapp-build-net-ubuntu.yml)

## Starsky .NET Core (Windows)

CI build for .NET Core on Windows
Builds and runs unit tests

[![webapp-build-net-windows](https://github.com/qdraw/starsky/actions/workflows/webapp-build-net-windows.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/webapp-build-net-windows.yml)

> [webapp-build-net-windows.yml](https://github.com/qdraw/starsky/actions/workflows/webapp-build-net-windows.yml)

## Starsky SonarQube ClientApp NetCore Analyze PR

Analyze the code with SonarQube on Pull Request

[![webapp-sonarqube-clientapp-netcore](https://github.com/qdraw/starsky/actions/workflows/webapp-sonarqube-clientapp-netcore.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/webapp-sonarqube-clientapp-netcore.yml)

> [webapp-sonarqube-clientapp-netcore.yml](https://github.com/qdraw/starsky/actions/workflows/webapp-sonarqube-clientapp-netcore.yml)

## Starsky Tools Node smoke test

Smoke tests for the starsky-tools

- dropbox import
- localtunnel
- mail
- mock service
- thumbnail

[![tools-smoke-test-node](https://github.com/qdraw/starsky/actions/workflows/tools-smoke-test-node.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/tools-smoke-test-node.yml)

> [tools-smoke-test-node.yml](https://github.com/qdraw/starsky/actions/workflows/tools-smoke-test-node.yml)

## starskyDesktop Electron PR (Missing .NET dependency)

Build the Electron app on pull request without .NET so faster but does not run the app
For Windows and Mac OS builds the app and runs the unit tests

[![desktop-electron-pr-build-mac-win](https://github.com/qdraw/starsky/actions/workflows/desktop-electron-pr-build-mac-win.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/desktop-electron-pr-build-mac-win.yml)

> [desktop-electron-pr-build-mac-win.yml](https://github.com/qdraw/starsky/actions/workflows/desktop-electron-pr-build-mac-win.yml)

## storybook clientapp netlify

Deploy storybook of clientapp to netlify

[![clientapp-storybook-netlify](https://github.com/qdraw/starsky/actions/workflows/clientapp-storybook-netlify.yml/badge.svg)](https://github.com/qdraw/starsky/actions/workflows/clientapp-storybook-netlify.yml)

> [storybook-clientapp-netlify.yml](https://github.com/qdraw/starsky/actions/workflows/storybook-clientapp-netlify.yml)

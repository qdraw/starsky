---
sidebar_position: 2
---

# Github Actions CI

GitHub Actions makes it easy to automate all your software workflows, now with world-class CI/CD.
Build, test, and deploy your code right from GitHub.

[View all github actions on github](https://github.com/qdraw/starsky/tree/master/.github/workflows)

## Important pipelines

- [Build desktop app (Create Desktop Release on tag for .Net Core and Electron)](#create-desktop-release-on-tag-for-net-core-and-electron)
- [Docker Hub on new version (Create Release on tag for docker hub)](#create-release-on-tag-for-docker-hub)
- [Docker unstable build (Docker buildx multi-arch CI unstable master)](#docker-buildx-multi-arch-ci-unstable-master)

# All github actions used by this project

There are multiple github actions used by this project. Bellow is a list of all github actions and a short on alphabet order.

## Auto upgrade .NET SDK version
Run weekly
Automatically upgrade the .NET SDK version and upgrade nuget packages file when a new version is released.

```bash
cd starsky-tools/build-tools/
npm run nuget-package-list
```

> [auto-dotnet-sdk-version-upgrade.yml](https://github.com/qdraw/starsky/actions/workflows/auto-dotnet-sdk-version-upgrade.yml) 
 
## Application Version Auto update 
Run on push and manual trigger
Upgrade the application version in the csproj file and package.json file

```bash
cd starsky-tools/build-tools/
npm run app-version-update
```

> [auto-update-application-version.yml](https://github.com/qdraw/starsky/actions/workflows/auto-update-application-version.yml)

## Auto update Nuget packages list
On push on master branch
Creates a list of nuget packages and their version in the nuget-package-list file

```bash
cd starsky-tools/build-tools/
npm run nuget-package-list
```

> [auto-update-nuget-packages-list.yml](https://github.com/qdraw/starsky/actions/workflows/auto-update-nuget-packages-list.yml)

## Auto Update Swagger

Update json file with the latest swagger file

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

> [auto-update-swagger-dotnet.yml](https://github.com/qdraw/starsky/actions/workflows/auto-update-swagger-dotnet.yml)

## auto clientapp create react app upgrade
Bootstrap the clientapp with the latest create react app version

```bash
cd starsky-tools/build-tools/
npm run clientapp-create-react-app-update
```

> [auto-upgrade-clientapp-create-react-app.yml](https://github.com/qdraw/starsky/actions/workflows/auto-upgrade-clientapp-create-react-app.yml)

## Auto Documentation create Docusaurus upgrade
Bootstrap the docs with the latest Docusaurus version

```bash
cd starsky-tools/build-tools/
npm run documentation-create-docusaurus-update
```

> [auto-upgrade-documentation-create-docusaurus.yml](https://github.com/qdraw/starsky/actions/workflows/auto-upgrade-documentation-create-docusaurus.yml)

## ClientApp React Linux CI
Build for the ClientApp on linux
Runs on pull request and push on the master branch

```bash
cd .tarsky/starsky/clientapp
npm ci
npm npm run build
npm run test:ci
```

> [clientapp-react-linux-ci.yml](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-linux-ci.yml)

## ClientApp React Windows CI
Build for the ClientApp on Windows
Runs on pull request and push on the master branch

```bash
cd .tarsky/starsky/clientapp
npm ci
npm npm run build
npm run test:ci
```

> [clientapp-react-windows-ci.yml](https://github.com/qdraw/starsky/actions/workflows/clientapp-react-windows-ci.yml)

## CodeQL analysis
Run CodeQL analysis on push and pull request

> [codeql-analysis.yml](https://github.com/qdraw/starsky/actions/workflows/codeql-analysis.yml)

## Documentation to github pages
Deploy docs site to github pages

> [documentation-gh-pages.yml](https://github.com/qdraw/starsky/actions/workflows/documentation-gh-pages.yml)

## end2end on ubuntu-ci
Cypress end to end testing on ubuntu ci
Runs a systemd service and a cypress test

> [end2end-ubuntu-ci.yml](https://github.com/qdraw/starsky/actions/workflows/end2end-ubuntu-ci.yml)

## Create Release on tag for docker hub
On tag push create a release for docker hub
runs on release of a new stable version

> [release-on-tag-docker-hub.yml](https://github.com/qdraw/starsky/actions/workflows/release-on-tag-docker-hub.yml)

## Create Desktop Release on tag for .Net Core and Electron
Build the .NET runtime for Linux, Windows and Mac OS
And build Electron
Only create release when a new tag is pushed

> [release-on-tag-netcore-desktop-electron.yml](https://github.com/qdraw/starsky/actions/workflows/release-on-tag-netcore-desktop-electron.yml)

## Docker buildx multi-arch CI unstable master
Build docker images for testing

> [starsky-docker-buildx.yml](https://github.com/qdraw/starsky/actions/workflows/starsky-docker-buildx.yml)

## Starsky .NET Core (Ubuntu)
CI build for .NET Core on Ubuntu
Builds and runs unit tests in international mode and locale NL_nl due dot and comma issues

> [starsky-dotnetcore-ubuntu.yml](https://github.com/qdraw/starsky/actions/workflows/starsky-dotnetcore-ubuntu.yml)

## Starsky .NET Core (Windows)
CI build for .NET Core on Windows
Builds and runs unit tests

> [starsky-dotnetcore-windows.yml](https://github.com/qdraw/starsky/actions/workflows/starsky-dotnetcore-windows.yml)

## Starsky SonarQube ClientApp NetCore Analyze PR
Analyze the code with SonarQube on Pull Request
Runs only on pull requests

> [starsky-sonarqube-clientapp-netcore.yml](https://github.com/qdraw/starsky/actions/workflows/starsky-sonarqube-clientapp-netcore.yml)

## Starsky Tools Node smoke test
Smoke tests for the starsky-tools
- dropbox import
- localtunnel
- mail
- mock service
- thumbnail

> [starsky-tools-node-smoke-test.yml](https://github.com/qdraw/starsky/actions/workflows/starsky-tools-node-smoke-test.yml)

## starskyDesktop Electron PR (Missing .NET dependency)

Build the Electron app on pull request without .NET so faster but does not run the app
For Windows and Mac OS builds the app and runs the unit tests

> [starskyapp-electron-pr-build-mac-win.yml](https://github.com/qdraw/starsky/actions/workflows/starskyapp-electron-pr-build-mac-win.yml)

## storybook clientapp netlify

Deploy storybook of clientapp to netlify

> [storybook-clientapp-netlify.yml](https://github.com/qdraw/starsky/actions/workflows/storybook-clientapp-netlify.yml)

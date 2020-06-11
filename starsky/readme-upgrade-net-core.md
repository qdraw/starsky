

## When upgrading SDK

Build pipelines need to be updated

 - [x] `.github/workflows/starsky-dotnetcore-ubuntu.yml`
 - [x] `.github/workflows/starsky-dotnetcore-windows.yml `
 - [x] `.github/workflows/starsky-sonarqube-clientapp-netcore.yml`
 - [x] `azure-pipelines-starsky.yml`
 - [x] `azure-pipelines-starsky.starskyapp.yml`

Docs
 - [x] `starsky/starsky/readme.md`

Docker only needs to be updated when a major version is upgraded

## Update Runtime version
Check at least those files

 - [x] `starsky/starsky/starsky.csproj`
 - [x] `starsky/starskyadmincli/starskyadmincli.csproj`
 - [x] `starsky/starskygeocli/starskygeocli.csproj`
 - [x] `starsky/starskyimportercli/starskyimportercli.csproj`
 - [x] `starsky/starskysynccli/starskysynccli.csproj`
 - [x] `starsky/starskytest/starskytest.csproj`
 - [x] `starsky/starskywebftpcli/starskywebftpcli.csproj`
 - [x] `starsky/starskywebhtmlcli/starskywebhtmlcli.csproj`

## Legacy project
 - [x] `starsky.netframework/starskyImporterNetFrameworkCli/starskyImporterNetFrameworkCli.csproj`
 - [x] `starsky.netframework/starskySyncNetFrameworkCli/starskySyncNetFrameworkCli.csproj`

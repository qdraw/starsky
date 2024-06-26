

## When upgrading SDK

There is an build-tools script that does this automatically 

## Build pipelines need to be updated

 - [x] `.github/workflows/starsky-dotnetcore-ubuntu.yml`
 - [x] `.github/workflows/starsky-dotnetcore-windows.yml `
 - [x] `.github/workflows/starsky-sonarqube-clientapp-netcore.yml`
 - [x] `.github/workflows/starsky-codecov-clientapp-netcore.yml`
 - [x] `.github/workflows/release-on-tag-netcore-electron.yml`
 - [x] `pipelines/steps/use_dotnet_version.yml`

Docs in `starsky (sln)`
 - [x] `starsky/starsky/readme.md`

## Docker
Docker only needs to be updated when a major version is upgraded

## Update Runtime version
Check at least those files

 - [x] `starsky/starsky/starsky.csproj`
 - [x] `starsky/starskyadmincli/starskyadmincli.csproj`
 - [x] `starsky/starskygeocli/starskygeocli.csproj`
 - [x] `starsky/starskyimportercli/starskyimportercli.csproj`
 - [x] `starsky/starskysynchronizecli/starskysynchronizecli.csproj`
 - [x] `starsky/starskythumbnailcli/starskythumbnailcli.csproj`
 - [x] `starsky/starskytest/starskytest.csproj`
 - [x] `starsky/starskywebftpcli/starskywebftpcli.csproj`
 - [x] `starsky/starskywebhtmlcli/starskywebhtmlcli.csproj`

Might useful to force evaluate packages
```
dotnet restore --force-evaluate
```


## Go to starsky root directory
```
atom  starsky/starsky/starsky.csproj starsky/starskyadmincli/starskyadmincli.csproj starsky/starskygeocli/starskygeocli.csproj starsky/starskyimportercli/starskyimportercli.csproj starsky/starskytest/starskytest.csproj starsky/starskywebftpcli/starskywebftpcli.csproj starsky/starskywebhtmlcli/starskywebhtmlcli.csproj starsky.netframework/starskyImporterNetFrameworkCli/starskyImporterNetFrameworkCli.csproj starsky.netframework/starskySyncNetFrameworkCli/starskySyncNetFrameworkCli.csproj
```

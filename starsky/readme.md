# Starsky
## List of Starksy Projects
 - [inotify-settings](../inotify-settings/readme.md) _to setup auto indexing on linux [(files)](../inotify-settings)_
 - __[starsky (sln)](../starsky/readme.md) _database photo index & import index project [(files)](../starsky)___
   - [starsky](../starsky/starsky/readme.md)  _mvc application / web interface [(files)](../starsky/starsky)_
   - [starsky-cli](../starsky/starsky-cli/readme.md)  _database command line interface [(files)](../starsky/starsky-cli)_
   - [starskyimportercli](../starsky/starskyimportercli/readme.md)  _import command line interface [(files)](../starsky/starskyimporterclid)_
   - [starskyTests](../starsky/starskyTests/readme.md)  _mstest unit tests [(files)](../starsky/starskyTests)_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../starskyapp) _React-Native app (Pre-alpha code)_

## General Starsky (sln) docs


### Install instructions

(incomplete)

1. Download exiftool
2. Update the `appsettings.json` configuration before starting
> Windows: use double escape \\\\ in config directory paths


### Bash build and configuation scripts

Those scripts are optional and used for configuation.

### pm2 `new-pm2.sh`
The script [`new-pm2.sh`](new-pm2.sh) is a script to setup Starsky using [pm2](http://pm2.keymetrics.io/).
```sh
 export ASPNETCORE_URLS="http://localhost:4823/"
 export ASPNETCORE_ENVIRONMENT="Production"
 ```

 ### Publish for platform scripts

 The scripts that are used to create a full build. (Linux has `libunwind8` and `gettext` as dependency)
  - [`publish-linux-arm.sh`](publish-linux-arm.sh) Linux ARM (Raspberry Pi 2/3)
  - [`publish-mac.sh`](publish-mac.sh) OS X 10.12+
  - [`publish-windows.sh`](publish-windows.sh) Windows 7+

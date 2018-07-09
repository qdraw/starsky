# Starsky (sln)

 - [inotify-settings](../inotify-settings) _to setup auto indexing on linux_
 - [starsky (sln)](../starsky) _database photo index & import index project_
   - [starsky](../starsky/starsky)  _mvc application / web interface [(docs)](../starsky/starsky/readme.md)_
   - [starsky-cli](../starsky/starsky-cli)  _database command line interface [(docs)](../starsky/starsky-cli/readme.md)_
   - [starskyimportercli](../starsky/starskyimportercli)  _import command line interface [(docs)](../starsky/starskyimportercli/readme.md)_
   - [starskyTests](../starsky/starskyTests)  _mstest unit tests_
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../starskyapp) _React-Native app (Pre-alpha code)_

### Bash build and configuation scripts

Those scripts are optional and used for configuation.

#### pm2 `new-pm2.sh`
The script [`new-pm2.sh`](new-pm2.sh) is a script to setup Starsky using [pm2](http://pm2.keymetrics.io/).
```sh
 export ASPNETCORE_URLS="http://localhost:4823/"
 export ASPNETCORE_ENVIRONMENT="Production"
 ```

 #### Publish for platform scripts

 The scripts that are used to create a full build. (Linux has `libunwind8` and `gettext` as dependency)
  - [`publish-linux-arm.sh`](publish-linux-arm.sh) Linux ARM (Raspberry Pi 2/3)
  - [`publish-mac.sh`](publish-mac.sh) OS X 10.12+
  - [`publish-windows.sh`](publish-windows.sh) Windows 7+

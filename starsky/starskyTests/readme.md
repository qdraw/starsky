# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * __[starskyTests](../../starsky/starskyTests/readme.md)  mstest unit tests__
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to html files_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse geotagging_
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starsky/starskyTests docs

Starksy has unittests for the C# application.
Those unittest does not require any configuration or external dependencies like a webservice.
The main application has exiftool as external dependency, but you don't need this for the starskyTests.

Run the tests inside the `starsky/starskyTests` folder:
```sh
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```
or use the `starsky/starskyTest.sh` `starsky/starskyTest.bat` files to generate coverage documentation

> The unittests are creating a few files inside the mstest build directory. Those files will be removed afterwards.

>> All tests must run succesfull to build

### Location of temp files
#### Windows
```
C:\Users\VssAdministrator\.nuget\packages\microsoft.testplatform.testhost\15.9.0\lib\netstandard1.5\
```

#### OS X
```
~/.nuget/packages/microsoft.testplatform.testhost/15.9.0/lib/netstandard1.5/
```

### Coverlet.msbuild
The goal is improve test coverage

### Coverage Chart
```
+-------------------+--------+--------+--------+
| Module            | Line   | Branch | Method |
+-------------------+--------+--------+--------+
| starsky           | 82,4%  | 80%    | 91%    |
+-------------------+--------+--------+--------+
| starsky.Views     | 0%     | 0%     | 0%     |
+-------------------+--------+--------+--------+
| starskygeocli     | 74,1%  | 61,4%  | 90,9%  |
+-------------------+--------+--------+--------+
| starskysynccli    | 52%    | 50%    | 100%   |
+-------------------+--------+--------+--------+
| starskywebhtmlcli | 74%    | 74,1%  | 100%   |
+-------------------+--------+--------+--------+
Total Line: 76,5%
Total Branch: 57,2%
Total Method: 86,6%
```

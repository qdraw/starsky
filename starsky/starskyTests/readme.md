# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskycore](../../starsky/starskycore/readme.md) _business logic (netstandard2.0)_
    * [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * __[starskyTests](../../starsky/starskyTests/readme.md)  mstest unit tests__
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to html files_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starsky/starskyTests docs

To know that the application is working like expected, there are test create.  Starksy has unit tests for the C# application.
Those unit test does not require any configuration or external dependencies like a webservice.
The main application has Exiftool as external dependency, but you don't need this for the starskyTests.

Run the tests inside the `starsky/starskyTests` folder:
```sh
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```
or use the `starsky/starskyTest.sh` `starsky/starskyTest.bat` files to generate coverage documentation

> The unit tests are creating a few files inside the MSTest build directory. Those files will be removed afterwards.

>> All tests must run successful to build

### Location of temporary files

During the unit test there are temporary files created to test the functionality. When you have preserve against this, please don't run any test. Those temporary only exist in the following folders:

#### Windows
```
C:\Users\VssAdministrator\.nuget\packages\microsoft.testplatform.testhost\15.9.0\lib\netstandard1.5\
```

#### OS X
```
~/.nuget/packages/microsoft.testplatform.testhost/15.9.0/lib/netstandard1.5/
```

### Coverlet.MSBuild
To measure how much code is tested by this automatically script we have included this library.   The goal is improve test coverage

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

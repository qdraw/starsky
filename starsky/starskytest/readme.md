# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskySyncCli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskycore](../../starsky/starskycore/readme.md) _business logic (netstandard 2.0)_
    * __[starskyTest](../../starsky/starskytest/readme.md)  mstest unit tests__
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starsky/starskyTest docs

To know that the application is working like expected, there are test create.  Starksy has unit tests for the C# application.
Those unit test does not require any configuration or external dependencies like a webservice.
The main application has Exiftool as external dependency, but you don't need this for the starskyTests.

### With Cake

When running the build script, inside `starskytest\coverage.report`

When using powershell running only the 'Starsky Mvc application' and tests

```powershell
powershell -File build.ps1 -ScriptArgs '-Target="CI"'
```

or using bash. You need to have `mono` installed

```sh
./build.sh -Target="CI"
```

### Without Cake as build tool
Run the tests inside the `starsky/starskyTests` folder:
```sh
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```
or use the `starsky/starskyTest.sh` `starsky/starskyTest.bat` files to generate coverage files


>  NOTE: The unit tests are creating a few files inside the MSTest build directory. Those files will be removed afterwards.

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
| starsky           | 64,5%  | 61,5%  | 68,7%  |
+-------------------+--------+--------+--------+
| starsky.Views     | 0%     | 0%     | 0%     |
+-------------------+--------+--------+--------+
| starskycore       | 92,8%  | 85,6%  | 96,1%  |
+-------------------+--------+--------+--------+
| starskygeocli     | 71,6%  | 59,7%  | 90,9%  |
+-------------------+--------+--------+--------+
| starskysynccli    | 49,1%  | 50%    | 100%   |
+-------------------+--------+--------+--------+
| starskywebhtmlcli | 73,6%  | 74,1%  | 100%   |
+-------------------+--------+--------+--------+

Total Line: 81,9%
Total Branch: 60,5%
Total Method: 89,1%
```

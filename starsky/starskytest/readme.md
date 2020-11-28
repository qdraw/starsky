# Starsky Test
## List of [Starsky](../../readme.md) Projects
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * __[starskyTest](../../starsky/starskytest/readme.md)  mstest unit tests__
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines (deprecated)_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _Desktop Application (Pre-alpha code)_

## starsky/starskyTest docs

To know that the application is working like expected, there are test create.  Starksy has unit tests for the C# application.
Those unit test does not require any configuration or external dependencies like a webservice.
The main application has Exiftool as external dependency that is installed automatically, but you don't need this for the starskyTest.

### With Cake

When running the build script, inside `starskytest\coverage.report`

When using powershell running only the 'Starsky Mvc application' and tests

```powershell
powershell -File build.ps1 -ScriptArgs '-Target="BuildTestNetCore"'
```

or using bash.

```sh
./build.sh -Target="BuildTestNetCore"
```

>  NOTE: The unit tests are creating a few files inside the MSTest build directory. Those files will be removed afterwards.

>> All tests must run successful to build

### Location of temporary files

During the unit test there are temporary files created to test the functionality. When you have preserve against this, please don't run any test. Those temporary only exist in the following folders:

#### Windows
```
C:\Users\VssAdministrator\.nuget\packages\microsoft.testplatform.testhost\16.2.0\lib\netstandard1.5\
```

#### OS X
```
~/.nuget/packages/microsoft.testplatform.testhost/16.2.0/lib/netstandard1.5/
```

### Coverlet.MSBuild
To measure how much code is tested by this automatically script we have included this library. 


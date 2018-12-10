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
> The unittests are creating a few files inside the mstest build directory. Those files will be removed afterwards.

>> All tests must run succesfull to build

### Location of temp files
#### Windows
```
C:\Users\VssAdministrator\.nuget\packages\microsoft.testplatform.testhost\15.7.2\lib\netstandard1.5\
```

#### OS X
```
~/.nuget/packages/microsoft.testplatform.testhost/15.7.2/lib/netstandard1.5/
```

### Coverlet.msbuild
The goal is improve test coverage

### Coverage Chart
```
+----------------+--------+--------+--------+
| Module         | Line   | Branch | Method |
+----------------+--------+--------+--------+
| starsky        | 80,2%  | 79,6%  | 92,4%  |
+----------------+--------+--------+--------+
| starskysynccli | 69,7%  | 56,2%  | 100%   |
+----------------+--------+--------+--------+
```

dotnet tool install -g dotnet-reportgenerator-globaltool



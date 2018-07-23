# Starsky
## List of Starksy Projects
 - [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux [(files)](../../inotify-settings/)_
 - [starsky (sln)](../../starsky/readme.md) _database photo index & import index project [(files)](../../starsky/)_
   - [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface [(files)](../../starsky/starsky/)_
   - [starsky-cli](../../starsky/starsky-cli/readme.md)  _database command line interface [(files)](../../starsky/starsky-cli/)_
   - [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface [(files)](../../starsky/starskyimportercli/)
   - __[starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests [(files)](../../starsky/starskyTests)___
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

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
#### OS X
```
~/.nuget/packages/microsoft.testplatform.testhost/15.7.2/lib/netstandard1.5/
~/.nuget/packages/microsoft.testplatform.testhost/15.7.2/lib/netstandard1.5/exist/
```

### Coverlet.msbuild
The goal is improve test coverage
```
+------------+--------+--------+--------+
| Module     | Line   | Branch | Method |
+------------+--------+--------+--------+
| starsky    | 77,3%  | 76,3%  | 91,3%  |
+------------+--------+--------+--------+
| starskycli | 68,2%  | 64,3%  | 50%    |
+------------+--------+--------+--------+
```

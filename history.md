# History Changelog
## List of __[Starsky](readme.md)__ Projects
 * [starsky (sln)](starsky/readme.md) _database photo index & import index project_
    * [starsky](starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky-tools](starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](starskyapp/readme.md) _Desktop Application_
     * [Download Desktop App](https://qdraw.github.io/starsky/assets/download/download.html) _Windows and Mac OS version_
 * __[Changelog](history.md) Release notes and history__

## Release notes of Starsky

Semantic Versioning 2.0.0 is from version 0.1.6+

## The following statuses are used
- Added _for new features_
- Breaking change _fix or feature that would cause existing functionality to change_
- Changed _for non-breaking changes in existing functionality for example docs change / refactoring / dependency upgrades_
- Deprecated _for soon-to-be removed features_
- Removed _for now removed features_
- Fixed _for any bug fixes_
- Security _in case of vulnerabilities_

## Update app version in child projects
To update all child projects to have the same version run the following script
```
node starsky-tools/build-tools/app-version-update.js
```

# Expected breaking changes in a future release:

- package telementry
- change mirror locations

# version 0.5.0-beta.2 _(Unreleased)_ - 2022-01-??
- [x]   (Changed) _Back-end_ Add correct connect-src url for websocket without port (PR #606)

# version 0.5.0-beta.1 - 2022-01-11
- [x]   (Changed) _Back-end_  Add request tracking for FSW SyncWatcherConnector (PR #589)
- [x]   (Changed) _Tools_ Add insider script for download Github artifacts (PR #589)
- [x]   (Changed) _App_ Middle mouse click in Electron app shows login page instead of content (PR #596 Issue #592 and PR #600)
- [x]   (Changed) _Front-end_ Add Trash title instead of !delete! (PR #597)
- [x]   (Changed) _Front-end_ Add Search query title instead (PR #597)
- [x]   (Changed) _Front-end_ Order by ImageFormat and then by filename (PR #598)
- [x]   (Breaking change) _Back-end_ change default option in Thumbnail-er cli to scan directories to enabled (-t true default) (PR #601) 
- [x]   (Added) _Back-end_ env variable to create swagger export and exit (PR #601)

# version 0.5.0-beta.0 - 2021-12-29
- [x]   (Breaking change) _CLI_ Removed sync cli (starskysynccli) which is replaced by starskysynchronizecli (PR #563)
- [x]   (Removed) _CLI_ Removed Net framework version which is replaced by .NET Core (PR #563)
- [x]   (Breaking change) _Back-end_ Removed obsolete SubPathSlashRemove API (PR #563)
- [x]   (Breaking change) _Back-end_ Removed old sync API  (PR #563)
- [x]   (Security) _Front-end_  Upgrade Prettier 2.5.1 and React scripts 5.0.0 (PR #569)
- [x]   (Breaking change) _Front-end_  Prettier new eslint rules 4.0.0 (PR #569)
- [x]   (Fixed) _Back-end_  IndexController with empty string introduced with removal of SubPathSlashRemove (PR #571)
- [x]   (Changed) _Back-end_ Upgrade Electron to 16.x and Electron Builder to 22.14.x (PR #571)
- [x]   (Breaking change) _Back-end_ rename "/api/sync/mkdir" to /api/disk/mkdir (PR #574)
- [x]   (Breaking change) _Back-end_ rename "/api/sync/rename" to /api/disk/rename (PR #574)
- [x]   (Changed) _Tools_ Upgrade local build tools Cake and dotnet-reportgenerator-globaltool and dotnet-sonarscanner (PR #575) 
- [x]   (Changed) _Back-end_  Avoid Disposed Query objects in syncWatcherConnector (PR #575)
- [x]   (Changed) _Back-end_  change FileSystemWatcher to BufferingFileSystemWatcher (PR #575)
- [x]   (Changed) _Back-end_  Add filter for FileSystemWatcher spamming with lots of events (PR #575)
- [x]   (Added) _Back-end_ Feature toggle to disable login for localhost requests (PR #579)
- [x]   (Added) _Back-end_ Check if account exists middleware _UseCheckIfAccountExist_  (PR #579)
- [x]   (Added) _Front-end_ Setup with wrong database connection give now explanation (PR #581)
- [x]   (App) _Back-end_  UI update with storage folder is reverted after restart (PR #584 Issue #582)
- [x]   (Fixed) _Back-end_  NoAccountLocalhostMiddleware with no database has no roles error (PR #483)
- [x]   (Fixed) _Back-end_ Add ImageStabilisation and database migration (only add new field)  (PR #483)
- [x]   (Fixed) _Back-end_ Fixed issue with Sony Lens Tamron lenses are diplaying dashes (PR #483)
- [x]   (App) _Back-end_  UI update with storage folder is reverted after restart (PR #584 Issue #582)
- [x]   (Breaking change) _Back_-end_ Droped support for older Mac OS version: now 10.15+ is required see: https://github.com/dotnet/core/blob/main/release-notes/3.1/3.1-supported-os.md
- [x]   (Added) _Back-end_ Application Insights track events in DiskwatcherQueue (PR #583)
- [x]   (Changed) _App_ Missing content length (PR #587)
- [x]   (Changed) _App_ Use generic OSX id (PR #588)

# version 0.4.13 - 2021-12-15
- [x]   (Added) _CLI_  Add csv option for import CLI (PR #510)
- [x]   (Fixed) _Tools_ Dotnet SDK updater build tools (Work in progress) (PR #510)
- [x]   (Fixed) _Back-end_  Fix for type LockoutEnd (PR #510)
- [x]   (Changed) _Back-end_ Add migration for MakeModel in ImportIndex (PR #510)
- [x]   (Fixed) _Tools_ Add push to gpx loader for mail  (PR #510)
- [x]   (Added) _Back-end_ Add feature (appSettings) `ApplicationInsightsDatabaseTracking` - Track database dependencies  (PR #528)
- [x]   (Added) _Back-end_ Add feature toggle (appSettings) `ApplicationInsightsLog` - Add WebLogger output to Application Insights (PR #528)
- [x]   (Added) _Back-end_ Filter for DiskWatcher sync to prevent database overload (PR #529)
- [x]   (Changed) _Back-end_ Upgrade dependencies for SixLabors.ImageSharp, ApplicationInsights, System.Threading.Tasks.Dataflow, Swashbuckle.AspNetCore and MSTest (PR #533)
- [x]   (Changed) _Back-end_ Upgrade SDK Version to 3.1.415 & Runtime version to 3.1.21 (PR #535)
- [x]   (Changed) _Back-end_ Rewrite thumbnail cleaner with chunks (CleanAllUnusedFilesAsync) (PR #531)
- [x]   (Changed) _Back-end_ Auto download Exiftool from mirror when main source is not up (PR #531)
- [x]   (Changed) _Back-end_ Change default LogLevel settings in appsettings.json (PR #531)
- [x]   (Changed) _Back-end_ Handle exceptions for HttpClientHelper to not interupt (PR #531)
- [x]   (Changed) _Back-end_ Change DiskWatcher background queue system (PR #536) 
- [x]   (Changed) _Back-end_ Add disk telemetry-channels when app is crashed (PR #536)
- [x]   (Changed) _Back-end_ Flush Application Insights on ApplicationStopping (PR #536)
- [x]   (Changed) _Back-end_ Move GeoBackgroundTask to GeoLookUp feature (PR #540)
- [x]   (Changed) _Back-end_ Add 10 seconds cache to UpdateAsync for performance reasons / diskWatcher catch up SetGetObjectByFilePathCache (PR #540)
- [x]   (Changed) _Back-end_ Remove NewtonSoftJson from PublishManifest and use System.Text.Json (PR #540)
- [x]   (Changed) _Back-end_ Remove NewtonSoftJson from various models (PR #540)
- [x]   (Changed) _Back-end_ Remove sync CleanAllUnusedFiles (PR #540)
- [x]   (Added) _Back-end_ Add event-counters to application insights https://docs.microsoft.com/en-us/azure/azure-monitor/app/eventcounters
- [x]   (Added) _Back-end_ setAuthenticatedUserContext for Application Insights (PR #540)
- [x]   (Added) _Back-end_ OperationId in RequestTelemetryHelper to track background Tasks in Application Insights (PR #540)
- [x]   (Added) _Back-end_ QueueBackgroundWorkItem for DiskWatcher to have a separate queue (PR #540)
- [x]   (Changed) _Back-end_ Change GeoLocationWrite to async variant  (PR #540)
- [x]   (Security) _Back-end_ Upgrade dependencies (PR #548 & #547 & #546 & #545 & #544 & #543 & #542 & #541)
- [x]   (Fixed) _Back-end_ Extend ServiceCollectionExtensions to load more Assemblies to get auto mapped by the service attribute (PR #550)
- [x]   (Added) _Back-end_ Add Application Insights logging for CLI Applications (Admin, Geo, Import, Synchronize, Thumbnail) (PR #552)
- [x]   (Changed) _Back-end_ Add dispose on parallel jobs  (PR #552)
- [x]   (Added) _Back-end_  Add index for IX_ImportIndex_FileHash and IX_Credentials_Id_Identifier (PR #555)
- [x]   (Added) _Back-end_ Fix for cache ManualSync when item is removed or added its now correct updated (PR #555)
- [x]   (Fixed) _Front-end_ Fix for Safari 14.x and newer that after close a modal, the scroll isn't locked anymore (PR #555) 
- [x]   (Fixed) _Back-end_ Add port to websocket url in connect-src for Safari 14.x using different port numbers  (PR #555)
- [x]   (Fixed) _Back-end_ Exception fix SQLite System.InvalidOperationException: ExecuteReader (PR #556)
- [x]   (Fixed) _Back-end_ Fix FlushApplicationInsights.FlushAsync System.NullReferenceException (PR #557)
- [x]   (Changed) _Back-end_ Upgrade SDK Version to 3.1.416 & Runtime version to 3.1.22 (PR #558)

# version 0.4.12 - 2021-11-04
- [x]   (Changed) _Back-end_ Your account is locked for an hour when you enter 3 non valid passwords (PR #443 & #445 & #446)
- [x]   (Changed) _Back-end_ Database migration for AccessFailedCount, LockoutEnabled and LockoutEnd in Users table (PR #443 & #445)
- [x]   (Fixed) _Back-end_ Group parts of the regex together to make the intended operator precedence explicit for getting Filename in clientApp (PR #444)
- [x]   (Breaking change) _Tools_ Dropbox has changed the way it authorized (September 30th, 2021), in dropbox-tools the refresh token is now used (PR #448)
- [x]   (Fixed) _Back-end_  Dispose error on index page (Issue #424 / PR #449)
- [x]   (Added) _Back-end_  Support for `-software` search (issue #441 / PR #450)
- [x]   (Added) _Back-end_  Docker compose support (PR #469)
- [x]   (Change) _Front-end_ Remove Enzeme as framework for unittests and use react testing framework (PR #463)
- [x]   (Change) _Back-end_ Retry for DiskWatcher (PR #479)
- [x]   (Change) _Back-end_ Max amount of retry for DiskWatcher when folders are not accessible (chown) (PR #490)
- [x]   (Changed) _Back-end_ DiskWatcher in combination with child folders that have no access keeps a known issue 
- [x]   (Fixed) _Back-end_ Skip folders with meta thumbnail tool when folder has no read rights (PR #490) 
- [x]   (Added) _Back-end_  Filter for import (ImportIgnore) (PR #490)
- [x]   (Added) _Back-end_ Add to docker hub and multi-arch build with buildx
- [x]   (Added) _Back-end_ use docker hub to pull images: `docker pull qdraw/starsky`

# version 0.4.11 - 2021-09-17
- [x]   (Security) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.17 (using SDK 3.1.411) (PR #428)
- [x]   (Fixed) _Back-end_ Make StorageFolder setting more clear (PR #429)
- [x]   (Fixed) _Back-end_ Change how appSettings are read to merge patch files (PR #429) 
- [x]   (Security) _Front-end_ Upgrade npm packages for clientapp/starskyapp (PR #431)
- [x]   (Changed) _Back-end_ Config order is documented in verbose help (add: -h -v to cli apps)  (PR #432)
- [x]   (Changed) _Back-end_ When new, make more clear how to setup storageFolder path (PR #429)
- [x]   (Fixed) _Back-end_ When download.geonames.org is down pick mirror (PR #434)
- [x]   (Added) _Back-end_ more logging for exiftool downloader (PR #434)
- [x]   (Added) _Back-end_ Fix websocket exception issues (PR #442 Issue #440 #436)

# version 0.4.10 - 2021-07-15
- [x]   (Fixed) _Back-end_ Performance change, FileIndexItem uses less memory in the application (PR #410)
- [x]   (Fixed) _Back-end_ Change Replace to use a single database query and update to empty string (PR #412)
- [x]   (Fixed) _Back-end_ Fix fast updating items to update the cache before update (PR #412)
- [x]   (Change) _Back-end_ Push direct to socket when update or replace to avoid undo after a second (PR #412)
- [x]   (Change) _Back-end_ Add 2000 px and 300 px size for thumbnails to match better with larger and smaller screens (PR #380)
- [x]   (Change) _Back-end_ Add Meta Thumbnail (150px) to get faster archive and search pages (PR #380)
- [x]   (Change) _Back-end_ Use 2000 px image in overlay (Publish) when available  (PR #380)
- [x]   (Change) _Back-end_ Start /api/thumbnail-generation in different thread instead of que (PR #380)
- [x]   (Added) _Back-end_ Add API "/api/thumbnail/list-sizes/{HashHere}" to check if the multiple sizes are there (PR #414)
- [x]   (Change) _Back-end_ Update thumbnail starsky-tools to use list-sizes API (PR #414)
- [x]   (Change) _Back-end_ Add support for multiple sizes for the Thumbnail cleaner (PR #419)
- [x]   (Fixed) _Back-end_ Set fallback image of ToBase64DataUriList when generation failed, instead of exception (PR #418)
- [x]   (Fixed) _Back-end_ Publish retry when output is corrupt (PR #418)
- [x]   (Fixed) _Front-end_  Fix check when no results are returned in publish modal (hotfix)
- [x]   (Fixed) _Tools_ Change image resampler in starsky-tools to bicubic for sharper output (hotfix 20210703)
- [x]   (Fixed) _Back-end_ Change image resampler in .NET to Lanczos3 for sharper output (hotfix 20210703)
- [x]   (Changed) _Front-end_ Change default option to load smaller images (isSingleItem to alwaysLoadImage) (PR #420)
- [x]   (Changed) _Front-end_ Upgrade npm packages in clientapp (PR #421)
- [x]   (Changed) _Tools_ Upgrade npm packages in starsky-tools/thumbnail (PR #422)
- [x]   (Added)   _App_ Add logging to app and write it to disk (PR #423)
- [x]   (Fixed) _Back-end_ Fix issue where sync with empty database adds a folder with only / to view (PR #423)
- [x]   (Changed)  _Back-end_ Enable useDiskWatcher by default, so file changes are picked up directly (PR #423)
- [x]   (Added)  _App_ Logs are stored by default in AppData or Application Support or .config folder (PR #423)

# version 0.4.9 - 2021-06-17
- [x]   (Fixed) _Front-end_ Show error when update fails in archive list (PR #391)
- [x]   (Fixed) _Front-end_ Fix for keeps loading forever if use fileList (archive) fails  (issue #382 & PR #392)
- [x]   (Fixed) _Front-end_  Add retry/reload button in Application Exception page  (PR #392)
- [x]   (Fixed) _Back-end_  Refactor Update validation to perform faster _(using a single query)_ (PR #394)
- [x]   (Fixed) _Back-end_ Rename thumbnail before exifTool writes the file (PR #396)
- [x]   (Fixed) _Back-end_ Issue where filePath is marked as change (PR #401)
- [x]   (Fixed) _Back-end_ SolveConcurrencyException also fixed for disposed objects (PR #401)
- [x]   (Fixed) _Back-end_ Skip socket push when item is not changed (Issue #399 & PR #402)
- [x]   (Fixed) _Back-end_ When updating the tags fast, the tool isn't keep track _(now only works when cache is enabled)_ (PR #402)

# version 0.4.8 - 2021-05-07
- [x]   (Changed) _Front-end_ Make Archive UI more white & Dark mode UI fixes (PR #358)
- [x]   (Added) _Front-end_  In select mode press delete key should move to trash in archive & search view (PR #360 / Issue #357)
- [x]   (Fixed)  _Back-end_ Remove check if byte size is the same for example when updating colorClass is the same (PR #362)
- [x]   (Fixed)  _Back-end_ Change Byte size to datetime last-edited (PR #362)
- [x]   (Changed)  _Back-end_ Need to re-sync thumbnails due changed fileHash (PR #362 & PR #361)
- [x]   (Changed)  _Back-end_ When run sync v2, only check on last edit time instead of filesize (PR #362 & PR #361)
- [x]   (Added) _Back-end_ Added logs to Application Insights when key is configured (PR #363)
- [x]   (Added) _Back-end_ Add manual sync for new api (/api/synchronize) and update via websockets (PR #363)
- [x]   (Added) _Front-end_ Change manual sync to new api (/api/synchronize) (PR #363)
- [x]   (Fixed) _Front-end_ Force sync is not endless loading after socket update (Issue #371 & PR #375)
- [x]   (Fixed) _Back-end_ Publish name with underscore breaks publish api (Issue #369 & PR #376)

# version 0.4.7 - 2021-04-11
- [x]   (Changed) _Back-end_  add cache for health check and timeout for 10 seconds on health calls (PR #332)
- [x]   (Fixed) _Front-end_ Zoom function for mobile (DetailView) (PR #327/ Issue #317)
- [x]   (Added) _Front-end_ Keyboard shortcuts Cmd + = and Cmd + - (DetailView) (PR #327/ Issue #317)
- [x]   (Fixed)  _Back-end_  Sending empty string on "/api/publish/exist" should return true (PR #334)
- [x]   (Added)  _Front-end_  Add publish option to Search and DetailView (PR #335/ Issue #298)
- [x]   (Security) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.13 (using SDK 3.1.407) (PR #338 & #337)
- [x]   (Changed) _Tools_ make it easier to deploy starsky with the new pm2 instances bash script to auto download releases (PR #341)
- [x]   (Fixed) _Front-end_ Publish selection click in modal closes and opens more menu in the right corner of the screen (PR #339/ Issue #336) 
- [x]   (Fixed) _Back-end_ Performance improvement by hash size limit (pull/348 issue/345)
- [x]   (Fixed) _Back-end_ Fix for SQLite exception (pull/348 issue/344)
- [x]   (Fixed) _Tools_ Make install scripts easier for the server in combination with PM2 (pull/348)
- [x]   (Fixed) _Back-end_ DbUpdateConcurrencyException save after, instead of nothing (pull #349)
- [x]   (Fixed) _Back-end_ fix issue where rename did not work in combination with useDiskWatcher (issue #347 pull #349)
- [x]   (Fixed) _Front-end_ input field when using safari you should not break words (pull #359)
- [x]   (Changed) _Back-end_ Geo CLI downloads now on startup dependencies (Issue #340 / pull #351)
- [x]   (Fixed)  _Back-end_ Rename service should now work with useDiskWatcher:true (Issue #352 / pull #354 & #355)
- [x]   (Fixed)  _Back-end_ Delete floating folders in database on scan synchronize (pull #354)

# version 0.4.6 - 2021-03-21
- [x]   (Added) _Front-end_  add prefilled selected option for
- [x]   (Added) _Front-end_  add sort option for fileName and ImageFormat but only on archive pages
- [x]   (Fixed) _Front-end_ Switch the text of the show raw button in Display options
- [x]   (Fixed) _Front-end_ When last file with colorClass is removed, the display is now correct (PR #313)
- [x]   (Fixed) _Front-end_ Implicit delete by updating from sockets should now not cover collection items with the same base name (PR #313)
- [x]   (Fixed) _Back-end_  When use DeleteItem the child directories are not stored in the database (PR #314)
- [x]   (Fixed) _Back-end_ Give removed items back when using `/api/rename` as status NotFoundSourceMissing (PR #314)
- [x]   (Fixed) _Back-end_ Don't create duplicate database items when there is already a database item in the output folder, but not on disk (PR #314)
- [x]   (Security) _Frond-end_  Upgrade ClientApp CRA _(Create React App 4.0.3 2021-02-22)_ (PR #318)
- [x]   (Security) _App_  Upgrade Electron to 12.x (PR #318)
- [x]   (Fixed) _App_ Fix local remote toggle in settings not switching the file watcher (PR #318)
- [x]   (Fixed) _Back-end_ DbUpdateConcurrencyException when renaming (PR #320 / Issue #312)
- [x]   (Fixed) _Front-end_ Connection error gives now ServerError instead of failing silence (PR #323 / Issue #322 )
- [x]   (Fixed) _Front-end_  Flat list behind query parameter `?list=true`, on archive pages, there is no UI option yet  (PR #302 Issue #251)
- [x]   (Fixed) _Front-end_  Detailview command click on close keeps loading (PR #324 Issue #316)  
- [x]   (Changed) _Front-end_  Rename of 'Close' to 'Parent Folder' because its looks like closing a window and it is't the same (PR #324)  

# version 0.4.5 - 2021-02-14
- [x]   (Fixed) _Back-end_ When remove a folder, the files within the folder are still in the database bug _issue #188_
- [x]   (Fixed) _Front-end_ Displaying files in realtime works now _issue #275_
- [x]   (Fixed) _Front-end_ Archive when added tag/description is cleared is still send _issue #279_
- [x]   (Changed) _Back-end_ "/api/thumbnail/{f}" status 409 is changed to status 210
- [x]   (Added) _Front-end_  Zoom in detailView _issue #242_
- [x]   (Fixed) _Back-end_  Delete large number of files gives exception _issue #281_
- [x]   (Added) _Front-end_  Keyboard accelerator Command / Ctrl A _issue #247_
- [x]   (Added) _Back-end_  add logger (Microsoft.Extensions.Logging.Abstractions)
- [x]   (Added) _Back-end_  RetryHelper for IOExceptions using Windows OS
- [x]   (Changed) _Back-end_ Change default logging settings in appsettings.json to have less Information
- [x]   (Changed) _Back-end_ Disable TieredCompilationQuickJit and TieredCompilation are disabled for optimization
- [x]   (Changed) _Back-end_ performance update for metaPreflight, reduced a cache call. Helpful for large folders
- [x]   (Security) _Frond-end_  Upgrade ClientApp CRA _(Create React App 4.0.2 2021-02-03)_
- [x]   (Security) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.12 (using SDK 3.1.112)

# version 0.4.4 - 2021-01-10
- [x]   (Security) _App_ npm audit fix node_modules/ini 1.3.8
- [x]   (Fixed) _Front-end_ When updating tags in sidebar and refresh afterwards its now not the old value anymore
- [x]   (Fixed) _App_ Starting from another user in Mac OS should work now
- [x]   (Fixed) _Front-end_ Label copy (press c and v) does not save with titles - issue #248
- [x]   (Fixed) _Front-end_ update websocket data for other items outside view when receiving data bug - issue #265
- [x]   (Fixed) _App_ when changing from local to remote it should open new window - issue #271
- [x]   (Fixed) _App_ when changing from local to remote it should add watcher be updated - issue #271
- [x]   (Fixed) _Front-end_ hide 'sign in instead' button on register page when there are no users yet
  
# version 0.4.3 - 2020-12-24
- [x]   (Fixed) _Back-end_ the latest version isn't checked right, it takes the oldest version to compare with
- [x]   (Changed) _App_ Rewrite of desktop application
- [x]   (Changed) _App_ Desktop settings  app-settings is changed to "starksy-app-settings.json"
- [x]   (Changed) _Back-end_ Allow version parameter for "/api/health/version"
- [x]   (Fixed) _Front-end_  Use real-time update Color class outside selection #252
- [x]   (Added) _App_ Add Dutch translation to menu's
- [x]   (Fixed) _Back-end_ When saving StorageFolder from Preferences its now saved in the right format
- [x]   (Fixed) _Back-end_ Files that are not in the index should not be listed in the cache
- [x]   (Security) _App_ node-notifier from 8.0.0 to 8.0.1 #258
- [x]   (Fixed) _Back-end_ Add check for duplicate folders in the database in synchronize
- [x]   (Fixed) _Back-end_ Handling errors for ConcurrencyException when saving #175
- [x]   (Fixed) _Back-end_ Handling errors on: DbUpdateConcurrencyException on RemoveItem #261

# version 0.4.2 - 2020-12-09
- [x]   (Changed) _Docs_ Update docs and remove old projects from docs
- [x]   (Security) _Frond-end_  Upgrade ClientApp CRA _(Create React App 4.0.1 2020-11-23)_
- [x]   (Security) _Frond-end_  Upgrade ClientApp Typescript version to 4.1.2
- [x]   (Security) _Frond-end_  Upgrade ClientApp React version to 17.0.1
- [x]   (Added) _Front-end_ Add warning when Application fails for trash and search
- [x]   (Added) _Front-end_ Add menu text & Rename Collection mode to Show raw files
- [x]   (Added) _Back-end_ When making new directory this broadcast it correctly using sockets
- [x]   (Added) _Front-end_ Enable touch swipe right and left on detailView pages to go next/prev
- [x]   (Fixed) _Back-end_ Import is now not adding duplicate content if UseDiskWatcher is faster to add items
- [x]   (Fixed) _Back-end_ Add filter (AppSettings.SyncIgnore) for sync (starsky.foundation.sync) #73
- [x]   (Added) _Back-end_ Update Sidecar field when running sync
- [x]   (Added) _Front-end_ Socket notification close causes app crash
- [x]   (Added) _Front-end_ Swipe image set loading state forever

# version 0.4.1 - 2020-11-27
- [x]   (Fixed) _Back-end_ Extra security headers for browsers
- [x]   (Added) _Back-end_ Change fileHash behavior to have more timeout time
- [x]   (Added) _Back-end_ add round for focalLength
- [x]   (Added) _Back-end_ Realtime Files API (issue #75) behind _useDiskWatcher_ feature toggle
- [x]   (Added) _Back-end_ New Sync service 'starsky.foundation.sync' behind new API
- [x]   (Added) _Back-end_ Split Sync in starskysynchronizecli and starskythumbnailcli
- [x]   (Deprecated) _Back-end_ Old Sync CLI, replaced by starskysynchronizecli (to be removed in future release)
- [x]   (Added) _Back-end_ Notify realtime websockets when DiskWatcher detects changes
- [x]   (Added) _Back-end_ Notify other users when a file or folder is moved #212
- [x]   (Changed) _Back-end_ Importer does update the database when file copy happens #104
- [x]   (Fixed) _Back-end_ Item exist but not in folder cache, it now add this item to cache #228
- [x]   (Added) _Back-end_ Check if Exiftool exist before running the import CLI

# version 0.4.0 - 2020-11-14
_Please check the breaking changes of 0.4.0-beta.0 and 0.4.0-beta.1_
- [x]   (Changed) _App_ Add styling to settings UI in App
- [x]   (Fixed) _Back-end_  Add extra catch to prevent sync issues when exif reading fails
- [x]   (Deprecated) _Back-end_ Json Sidecar format is very likely to change in future releases and be incompatible
- [x]   (Added) _App_ Add extra delay to check for updates to avoid issues when local
- [x]   (Added) _App_ Add fix for selecting wrong domains to avoid an exception
- [x]   (Fixed) _Back-end_ When switching very fast after update, info isn't updated until process is done (this is fixed)
- [x]   (Security) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.9 (using SDK 3.1.403)
- [x]   (Fixed) _Front-end_ Clean Front-end cache when moving file/renaming file
- [x]   (Fixed) _Front-end_ Change text when selecting an non existing filter combination
- [x]   (Fixed) _Back-end_ Fix for dispose Errors in Query
- [x]   (Fixed) _Back-end_ Allow upload to folder with files that are uppercase
- [x]   (Fixed) _Back-end_ Database-item is now correct updated when you move an item to the root folder (/)
- [x]   (Security) _App_ Update Electron to 10.1.5 (Node 12.16.x and Chromium 85.0.x)
- [x]   (Added) _Back-end_ In the rename/move API When enable Collections, this files are also moved (file to folder)
- [x]   (Added) _Back-end_ Xmp sidecar files are moved with gif/bmp/Raw/mp4 file types
- [x]   (Added) _Back-end_ In the rename API When enable Collections, this files are also moved (file to deleted)
- [x]   (Deprecated) _App_ The current app-settings (so only the default app/remote location) are going to change.
                            if you update those could be gone. but you could set them again

# version 0.4.0-beta.2 - 2020-11-04
- [x]   (Changed) _Front-end_ Enable sockets client side option by default
- [x]   (Changed) _Back-end_ UseRealtime (sockets) backend option changed to enable by default
- [x]   (Fixed) _Front-end_ When updating files with realtime mode on, collection mode raws are shown after update
- [x]   (Added) _Back-end_ API to check if current version is the latest on github releases
- [x]   (Added) _Front-end_ Clientside check for latest version (click away for 4 days)
- [x]   (Added) _App_ Check for latest version and click away for 4 days

# version 0.4.0-beta.1 - 2020-10-31
_First release on Github Releases_
- [x]   (Added) _App_ Press 'Command/Ctrl + E' to Edit a file with local tools (Mac OS & Windows)
- [x]   (Fixed) _Front-end_ Going next en prev in search detail view context is going more smooth
- [x]   (Fixed) _Back-end_ Allow websockets in CSP for Safari and old Firefox
- [x]   (Breaking change) _Back-end_ Change "/api/health/version" now its needed to upgrade StarskyApp to 0.3 or newer
- [x]   (Added) _Back-end_ Add Sidecar API (xmp files) for getting by filepath
- [x]   (Added) _Back-end_ Uploading Sidecar API (xmp files)
- [x]   (Fixed) _Back-end_ Fix issue where rename did serve a 500 page after successful renaming
- [x]   (Fixed) _Back-end_ Uploading image with colorClass keeps it own colorClass instead of number 0/ grey
- [x]   (Fixed) _Back-end_ Remove file from temp folder after thumbnail upload (and copy it to thumbnailTemp)
- [x]   (Deprecated) _inotify-settings_ Plans to integrate inotify-wait in to the core product
- [x]   (Added) _Back-end_ Add Caching back for /api/info to 1 minute
- [x]   (Added) _Back-end_ Add Lens Info as field within MakeModel (exif read / xmp read / exiftool write)
- [x]   (Added) _Back-end_ Update Exif Height/ Width when writing XMP files
- [x]   (Added) _Front-end_ Hide large aspect ratios, so show 4:3 but hide 120:450
- [x]   (Added) _App_ Use separate config vars when in non-package mode and production
- [x]   (Added) _Back-end_ Logout page is working again

# version 0.4.0-beta.0 - 2020-10-19
_New Feature: In this release websockets are used (note: when using reverse config)_
- [x]   (Added) _Front-end_ Update view when other clients are updating content
- [x]   (Changed) _Front-end_ In GPX view mode & when unlocked: touchZoom and doubleClickZoom are enabled
- [x]   (Fixed) _Front-end_ When file is added to view, the colorClassActiveList is updated
- [x]   (Fixed) _Front-end_ When folder or file is renamed the clientside cache is not correct
- [x]   (Fixed) _Front-end_ When in Archive mode and 'Move file to trash' client cache is cleared
- [x]   (Added) _Back-end_ Add identifier to '/api/account/status'
- [x]   (Breaking change) _Back-end_ rename api "/account/login" to "/api/account/login"
- [x]   (Breaking change) _Back-end_ rename api "/account/register" to "/api/account/register"
- [x]   (Breaking change) _Back-end_ rename api "/account/register/status" to "/api/account/register/status"
- [x]   (Breaking change) _Back-end_ rename api from "/api/removeCache" to "/api/remove-cache"
- [x]   (Breaking change) _Back-end_ rename api from "/api/downloadPhoto" to "/api/download-photo"
- [x]   (Breaking change) _Back-end_ rename api from "/api/export/createZip" to "/api/export/create-zip"
- [x]   (Breaking change) _Back-end_ rename api from "/export/zip/{f}.zip" to "/api/export/zip/{f}.zip"
- [x]   (Breaking change) _Back-end_ rename api from "/redirect/SubpathRelative" to "/redirect/sub-path-relative"
- [x]   (Breaking change) _Back-end_ rename api from "/api/search/relativeObjects" to "/api/search/relative-objects"
- [x]   (Breaking change) _Back-end_ rename api from "/api/search/removeCache" to "/api/search/remove-cache"
- [x]   (Breaking change) _Back-end_ rename api from "/sync/mkdir" to "/api/sync/mkdir"
- [x]   (Breaking change) _Back-end_ rename api from "/sync" to "/api/sync"
- [x]   (Breaking change) _Back-end_ rename api from "/sync/rename" to "/api/sync/rename"
- [x]   (Added) _Front-end_ When source is missing don't allow user to perform actions in DetailView
- [x]   (Added) _Front-end_ Add link in "/account/login" to account register when user is already logged-in
- [x]   (Fixed) _Back-end_ Upload with direct path is working again

# version 0.3.3 - 2020-10-10
_In the next major release websockets are used, please note when using a reverse proxy_
- [x]   (Fixed) _Back-end_ Allow web app to run outside current folder
- [x]   (Fixed) _Back-end_ Allow linking existing env variables to make configuration easier
- [x]   (Added) _Back-end_ Realtime foundation project to support WebSocket updates (start on issue #75)
- [x]   (Added) _Back-end_ Importer asterisk does not always pick first item (fix issue #140)
- [x]   (Added) _Back-end_ Health Details are logged without Json Exception in Application Insights
- [x]   (Added) _Front-end_ Add link to register page on login screen
- [x]   (Added) _Front-end_ Add link to login page on register screen
- [x]   (Added) _Front-end_ Add 'Move to Trash' to search pages
- [x]   (Fixed) _Front-end_ Allow searching for query `!delete!`
- [x]   (Fixed) _Front-end_ Allow case-insensitive search query for `-inurl`
- [x]   (Fixed) _Front-end_ Add loading delete and undo delete for trash page
- [x]   (Changed) _Front-end_ Upload multiple files after each other instead of in once
- [x]   (Fixed) _Front-end_ Show error status when upload fails instead of loading
- [x]   (Added) _Front-end_ Archive/Search/Trash - When in select mode you can add multiple files
                            to the selection by pressing the shift key and click
- [x]   (Added) _Front-end_ In the search suggestion field arrow up and down keys select next / prev
- [x]   (Fixed) _Front-end_ When typing a suggestion remove the field gives you the main menu back
- [x]   (Security) _App_ update Electron to 9.3.1

# version 0.3.2 - 2020-09-19
- [x]   (Fixed) _Front-end_ DetailView - DateTime push in DetailView has no influence on colorClass anymore
- [x]   (Fixed) _Front-end_ DetailView - Links to collections are always with `details=true`
- [x]   (Fixed) _Front-end_ DetailView - When pressing delete the entire clientSide cache is cleared (to avoid next/prev issues)
- [x]   (Fixed) _Front-end_ Archive - When selecting a new colorClass this is added to the filter
- [x]   (Fixed) _Front-end_ DetailView - Safari 12 and lower does autorotate the image correct
- [x]   (Added) _CLI_ Stop with warning when running WebHtmlPublish over the same folder (checks for `_settings.json`)
- [x]   (Fixed) _Front-end_ Archive - When click on a Link in Archive, with command key it should ignore preloader
- [x]   (Fixed) _Front-end_ Modal Sync Manually - Folders with plus `+` in the url are synced
- [x]   (Fixed) _Front-end_ Modal Sync Manually - When ColorClass is selected, its now updating the state to keep the selection
- [x]   (Fixed) _Front-end_ Modal Sync Manually - Sync Manual and Clears Cache cleans now also the client cache.
- [x]   (Fixed) _Front-end_ DetailView - When pressing ColorClass it also updated when going to the next and back to the same image.
- [x]   (Fixed) _Front-end_ DetailView formcontrol fix styling issue when insert 40 or 00 on a datetime input
- [x]   (Fixed) _Front-end_ Form Control allow command a or ctrl a when a field is full to select the entire text
- [x]   (Security) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.8 (using SDK 3.1.402)
- [x]   (Added) _CLI_ Add account creation by StarskyAdminCli
- [x]   (Added) _AppSettings.UseHttpsRedirection_ - Redirect users to https page.
                                            You should enable before going to production. Always disabled in debug/develop mode
- [x]   (Added) _CLI_ Show DateTime when the Assemblies are build with the flags: `-h -v`

# version 0.3.1 - 2020-09-08
- [x]   (Added) _Front-end_ UI improvement on Archive add t/i keyboard shortcut to select tags
- [x]   (Added) _Front-end_ Client Side caching for 3 minutes to avoid requests and speed on slow devices
- [x]   (Added) _Front-end_ Warning when video is not found
- [x]   (Added) _Front-end_ Warning when playback is not supported or not working
- [x]   (Added) _Back-end_ Download API has now default client side caching
- [x]   (Added) _Front-end_ Add Preloader for ColorClass filter, only used when using this app on a slow server
- [x]   (Added) _Front-end_ Add updating parent items in the front-end cache
- [x]   (Added) _Back-end_ Publish - Files that are not found while publishing are ignored
- [x]   (Added) _Back-end_ Publish - Show status when there a no items found before publishing
- [x]   (Added) _Back-end_ Search - search for colorClass by indexer `--colorclass=1`
- [x]   (Added) _Front-end_ DetailView - Add fast copy for DetailView (press c to save tags, title and description)
- [x]   (Added) _Front-end_ DetailView - Add fast paste for DetailView (press v to overwrite tags, title and description)
- [x]   (Added) _Front-end_ DetailView - Show Notification dialog when Copy or Paste action happens
- [x]   (Fixed) _Front-end_ Search/DetailView - When going fast to the next/prev items this is requesting
                            relativeObjects again to avoid displaying the next icon but not able to click on it
- [x]   (Added) _Back-end_ Add Response compression in ASP.NET Core
- [x]   (Fixed) _Back-end_ Change Cache time to 365 days for clientapp and wwwroot

# version 0.3.0 - 2020-09-02
_Note: When you upgrade from 0.2.7 please make sure you have applied the configuration updates_
- [x]   (Fixed) _Back-end_ publish with metadata did not work
- [x]   (Fixed) _Back-end_ Publisher did rotate images when using Exif Orientation
- [x]   (Fixed) _Back-end_ fix issue where ExifTool executables did not have write access on *nix
- [x]   (Added) _Back-end_ Download Geo-Data from geonames.org on startup
- [x]   (Added) _Back-end_  Web Publisher - first image as other thumbnail format
- [x]   (Added) _Front-end_  Gpx view, Add ZoomIn/ZoomOut (only in GPX mode)
- [x]   (Added) _Front-end_  Gpx View, unlock button (you change the map location now)
- [x]   (Added) _Front-end_  Gpx view, go to current location (no marker, only change view)
- [x]   (Security) _Frond-end_  Upgrade ClientApp CRA _(Create React App 3.4.3 2020-08-12)_
- [x]   (Fixed) _Front-end_ Add Preloader icon when pressing ColorClassSelect
- [x]   (Fixed) _Front-end_ For Archive and Search: When in select mode and navigate next to
                            the select mode is still on but there are no items selected

# version 0.3.0-beta.1 - 2020-08-16
- [x]   __(Breaking change)__ _Back-end_ Manifest (_settings.json) for exporting
- [x]   __(Breaking change)__ _Back-end_ AppSettings config for: AppSettingsPublishProfiles __(need manual config changes)__
- [x]   (Added) Add new Publish UI in Web Interface
- [x]   (Fixed) _Back-end_ change `/api/delete` collections default option

# version 0.3.0-beta.0 - 2020-08-11
- [x]   (Added) _Back-end_ Update meta information for folders
- [x]   (Added) _Back-end_ Write component
- [x]   (Added) _Back-end_ Add read component (sync) [not implemented]
- [x]   (Added) _Back-end_ Move json sidecar file
- [x]   (Added) _Back-end_ Directory sidecar write file
- [x]   (Fixed) _Back-end_ Unknown/GPX files sidecar files
- [x]   (Fixed) _Back-end_ GPX rename file does not work
- [x]   (Added) _Back-end_ FileSize update for add item
- [x]   (Added) _Back-end_ FileSize on add item
- [x]   (Breaking change) _Back-end_ Need to run migrations to add FileSize field (done by starting the mvc application)
- [x]   (Added) _Back-end_ Creating thumbnails from Web Interface (no status)
- [x]   (Changed) _Front-end_ Move options from display options to Synchronize manually in the UI

# version 0.2.7 - 2020-07-31
- [x]   (Security) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.5 (using SDK 3.1.301)
- [x]   (Fixed) _Back-end_ Fix GPS Tracking issue with 'Local' time.
- [x]   (Deprecated) Starsky Net Framework will be unsupported in 0.3
- [x]   (Added) _Back-end_ Docker support,

# version 0.2.6 - 2020-06-08
- [x]   (Added)  _Back-end_ Option for shared `AppSettings`
- [x]   (Added)  _Back-end_ API to update some `appSettings` from the UI
- [x]   (Fixed)  _Tools_ Ignore non-jpeg files for thumbnail tool
- [x]   (Fixed)  _Tools_ ./build.sh `--no-sonar` build flag, to ignore sonarQube
- [x]   (Added)  _Back-end_ Add Permissions `UserManager.AppPermissions.AppSettingsWrite` in Admin scope.
- [x]   (Added)  _Back-end_  `SYSTEM_TEXT_ENABLED` flag is enabled
- [x]   (Changed) _App_ update Electron to 9.0
- [x]   (Changed) _App_ remove inline javascript
- [x]   (Changed) _Back-end_ rename to "/api/account/change-secret"
- [x]   (Added) _Front-end_ Add preferences pane
- [x]   (Added) _Front-end_ Add first version of preferences-app-settings
- [x]   (Added) _Front-end_ Add first version of preferences-password
- [x]   (Fixed) _Back-end_ AppSettings Update API Values that are true are overwritten when summing new value #45
- [x]   (Fixed) _Back-end_  Importer disposed object #46
- [x]   (Fixed) _Back-end_ In /api/update allow null `\\0` to support emthy overwrites
- [x]   (Fixed) _Front-end_ Send null `\\0` value when a user the content in detailView a tags/description field removes
- [x]   (Fixed) _Front-end_ Chrome 81+ Exif rotation on non-thumbnail images #48
- [x]   (Fixed) _Back-end_  Redirect with Prefix issu #49
- [x]   (Security) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.4 (using SDK 3.1.300)
- [x]   (Fixed)  _App_ Add playback for video in App Issue #53
- [x]   (Fixed)  _App_ StarskyApp should see map Issue #52
- [x]   (Fixed)  _Front-end_  Download folders with + (plus) not found Issue #54

# version 0.2.5 - 2020-05-22
- [x]   (Added)  _Tools_ Azure pipeline for starskyApp
- [x]   (Added)  _Tools_ app-version-update.js, add more folders and check input for matching sem-ver
- [x]   (Added)  _Tools_ docs.js styling update
- [x]   (Added)  _Tools_ show `/api/health` results in Application Insights when it fails
- [x]   (Added)  _Back-end_ Fix for `Exist_ExifToolPath` on first run
- [x]   (Added)  _Back-end_ Include ExifTool on first run for Windows and Unix (Perl is needed on \*nix)
- [x]   (Fixed)  _Front-end_ Files that already are deleted is not shown visualy  Issue #26
- [x]   (Fixed)  _Back-end_ starskygeocli -g 0 Not found
- [x]   (Fixed)  _Back-end_ Bug upload or import gpx fails FileError
- [x]   (Added)  _App_  Starsky App Allow Remote connections
- [x]   (Added)  _App_  Starsky App Menu Cleanup
- [x]   (Added)  _App_  Starsky App Multiple window support
- [x]   (Added)  _App_  Starsky App WindowStateKeeper
- [x]   (Added)  _App_  Starsky App Settings window
- [x]   (Changed) _App_ First version of the Starsky Desktop App, required a build for at least v0.2.5
- [x]   (Added)  _Back-end_ Version Health API to match MAJOR and MINOR version for example 0.2

# version 0.2.4 - 2020-05-10
- [x]   (Added)  _Tools_ Easy internal version upgrade Starsky Version
- [x]   (Added)  _Tools_ add check for ProjectGuids to be valid/exist and non-duplicate
- [x]   (Added)  _Back-end_ Show version number in command line
- [x]   (Added)  _Back-end_ Fix for import Gpx
- [x]   (Fixed)  _Front-end_ In DetailView click on colorClass move to next item, the colorClass should match the file
- [x]   (Added)  _Front-end_ Add Storybook for keep components easier to manage.
- [x]   (Changed) _Back-end_ `/api/env` behind login
- [x]   (Added) _Back-end_ allow multiple inputs in importer CLI (dot comma ; separated )
- [x]   (Added) _Tools_ allow multiple inputs in `dropbox-importer`
- [x]   (Fixed) _Back-end_ QueueBackgroundWorkItem has now Application Insights Telemetry tracking for exceptions
- [x]   (Fixed) _Back-end_ Fix for imageFormat GPX. does now support wihout xml prefix
- [x]   (Fixed) _Back-end_ Bugfix for Importer to allow .XMP files read and copy

# version 0.2.3 - 2020-05-04
- [x]   (Fixed)  _Back-end_ New users could not sign up
- [x]   (Fixed)  _Front-end_ Register page has wrong title
- [x]   (Fixed)  _Front-end_ Login flow return url fixed
- [x]   (Fixed)  _Front-end_ add bugfix for double slash on home while selecting files
- [x]   (Fixed)  _Front-end_ use appendChild instead of append in portal for older browsers
- [x]   (Fixed)  _Front-end_ order when files are added does now match the backend (archive-context)
- [x]   (Removed) _Back-end_ Import to filter on files older than 2 years
- [x]   (Fixed)  _Back-end_  Import UnitTests __Can't build after 2020-04-22, Import UnitTests have a date bug.__
                             __For all versions older than 0.2.2__
- [x]   (feature) _Back-end_ Import to async function refactor
- [x]   (Fixed)  _Back-end_ Fixes for bugs introduced after refactoring
- [x]   (Fixed)  _Back-end_ Bugfixes for starskyImporter
- [x]   (Fixed)  _Back-end_ Delete After now works
- [x]   (Fixed)  _Back-end_ Import IndexMode works
- [x]   (Fixed)  _Back-end_ Import File Extension issue
- [x]   (Fixed)  _Back-end_ Import Empty string import nice warning (0 results)
- [x]   (Fixed)  _Front-end_ video invalid datetime (UTC Time issues)
- [x]   (Fixed)  _Back-end_ Force Sync fail (Object Disposed)
- [x]   (Fixed)  _Back-end_ Export fail (Object Disposed)
- [x]   (Fixed)  _Back-end_ Upload with filename the same name does add item to cache + should update thumbnail cache

# version 0.2.2 - 2020-04-17
__Should build before 2020-04-22, Import UnitTests have a date bug. For all versions older than 0.2.2__
- [x]   (Added) _Front-end_ Timezone issues in Safari
- [x]   (Feature) _Front-end_ Add menu for search
- [x]   (Fixed)  _Front-end_ Collection support in update tags / ColorClassSelect
- [x]   (Fixed) _Front-end_ navigator.language issue in Safari
- [x]   (Changed) _Front-end_ use `starsky` prefix in api urls
- [x]   (Fixed) _Front-end_ use `starsky` prefix only when needed
- [x]   (Fixed) _Back-end_ __Cookie path fix for stuck in 'Do you want to log out?' screen__
- [x]   (Fixed) _Front-end_ with prefix on the archive page navigate to the right url
- [x]   (Fixed) _Front-end_ Search page cache not cleared after edit multiple images
- [x]   (Fixed) _Front-end_ Delete multiple images collections no applied
- [x]   (Other) _Front-end_ search tags detailView cache not cleared only if you switch very fast _known issue_ _won't fix_
- [x]   (Fixed) _Front-end_ search update read only files, Created an error message if this happens
- [x]   (Fixed) _Front-end_ sync API 404 fix from UI
- [x]   (Fixed) _Back-end_ QuickTime DateTime creates error while checking GPX files
- [x]   (Added) _Front-end_ 'Scroll to Top' when to next search result page

# version 0.2.1 - 2020-04-08
_Should build before 2020-04-22, Import UnitTests have a date bug. For all versions older than 0.2.2_
- [x]   (Fixed) _Front-end_ Readonly mode and modals
- [x]   (Added) _Back-end_ ReadOnly status to DetailView
- [x]   (Added) _Back-end_ mp4/h.264 video support
- [x]   (Added) _Front-end_ video player (mp4)
- [x]   (Added) _Back-end_  unit tests for mp4/quickTime
- [x]   (Security) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.3 (using SDK 3.1.201)
- [x]   (Security) _Back-end_ Lots of dependencies (EF Core to 3.1.3)
- [x]   (Security) _Frond-end_  Upgrade ClientApp CRA _(Create React App 3.4.1 2020-03-20)_
- [x]   (Changed) _Back-end_ Use vstest instead of mstest

# version 0.2.0 - 2020-03-20
_Should build before 2020-04-22, Import UnitTests have a date bug. For all versions older than 0.2.2_
- [x] 	(feature) _Front-end_ icons for xmp and raw (tiff-based) in archive mode
- [x] 	(feature) _Back-end_ support for Canon's way of reading ISO-Speed
- [x]	(feature) _Back-end_ abstractions to get the filesystem data
- [x]	(feature) _Back-end_ Injection framework implemented
- [x] 	(rename) _Back-end_ Feature renaming and docs updates
- [x] 	(feature) _Back-end support for RAW that is not Sony for example Nikon `.NEF`
- [x]   (feature) tiff, `arw`:sony, `dng`:adobe, `nef`:nikon, `raf`:fuji, `cr2`:canon,
                        `orf`:olympus, `rw2`:panasonic, `pef`:pentax,
- [x]   (bugfix) _Back-end_ allow underscore import/upload (api name changed in later version)
- [x]   (bugfix) _Front-end_ Download selection thumbnail right extension suggestion
- [x]   (version) _Back-end_ __breaking change__ rename of api `/api/import/history`
- [x]   (version) _Back-end_ __breaking change__ rename of api `/api/import/thumbnail`
- [x]   (version) _Back-end_ __breaking change__ rename of api `/api/import`
- [x]   (version) _Back-end __breaking change__ remame `"Path": "{AssemblyDirectory}/WebHtmlPublish/EmbeddedViews/`
- [x]   (version) _Back-end_ __namespace changes__ Introduction of feature/foundation projects

# version 0.1.17 - 2020-03-07
_Should build before 2020-04-22, Import UnitTests have a date bug. For all versions older than 0.2.2_
- [x]  (feature) _Front-end_ DateTime editing in detailView
- [x]  (feature) _Front-end_ change DateTime layout
- [x]  (feature) _Back-end_ tags XMP in file read/write support
- [x]  (feature) _Back-end_ description XMP in file read/write support
- [x]  (feature) _Back-end_ title XMP in file read/write support
- [x]  (feature) _Back-end_ dateTime XMP in file read/write support
- [x]  (feature) _Back-end_ latitude XMP in file read/write support
- [x]  (feature) _Back-end_ longitude XMP in file read/write support
- [x]  (feature) _Back-end_ locationAltitude XMP in file read/write support
- [x]  (feature) _Back-end_ locationCity XMP in file read/write support
- [x]  (feature) _Back-end_ locationState XMP in file read/write support
- [x]  (feature) _Back-end_ locationCountry XMP in file read/write support
- [x]  (feature) _Back-end_ aperture XMP in file read/write support
- [x]  (feature) _Back-end_ shutterSpeed XMP in file read/write support
- [x]  (feature) _Back-end_ isoSpeed XMP in file read/write support
- [x]  (feature) _Back-end_ focalLength XMP in file read/write support
- [x]  (bugfix) _Back-end_ search fix bug where Make/Model is giving a Exception (fixed)
- [x]  (bugfix) _Front-end_ clientside bug dateTime is not displayed correct

# version 0.1.16 - 2020-02-23
- [x]  (bugfix) _Back-end_ `/api/import/fromUrl` Path Traversal Injection fix
- [x]  (feature) _Back-end_ Feature toggle to change from `Newtonsoft.Json` to `System.Text.Json`
                            _using Newtonsoft for this version_
- [x]  (bugfix) _Frond-end_ Trash display content after deleted (changes in wrapper)
- [x]  (rename) _Back-end_ `/api/import/allowed` to `/api/allowed-types/mimetype/sync`
- [x]  (upgrade) _Frond-end_  Upgrade ClientApp CRA _(Create React App 3.3.1, 2020-01-31)_
- [x]  (bugfix) _Frond-end/Back-end_ Add length limit length for search queries
- [x]  (bugfix) _Frond-end_ Add length limit length for tags
- [x]  (feature) _Back-end_ Change Password for current user (API only)
- [x]  (bugfix) _Back-end_  System.IO.EndOfStreamException: Expected to read 372 payload bytes but only received 21.
        on `/api/search?t=-Datetime>1+-Datetime<0+-ImageFormat:jpg` (Timeout issue)
- [x]  (bugfix)  Unit tests for NOT queries `/search?t=-Datetime%3E7%20-ImageFormat-%22tiff%22`
- [x]  (performance) _Back-end_  refactoring of `/api/search`
- [x]  (performance) _Back-end_  refactoring of `SearchSuggestionsService`
- [x]  (bugfix) _Back-end_  add version to application insights
- [x]  (version) _Back-end_  Upgrade .NET Core (TargetFramework) to 3.1.1 (using SDK 3.1.101)
- [x]  (version) _Back-end_ Upgrade RazorLight to 2.0.0-beta4
- [x]  (feature) _CLI_ StarskyAdminCli is added (but not documented or included by default)
- [x]  (feature) _Front-end_ Show warning when there are connection issues
- [x]  (feature) _Back-end_ Search: support for complex and/or operators `(this || or) && that`

# version 0.1.15 - 2020-02-06
- [x]  (bugfix) _Front-end_ Drag'n drop is now only with files
- [x]  (version) _Back-end_ _Legacy starsky.netframework_ 0.1.15 release included
- [x]  (version) _Back-end_ _dependecies_ Microsoft.EntityFrameworkCore, Microsoft.Extensions.Configuration to 3.1.1
- [x]  (version) _Back-end_ _dependecies_ starskycore.dll from [netstandard2.0;netstandard2.1] to netstandard2.0
- [x]  (version) _Back-end_ _rename_ starskySyncNetFrameworkCli and starskyImporterNetFrameworkCli
- [x]  (version) _Back-end_  __breaking change__ add Software as field in database (run migrations)

# version 0.1.14 - 2020-02-04
- [x]   (bugfix) _back-end_ Security fixes in controllers
- [x]   (bugfix) _back-end + front-end_ name: colorClassActiveList replace everywhere
- [x]   (version) _Back-end_ update to .NET .Core SDK 3.1.101 and version 3.0.2 (TargetFramework).
                            Run `./build.sh` before you start developing
- [x]   (bugfix) _Front-end_ preloader when uploading
- [x]   (bugfix) _Front-end_ translations in item-list-view, item-text-list-view,
- [x]   (bugfix)  _Front-end_ translations in containers/search, containers/trash, search, trash and trash-page
- [x]   (bugfix)  _Front-end_ translations in menu-trash and modal-export (isProcessing === ProcessingState.server)
- [x]   (feature) _Front-end_ tags on folder change to two line
- [x]   (feature) _Front-end_ _Back-end_ Health view, to make more clear when paths are configured right
- [x]   (feature) _Front-end_ _Back-end_ Health view, to make more clear when the server time is not correct
- [x]   (feature) _Front-end_ _Back-end_ Health view (feature) when a disk is full, show a warning

# version 0.1.13 - 2020-01-25
- [x]   (remove) _V1_ __Removal of Old Layout (V1) and All Razor Views__
- [x]   (feature) _Front-end_ _API_ Add new account in React
- [x]   (feature) _Front-end_ _API_ Redirect to new account creation on new setup
- [x]   (feature) _Front-end_ Upload to directory (+ backend)
- [x]   (bugfix) add filter for backslashes in structure: `\\\\d`
- [x]   (breaking change) removal of GetColor in razor views
- [x]   (breaking change) rename of field  colorClassFilterList={[]} ==>  colorClassActiveList={[]}
- [x]   (bugfix) colorclass filter are selecting
- [x]	  (bugfix) export poll after 206 'not ready'
- [x]   (feature) _Front-end_ make folder layout smooth responsive

# version 0.1.12 - 2020-01-15
- [x]   (bugfix) _Front-end_ Add Loading in Move dialog
- [x]   (feature) _Front-end_ Move file in menu
- [x]   (bugfix) _API_ fix various issues in `/api/rename`
- [x]   (remove) _General_ starskyApp content
- [x]   (docs) _General_ add html generation to build process
- [x]   (remove) _API_ only the retryThumbnail is removed `api/thumbnail?retryThumbnail=true`  
                        (remove thumbnail if corrupt) due public facing
- [x]   (remove) _Front-end_ search replace is no longer a beta feature, so no feature toggle
- [x]   (bugfix) _Front-end_ archive updating multiple values didn't provide the right results
- [x]   (bugfix) _Front-end_ when pressing multiple colorclass items with the value 0 (colorless/no-color) these are updated
- [x]   (bugfix) _Front-end_ fix issue where force sync and clear cache didn't update the view
- [x]   (bugfix) _Front-end_ fix issue Collections toggle where not correct shown
- [x]   (feature) _API_ Add `/sync/mkdir` to create directories and sync to the db
- [x]   (feature) _Front-end_ Add Modal for Make Directory to the Archive Menu
- [x]   (build) _CI_ With `CI=true` eslint errors will break the build
- [x]   (feature) _Front-end_ Add English language to most of the menu items (switched by browser language)
- [x]   (bugfix) _Front-end_ When changing search page there is a preloader icon shown
- [x]   (feature)  _Front-end_ clear search cache after updating values in detailview

# version 0.1.11 - 2020-01-02
- [x]   (bugfix) _Front-end_ _Detailview_ when press Delete and switch image, the next image should be marked as not deleted (Fixed)
- [x]   (bugfix) _Front-end_ _Archive_ when press 'Select' the images are not reloaded (Fixed)
- [x]   (bugfix) _Front-end_ _Archive_ _iOS_ After press 'Select' and return the scrollstate keeps on the same position (fixed)
- [x]   (bugfix) _Front-end_ When going from Archive to Detailview and back the scrollstate is still the same (fixed)
- [x]   (feature) _Front-end_ Dark theme style added
- [x]   (bugfix) _Front-end_ _Chrom{e,ium}_ the location after loging in now updated (fixed)
- [x]   (bugfix) _CORS_ Add AllowCredentials policy for production
- [x]   (feature) _Front-end_ Next/Prev back in Detailview is now also support when searching
- [x]   (change) _Front-end_ Remove V1 from main menu
- [x]   (feature) _API_ Add new endpoint `/api/search/relativeObjects`
- [x]   (bugfix) _Front-end_ change default outline for Chrome/Safari
- [x]   (bugfix) _Front-end_ fix document.title undefined error
- [x]   (bugfix) _Front-end_ fix issue where on empty searchquery a sidebar is shown
- [x]   (bugfix) _Front-end_ When import a non supported image Ok is shown
- [x]   (feature) _Front-end_ Show GPX Files with a map (powered by leaflet/openstreet maps)
- [x]   (change) _Back-end_ __Breaking change__ Rename suggest API from `/suggest` to `/api/suggest`
- [x]   (bugfix) _Front-end_ Rotation in Detailview on iPad OS 13+ is working (fixed)
- [x]   (feature) _API_ Add is Valid Filename check on rename API (`/api/rename`)

# version 0.1.10 - 2019-12-15
- [x]   (bugfix) Archive => After pressing 'Apply' the updates are not shown
- [x]   (version) _Front-end_  Upgrade ClientApp from React 16.9.0 to 16.9.15 _(Create React App 3.3.0, 5 Dec 2019)_
- [x]   (bugfix) _Front-end_  Front-end for Rename files (in detailview)
- [x]   (feature) _Front-end_  (front-end) 'Rotate to Right'
- [x]   (bugfix) _Front-end_  Improve Unit test coverage (at least 80% on coverage-report _561 mstest and 271 jest tests_)
- []    (bug) _Front-end_ `/starsky` paths are not supported __not fixed__

# version 0.1.9 - 2019-12-01
_Upgrade to .NET Core 3.0 (TargetFramework) & EF Core 3.1-preview3_
- [x] use for example '3 hours' instead of yesterday in detailview
- [x] (bugfix) EventTarget issue (Safari) when using /import
- [x] (azure-pipeline) add yaml file to replace classic build pipeline
- [x] Indexes are working now for MySQL at the first time run
- [x] Upgrade to .NET Core & EF Core 3.1-preview3 (EF Core 3.0 is missing PredicateBuilder support)
- [x] (bugfix) 23:00 o'clock/first of month date.spec unittest
- []  (Add warning) (bug) for iOS Safari only when using .local domains login fails
                       (work around use ip-addresses) (update to iOS13)
- [x] Update build pipeline to support multiple runtimes in 1 run
- [x] Upgrade from Swagger 4.0.1 to Swagger 5 (has breaking changes)

# version 0.1.8 - 2019-11-10
- (bugfix) ignore directories without reading rights (instead of crashing)
- (change) Import API has now a limit of 320 MB instead of 32MB
- [x]   Login in V2 layout
- (bugfix) Catch is used for example the region VA (Vatican City)
- [x]   (bugfix) when offline geoReverseLookup creates an 0 byte zip
- [x]   (bugfix) (UI) in archive mode, selecting and deselecting ColorClass does not include filepaths in request.
- [x]   (bugfix) (UI) when pressing force sync and renew now the view is updated
- [x]   (tools) add Dropbox Import tool
- __Breaking API change__ from `/import/fromUrl/` to `/api/import/fromUrl`
- [x]   Import page (without link)
- [x]   (bug) GPX, tiff, dng  files uploads are not allowed in new UI (fixed)
- [x]   (feature) Geo from Web Interface (including status) - in preview status
- [x]   (bug) Force sync and renew for directories that contain a + sign are passing the wrong values (fixed)
- [x]   Add unit tests for importing raw's with .xmp files
- [x]   (starsky-tools) add Dropbox import helper tool

# version 0.1.7 - 2019-09-27
_Works with  .NET Core SDK 3.0.100_
- (bugfix) LastEdited (in front-end) is now also shown when there is no Datetime
- (bugfix) after account is created the redirect to a 404 page
- (bugfix) (layout v1) import to the right controller
- (alpha api/subject to change json output) /api/health to check the status of the application
- __Breaking change__ rename of `starsky-node-client` → `starsky-tools`
- (starsky-tools/localtunnel) to test local builds
- starsky-tools/thumbnail, added auto cleanup, allow ranges e.g. 1-20 ago
- (V2 UI Archive/Trash) add Select all/Undo selection to menu
- (bugfix) search queries shorter than 2 digits are working
- __(behind feature flag)__ Front-end for Replace API `archive-sidebar-label-edit` in folder view
                            (add localStorage item with name `beta_replace`)
- (front-end) `archive-sidebar-label-edit` split in separate components
- (front-end) show collections in DetailView
- (front-end) reject delete button when a file is read-only mode
- (front-end) add to-trash-button and select all button to archive view
- (front-end) add keyboard shortcut for delete
- (front-end) add feature to cancel image loading if the page change
- (front-end) refresh/forceSync does reload page
- (dotnet) version from 2.2.6 to 2.2.7
- (dotnet) Microsoft.AspNetCore.App 2.2.7 is added as dependency to avoid mixed version errors
- (bugfix) menu-archive press TrashSelection gives no 404 error anymore

# version 0.1.6 - 2019-09-12
__For this version you need to downgrade the .NET Core SDK to SDK 2.2.401__
- __Breaking change__ the V1 layout is now at `/v1`
- Add Renewed UI with most of the functionality of the old UI (default on)
  - [x]  Folder/Archive UI
  - [x]  Folder/Archive ColorClass filter UI
  - [x]  Folder/Archive Select UI
  - [x]  Folder/Archive Labels Add UI
  - [x]  Folder/Archive Labels Overwrite UI
  - [x]  Search UI
  - [x]  Export/ Dialog/ Single + Select UI
  - [x]  DetailView (include details) UI
  - [x]  Collections toggle in UI
  - [x]  ForceSync in UI (under Display options)
  - [x]  Cache-clean for folders in UI (under Display options)
  - [x]  Toggle for isSingleItem (under Display options)
  - [x]  Toggle for Collections view (under Display options)
  - [x]  Import page __Has link to V1 layout__
  - [x]  Account page __Deprecated in V2 layout__
  - [x]  Login __is in V1 layout style__
  - [x]  Not Found page
  - [x]  Trash page
- Build CI changes to run Jest tests (1.2% coverage yet)
- IE11 (Internet Explorer) is not working anymore with this application
- Some older Safari, Chrome and Firefox browsers are not supported in the new layout
- Added `ExifStatus.Deleted` including in `ReplaceService`
- __Breaking API change__ from `/search/` to `/api/search`
- __Breaking API change__ from `/search/trash` to `/api/search/trash`
- __API change__ change number of search results per page from 20 to 120
- _Legacy starsky.netframework_ 0.1.6 release included

# version 0.1.5.9 - 2019-08-19
_Version number does not match SemVer_
- Entity Framework add database indexes
- __Breaking Change__  Entity Framework add database Field for FocalLength
- _Legacy starsky.netframework_ 0.1.5.9 release included

# version 0.1.5.8 - 2019-08-14
_Version number does not match SemVer_
- Change Dot NET version to the `.Net Core 2.2` (TargetFramework) release (C# 7)
- Rollback version due Entity Framework performance issues with MySQL
- Swagger is enabled
## The following changes from 0.1.5.7 are included in this release
- __Breaking API change__ from `/account?json=true` to `/account/status` `api`
- __Breaking API change__ from `/api/` to `/api/index`
- Add support for command line -x or don't add xmp sidecar file
- [x]   XMP disable option when importing using a flag (used for copying photos)
- _Legacy starsky.netframework_ 0.1.5.8 release included

# version 0.1.5.7 - 2019-08-09
_Version number does not match SemVer_
- Update Dot NET version to the `.Net Core 3 Preview 7` (TargetFramework) release
- Update to C# version 8
- Keep the core .netstandard2.0 for NetFramework reference
- __Breaking API change__ from `/account?json=true` to `/account/status` `api`
- __Breaking API change__ from `/api/` to `/api/index`
- __Known issue__ Swagger support is disabled
- Add support for command line -x or don't add xmp sidecar file
- [x]   XMP disable option when importing using a flag (used for copying photos)
- _Legacy starsky.netframework_ 0.1.5.7 release included

# version 0.1.5.6 - 2019-08-07
- change '/api/info' to support readonly meta display
- add /suggest/all to show all suggestions
- upgrade dependencies to support Debian 10 (to fix: No usable version of the libssl was found)
- fix localisation issue with starskyWebHtmlCli
- add copy of content folder in bin with starskyWebHtmlCli

# version 0.1.5.5 - 2019-05-17
- implement search suggestions API `/suggest?t=d`
- search suggestions are always lowercase
- bugfix: `/api?f=detailView` pages are now working
- suggestions are part of the warmup script
- bugfix: spaces where not rendered correctly during the 'update' call in archive view
- __bugfix: you could login without password__

# version 0.1.5.4 - 2019-04-24
- fix: for readonly there is no TIFF label
- Front-end copy ctrl+shift+c visual feedback
- Warmup script with variables
- Allow CORS for DEBUG mode for localhost domains
- __CHANGE:__ Unauthorised users return 401 on /api (instead of redirect)
- add: `/import/history` API for viewing recent uploads (today only) _subject to change_
- __CHANGE__ Database Structure: Field added in ImportDatabas
  Update all your clients at once to avoid issues between -3 and -4
- _Legacy starsky.netframework_ 0.1.5.4 release included

# version 0.1.5.3 - 2019-03-31
- refactoring connection to ExifTool to use iStorage
- refactoring thumbnail service
- changed Basic Auth middleware to use scoped
- bugFix: sync from webUI with '+' in name
- _Legacy starsky.netframework_ 0.1.5.3 release included
## Features that are affected:
- [x]   check UpdateWriteDiskDatabase / check update service
- [x]   check rotation
- [x]   check GeoLocationWrite
- [x]   check: AddThumbnailToExifChangeList (removed)
- [x]   check: rename thumb in starskyGeoCli
- [x]   check: StarskySyncCli -t (thumbnail service)
- [x]   check: /api/delete
- [x]   check: /api/download photo
- [x]   check: xmpSync for exiftool
- [x]   check: starskyWebHtmlCli
- [x]   check: ResizeOverlayImage in webHtmlCli
- [x]   check: if (profile.MetaData) // todo: check if works
- [x]   check: export to zip
- [x]   check: legacy releases

# version 0.1.5.2 - 2019-03-22
- exifTool implementation write bug fixed
- [x]   Performance upgrade xmp/tiff files (does not check filehash again)
- refactor xmp/exif module to support iStorage
- bugfix to searching filehashes with null content
- include test with incomplete xmp file
- ExifToolImportXmpCreate in appsettings
- Bugfix / bug fix for: issue where import with spaces creates multiple items in the database
- Already exist: config scheme overwrite feature for command line e.g. --scheme:/yyyy
- _Legacy starsky.netframework_ 0.1.5.2 release included

# version 0.1.5.1 - 2019-03-17
- Breaking Change: added `LastEdited` field
- Add `LastEdited` to search as field
- _Legacy starsky.netframework_ 0.1.5.1 release included

# version 0.1.5 - 2019-03-17
- add partial support for || (or) queries using search
	-	the type datetime e.g. `-datetime=1 || -datetime=2` is not supported yet
- bugfix to large int relative to today
- add support for not queries `-file` to ignore the word file
- performance improvements to importer
- .NET Core to 2.1.8  (TargetFramework) (update to dotnet-sdk-2.2.104)
- input args changed: --recursive (typo)
- frontend readonly bug fix
- frontend temp disabled alt+next loop due set-interval overflow
- Search: performance update for searching multiple tags
- Replace API introduced, search and replace in strings
- Done some IStorage refactorings, but not complete yet
- Next/Prev links are now served by backend code to avoid when javascript is not loaded, your selection is reset
- Add Collections to DetailView model
- Add more Update/Replace Tests

# version 0.1.4 - 2019-03-01
- fix issue where login fails results in a error 500
- http push headers update (add /api/info to push on detailView)
- Initial release of the `sync/rename` api (not implemented in the front-end)
- Mark FilesHelper as deprecated, use IStorage now

# version 0.1.3 - 2019-02-13
- fix issues on exporting
- fix issue on Sync (-p option)
- change `ArgsHelper(appSettings).GetPathFormArgs` need to have appSettings
- fix UI: zooming in on iPad triggers next/prev
- fix init sqlite for legacy app
- add: replace `{AssemblyDirectory}` in `AppSettingsPublishProfiles.Path`
- change settings to enable swagger: use now `app__AddSwagger` to enable
- Create build scripts using Cake
- _Legacy starsky.netframework_ 0.1.3 release included

# version 0.1.2 - 2019-02-01
- starskywebftpcli
- add json export for starskyWebHtmlCli
- bugfix: migrations
- change to runtime: 2.1.7
- add 'import/FromUrl' api
- _Legacy starsky.netframework_ 0.1.2 release included
## Known issues in this release: _(all fixed in 0.1.3)_
- [x]   Export: When export a gpx file this is ignored
- [x]   Export: When export a thumbnail of a Raw file, the zip has no files
- [x]   Sync: Feature for selecting a folder with the sync cli does not work correctly
- [x]   UI: zooming in on iPad triggers next/prev

# version 0.1.1 - 2019-01-25
- add ignore index feature to importer
- _Legacy starsky.netframework_ 0.1.1 release included

# version 0.1.0 - 2019-01-22
- initial release

# version 0.0.1 - 2018-03-08
- Initial commit

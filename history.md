[< readme](readme.md)

# Todo:
## Features planned (in random order) and lower priority
- []   Realtime Files API
- []   mp4/h.264 video support
- []   time zone shifting
- []   Search: support for complex and/or operators `(this || or) && that`
- []   Docker support, including backend abstractions to get the data (partly working)
- []   Health view, to make more clear when paths are configured right
- []   Health view, to make more clear when the server time is not correct
- []   Health view (feature) when a disk is full, show a warning
- []   (front-end) Search details show exact query behind modal
- []   Upload to direct folder API + front-end
- []   (front-end) Info Messages to show actions (e.g. pressed copy all fields shortcut )

## High priority features planned
- []   (feature) Creating thumbnails from Web Interface (including status)

# version 0.1.11 - tba
- [] Change from `Newtonsoft.Json` to `System.Text.Json` __not implemented__

# version 0.1.10 - 2019-12-15
- [x] (bugfix) Archive => After pressing 'Apply' the updates are not shown
- [x] Upgrade ClientApp from React 16.9.0 to 16.9.15
- [x] Front-end for Rename files (in detailview)
- [x] (front-end) 'Rotate to Right'
- [x] Improve Unit test coverage (at least 80% on coverage-report _561 mstest and 271 jest tests_)
- [] (bug) `/starsky` paths are not supported __not fixed__

# version 0.1.9 - 2019-12-01
_Upgrade to .NET Core 3.0 & EF Core 3.1-preview3_
- [x] use for example '3 hours' instead of yesterday in detailview
- [x] (bugfix) EventTarget issue (Safari) when using /import
- [x] (azure-pipeline) add yaml file to replace classic build pipeline
- [x] Indexes are working now for MySQL at the first time run
- [x] Upgrade to .NET Core & EF Core 3.1-preview3 (EF Core 3.0 is missing PredicateBuilder support)
- [x] (bugfix) 23:00 o'clock/first of month date.spec unittest
- []  (Add warning) (bug) for iOS Safari only when using .local domains login fails (work around use ip-addresses) __bug is not fixed__
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
- __(behind feature flag)__ Front-end for Replace API `archive-sidebar-label-edit` in folder view (add localStorage item with name `beta_replace`)
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

# version 0.1.5.9 - 2019-08-19
_Version number does not match SemVer_
- Entity Framework add database indexes
- __Breaking Change__  Entity Framework add database Field for FocalLength

# version 0.1.5.8 - 2019-08-14
_Version number does not match SemVer_
- Change Dot NET version to the `.Net Core 2.2` release (C# 7)
- Rollback version due Entity Framework performance issues with MySQL
- Swagger is enabled
## The following changes from 0.1.5.7 are included in this release
- __Breaking API change__ from `/account?json=true` to `/account/status` `api`
- __Breaking API change__ from `/api/` to `/api/index`
- Add support for command line -x or don't add xmp sidecar file
- [x]   XMP disable option when importing using a flag (used for copying photos)

# version 0.1.5.7 - 2019-08-09
_Version number does not match SemVer_
- Update Dot NET version to the `.Net Core 3 Preview 7` release
- Update to C# version 8
- Keep the core .netstandard2.0 for NetFramework reference
- __Breaking API change__ from `/account?json=true` to `/account/status` `api`
- __Breaking API change__ from `/api/` to `/api/index`
- __Known issue__ Swagger support is disabled
- Add support for command line -x or don't add xmp sidecar file
- [x]   XMP disable option when importing using a flag (used for copying photos)

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

# version 0.1.5.3 - 2019-03-31
- refactoring connection to ExifTool to use iStorage
- refactoring thumbnail service
- changed Basic Auth middleware to use scoped
- bugFix: sync from webUI with '+' in name
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

# version 0.1.5.1 - 2019-03-17
- Breaking Change: added `LastEdited` field
- Add `LastEdited` to search as field

# version 0.1.5 - 2019-03-17
- add partial support for || (or) queries using search
	-	the type datetime e.g. `-datetime=1 || -datetime=2` is not supported yet
- bugfix to large int relative to today
- add support for not queries `-file` to ignore the word file
- performance improvements to importer
- .NET Core to 2.1.8 (update to dotnet-sdk-2.2.104)
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
- http push headers update (add /api/info to push on detailview)
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

# version 0.1.2 - 2019-02-01
- starskywebftpcli
- add json export for starskyWebHtmlCli
- bugfix: migrations
- change to runtime: 2.1.7
- add 'import/FromUrl' api
## Known issues in this release: _(all fixed in 0.1.3)_
- [x]   Export: When export a gpx file this is ignored
- [x]   Export: When export a thumbnail of a Raw file, the zip has no files
- [x]   Sync: Feature for selecting a folder with the sync cli does not work correctly
- [x]   UI: zooming in on iPad triggers next/prev

# version 0.1.1 - 2019-01-25
- add ignore index feature to importer

# version 0.1.0 - 2019-01-22
- initial release

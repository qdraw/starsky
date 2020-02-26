[< readme](readme.md)

# History

## Todo:
### Features planned (in random order) and lower priority
- []   (feature) _Back-end_ Realtime Files API
- []   (feature) _Back-end_ mp4/h.264 video support
- []   (feature) _Back-end_ Docker support, including backend abstractions to get the data (partly working)
- []   (feature) _Frond-end_  Search details show exact query behind modal
- []   (feature) _Frond-end_  Info Messages to show actions (e.g. pressed copy all fields shortcut )
- []   (feature) _Front-end_ Zoom in picture

### High priority features planned
- []  (feature) Creating thumbnails from Web Interface (including status)

# version 0.1.17 - 2020-02-??
- [x]  (feature) _Front-end_  Datetime editing in detailView
- [x]  (feature) _Front-end_ change datetime layout
- [x]  (feature) _Back-end_ tags XMP in file read/write support
- [x]  (feature) _Back-end_ description XMP in file read/write support
- [x]  (feature) _Back-end_ title XMP in file read/write support
- [x]  (feature) _Back-end_ dateTime XMP in file read/write support
- [x]  (feature) _Back-end_ latitude XMP in file read/write support
- [x]  (feature) _Back-end_ longitude XMP in file read/write support

# TODO ADD:
# XMP read/write in file support for:
locationAltitude	0
locationCity	""
locationState	""
locationCountry	""
aperture	0
shutterSpeed	""
isoSpeed	0
software	null
makeModel	""
make	""
model	""
focalLength	0

- []   (bug) _Front-end_ Readonly mode and modals __not implemented__


# version 0.1.16 - 2020-02-23
- [x]  (bugfix) _Back-end_ `/api/import/fromUrl` Path Traversal Injection fix
- [x]  (feature) _Back-end_ Feature toggle to change from `Newtonsoft.Json` to `System.Text.Json`
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
- [x]   (version) _Back-end_ update to .NET .Core SDK 3.1.101 and version 3.0.2 (TargetFramework). Run `./build.sh` before you start developing
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
- [x]   (remove) _API_ only the retryThumbnail is removed `api/thumbnail?retryThumbnail=true`  (remove thumbnail if corrupt) due public facing
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

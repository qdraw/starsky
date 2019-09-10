[< readme](readme.md)

# Features planned (in random order)

- []   Realtime Files API
- []   mp4/h.264 video support
- []   time zone shifting
- []   Pagination on folders - Performance update for example 800 files
- []   Search: support for complex and/or operators `(this || or) && that`
- []   Add unit tests for importing raw's with .xmp files
- []   Upgrade from Swagger 4.0.1 to Swagger 5 (has breaking changes)
- []   Docker support, including backend abstractions to get the data (partly working)
- []   Health view, to make more clear when paths are configured right
- []   Health view, to make more clear when the server time is not correct

## Work in progress `/feature/201909-react`
- []   Front-end for Rename files
- []   Front-end for Replace API in folder view
- []   Import page (without link)
- []   Login in V2 layout

# planned for 0.1.6
- []   Trash page

# version 0.1.6 - tbd
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
- Build CI changes to run Jest tests (1.2% coverage yet)
- IE11 (Internet Explorer) is not working anymore with this application
- Some older Safari, Chrome and Firefox browsers are not supported in the new layout
- Added `ExifStatus.Deleted` including in `ReplaceService`
- __Breaking API change__ from `/search/` to `/api/search`
- __Breaking API change__ from `/search/trash` to `/api/search/trash`

# version 0.1.5.9 - 2019-08-19
- Entity Framework add database indexes
- __Breaking Change__  Entity Framework add database Field for FocalLength

# version 0.1.5.8 - 2019-08-14
- Change Dot NET version to the `.Net Core 2.2` release (C# 7)
- Rollback version due Entity Framework performance issues with MySQL
- Swagger is enabled
## The following changes from 0.1.5.7 are included in this release
- __Breaking API change__ from `/account?json=true` to `/account/status` `api`
- __Breaking API change__ from `/api/` to `/api/index`
- Add support for command line -x or don't add xmp sidecar file
- [x]   XMP disable option when importing using a flag (used for copying photos)

# version 0.1.5.7 - 2019-08-09
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

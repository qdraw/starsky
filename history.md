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
- []   XMP disable option when importing using a flag (used for copying photos)
- []   Health view, to make more clear when paths are configured right
- []   Health view, to make more clear when the server time is not correct


## Work in progress `/feature/201904-react`

- []   Front-end for Rename files
- []   Front-end for Replace API in folder view

# version 0.1.5.5 - 2019-05-17
- implement search suggestions API `/suggest?t=d`
- search suggestions are always lowercase
- bugfix: `/api?f=detailView` pages are now working
- suggestions are part of the warmup script
- bugfix: spaces where not rendered correctly during the 'update' call in archive view
- bugfix: you could login without password

# version 0.1.5.4 - 2019-04-24
- fix: for readonly there is no TIFF label
- Front-end copy ctrl+shift+c visual feedback
- Warmup script with variables
- Allow CORS for DEBUG mode for localhost domains
- __CHANGE:__ Unauthorised users return 401 on /api (instead of redirect)
- add: `/import/history` API for viewing recent uploads (today only) _subject to change_
- __CHANGE__ Database Structure: Field added in ImportDatabas
  Update all your clients at once to avoid issues between *3 and *4

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

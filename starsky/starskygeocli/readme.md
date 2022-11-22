# Geo Cli
## List of [Starsky](../../readme.md) Projects
 * [By App documentation](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * __[starskyGeoCli](../../starsky/starskygeocli/readme.md)  gpx sync and reverse 'geo tagging'__
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskydesktop](../../starskydesktop/readme.md) _Desktop Application_
    * [Download Desktop App](https://qdraw.github.io/starsky/assets/download/download.html) _Windows and Mac OS version_
 * [Changelog](../../history.md) _Release notes and history_

## Geo Sync Options

### Introduction gpx tagging
When your camera has no GPS support build in you can use your mobile phone to keep your track. This application can combine a list of your recent locations and when the photo has taken into a location. When you have a GPX track file and a photo you can know where it is taken. With various sport track apps support exporting gpx files.

#### Important things to know
- Your camera date and time has to be correct.
- Only gpx track files are supported (no 'way points' or routes).
- All 'track points' should have a latitude, longitude, elevation and time in UTC.
- You need to add your `CameraTimeZone` name to the Starsky configuration.
- Gpx 'track points' more than 5 minutes difference are ignored.
- 'Track points' less that 5 minutes difference are using the closest point
- All gpx files in the selected folder are combined and used.

### Introduction reverse 'geo tagging'
To add the nearest city, state and country to the already 'geo tagged' file use reverse 'geo tagging'

#### Important things to know
- When the reverse 'geo tagged' item is less that 40 kilometers from that place add it to the file
- Uses GeoNames.org data to for all cities with a population > 1000 or seats of adm div (ca 150.000)


### Geo Cli Help window
```sh
Starksy Geo Cli ~ Help:
--help or -h == help (this window)
--path or -p == parameter: (string) ; fullpath (all locations are supported)
--subpath or -s == parameter: (string) ; relative path in the database
--subpathrelative or -g == Overwrite subpath to use relative days to select a folder, use for example '1' to select yesterday. (structure is required)
-p, -s, -g == you need to select one of those tags
--all or -a == overwrite reverse geotag location tags (default: false / ignore already taged files)
--index or -i == parameter: (bool) ; gpx feature to index geo location, default true
--verbose or -v == verbose, more detailed info
  use -v -help to show settings:
```

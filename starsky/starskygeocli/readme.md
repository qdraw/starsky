# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to html files_
    * __[starskyGeoCli](../../starsky/starskygeocli/readme.md)  gpx sync and reverse geotagging__
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starskyGeoSync docs

### Introduction gpx tagging
When your camera has no GPS support build in you can use your mobile phone to keep your track.
With various sport track apps support exporting gpx files.

#### Important things to know
- Your camera date and time has to be correct.
- Only gpx track files are supported (no waypoints or routes).
- All trackpoints should have a latitude, longitude, elevation and time in UTC.
- You need to add your `CameraTimeZone` name to the Starsky configuation.
- Gpx waypoints more than 5 minutes difference are ignored.
- Waypoints less that 5 minutes difference are using the closest point
- All gpx files in the selected folder are combined and used.

### Introduction reverse geotagging
To add the nearest city, state and country to the already geotagged file use reverse geotagging

#### Important things to know
- When the reverse geotagged item is less that 40 kilometers from that place add it to the file
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

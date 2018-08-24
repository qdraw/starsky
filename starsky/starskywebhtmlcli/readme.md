# Starsky
## List of [Starsky](../../readme.md) Projects
 - [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux [(files)](../../inotify-settings)_
 - [starsky (sln)](../../starsky/readme.md) _database photo index & import index project [(files)](../../starsky)_
   - [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface [(files)](../../starsky/starsky)_
   - [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface [(files)](../../starsky/starskysynccli)_
   - [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface [(files)](../../starsky/starskyimportercli)_
   - [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests [(files)](../../starsky/starskyTests)_
   - __[starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to html files [(files)](../../starsky/starskywebhtmlcli)___
 - starsky-node-client  _(depreciated)_
 - [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

## starskyWebHtmlCli docs

### starskyWebHtmlCli Help:

#### Example configuration
```json
{
    "app" :{
        "publishProfiles":
            [
                {
                    "ContentType":  "html",
                    "SourceMaxWidth":  0,
                    "OverlayMaxWidth":  0,
                    "OverlayFullPath": "",
                    "Path": "index.html",
                    "Template": "Autopost",
                    "Append": ""
                },
                {
                    "ContentType":  "html",
                    "SourceMaxWidth":  0,
                    "OverlayMaxWidth":  0,
                    "OverlayFullPath": "",
                    "Path": "autopost.txt",
                    "Template": "Autopost",
                    "Append": ""
                },
                {
                    "ContentType":  "jpeg",
                    "SourceMaxWidth":  1000,
                    "OverlayMaxWidth":  200,
                    "Path": "/overlay.jpg",
                    "Folder": "1000",
                    "Append": "_kl1k"
                },
                {
                    "ContentType":  "jpeg",
                    "SourceMaxWidth":  500,
                    "OverlayMaxWidth":  100,
                    "OverlayFullPath": "/overlay.jpg",
                    "Folder": "500",
                    "Append": "_kl"
                }
            ]
    }
}
```
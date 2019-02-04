# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskySyncCli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * __[starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  copy a content package to a ftp service__
    * [starskycore](../../starsky/starskycore/readme.md) _business logic (netstandard 2.0)_
    * [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_
 
 
## starskyWebFtpCli docs

Run the `starskyWebHtmlCli` first, to create a 'content package'
A 'content package' is a folder with static html files and resized images.

### AppSettings 

Use the `publishProfiles` by the `starskyWebHtmlCli` and add the `WebFtp` field.
```json
{
    "app" :{
        "WebFtp": "ftp://username%40qdraw.nl:secret@ftp.qdraw.nl",
        "publishProfiles":
            [
                {
                    "ContentType":  "html",
                    "SourceMaxWidth":  0,
                    "OverlayMaxWidth":  0,
                    "OverlayFullPath": "",
                    "Path": "index.html",
                    "Template": "Index.cshtml",
                    "Prepend": "",
                    "Copy": "true"
                },
                {
                    "ContentType":  "html",
                    "SourceMaxWidth":  0,
                    "OverlayMaxWidth":  0,
                    "OverlayFullPath": "",
                    "Path": "index.web.html",
                    "Template": "Index.cshtml",
                    "Prepend": "https://media.qdraw.nl/log/{name}",
                    "Copy": "true"
                },
                {
                    "ContentType":  "html",
                    "SourceMaxWidth":  0,
                    "OverlayMaxWidth":  0,
                    "OverlayFullPath": "",
                    "Path": "autopost.txt",
                    "Template": "Autopost.cshtml",
                    "Prepend": "https://media.qdraw.nl/log/{name}",
                    "Copy": "true"
                },
                {
                    "ContentType":  "jpeg",
                    "SourceMaxWidth":  1000,
                    "OverlayMaxWidth":  380,
                    "Path": "{AssemblyDirectory}/EmbeddedViews/qdrawlarge.png",
                    "Folder": "1000",
                    "Append": "_kl1k",
                    "Copy": "true"
                },
                {
                    "ContentType":  "jpeg",
                    "SourceMaxWidth":  500,
                    "OverlayMaxWidth":  200,
                    "Path": "{AssemblyDirectory}/EmbeddedViews/qdrawsmall.png",
                    "Folder": "500",
                    "Append": "_kl",
                    "Copy": "true"
                },
                {
                    "ContentType":  "moveSourceFiles",
                    "Folder": "orgineel",
                    "Copy": "false"
                }
            ]
    }
}
```



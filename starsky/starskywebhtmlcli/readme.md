# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux [(files)](../../inotify-settings)_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project [(files)](../../starsky)_
    * [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface [(files)](../../starsky/starsky)_
    * [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface [(files)](../../starsky/starskysynccli)_
    * [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface [(files)](../../starsky/starskyimportercli)_
    * [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests [(files)](../../starsky/starskyTests)_
    * __[starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  publish web images to html files [(files)](../../starsky/starskywebhtmlcli)__
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks [(files)](../../starsky-node-client)_
 * [starskyapp](../../starskyapp) _React-Native app (Pre-alpha code)_

## starskyWebHtmlCli docs

### starskyWebHtmlCli Help:

This application is used to create thumbnail web images and prerender html files.
In the `appsettings.json` is used to setup the publish actions.
The application loops though `publishProfiles` in `appsettings.json`.

#### ContentType
There are options to do predefined tasks
- `html`, uses razor to generate html files
- `jpeg`, resizes images to smaller files
- `moveSourceFiles`, move action to child folder

#### SourceMaxWidth
The width in pixels of the output image. This is used only for ContentType `jpeg`.

#### OverlayMaxWidth
The width of the overlay image (you can add a logo as overlay) over the output image.
This is used only for ContentType `jpeg`.

#### Path
When using ContentType `html` this is the filename of the rendered html file.
With ContentType `jpeg`, this is the full filepath of the image used in `OverlayMaxWidth`

#### Template
Used with ContentType `html` to select the Razor template file

#### Prepend
In ContentType `html` this is used to add text before the urls used in the html output

#### Append
In ContentType `jpeg` this used to add text after the current filename

#### Folder
When using ContentType `jpeg` there are child folders created with this name.
In the example there are subfolders created with names 1000 and 500.
In ContentType `moveSourceFiles` this is the folder to move the file to.


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
                    "Template": "Index.cshtml",
                    "Prepend": ""
                },
                {
                    "ContentType":  "html",
                    "SourceMaxWidth":  0,
                    "OverlayMaxWidth":  0,
                    "OverlayFullPath": "",
                    "Path": "index.web.html",
                    "Template": "Index.cshtml",
                    "Prepend": "https://media.qdraw.nl/log/{name}"
                },
                {
                    "ContentType":  "html",
                    "SourceMaxWidth":  0,
                    "OverlayMaxWidth":  0,
                    "OverlayFullPath": "",
                    "Path": "autopost.txt",
                    "Template": "Autopost.cshtml",
                    "Prepend": "https://media.qdraw.nl/log/{name}"
                },
                {
                    "ContentType":  "jpeg",
                    "SourceMaxWidth":  1000,
                    "OverlayMaxWidth":  380,
                    "Path": "/data/git/starsky/starsky/starskywebhtmlcli/EmbeddedViews/qdrawlarge.png",
                    "Folder": "1000",
                    "Append": "_kl1k"
                },
                {
                    "ContentType":  "jpeg",
                    "SourceMaxWidth":  500,
                    "OverlayMaxWidth":  200,
                    "Path": "/data/git/starsky/starsky/starskywebhtmlcli/EmbeddedViews/qdrawsmall.png",
                    "Folder": "500",
                    "Append": "_kl"
                },
                {
                    "ContentType":  "moveSourceFiles",
                    "Folder": "orgineel"
                }
            ]
    }
}
```

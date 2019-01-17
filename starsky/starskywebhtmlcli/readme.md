# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md)  _mvc application / web interface_
    * [starskycore](../../starsky/starskycore/readme.md) _business logic (netstandard2.0)_
    * [starskysynccli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyimportercli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests_
    * __[starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  publish web images to html files__
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starskyWebHtmlCli docs

### Introduction:

For example to generate content for a blog, the 'Web HTML Cli' can be used. This application is used to create thumbnail web images and 'pre render' html files. 
All actions are customizable in the `appsettings.json`. There is a section called `publishProfiles` in `appsettings.json`.
The `publishProfiles` are executed during runtime.

### The StarskyWebHtmlCli --Help window:
```sh
Starksy WebHtml Cli ~ Help:
--help or -h == help (this window)
--path or -p == parameter: (string) ; fullpath (select a folder)
--name or -n == parameter: (string) ; name of blogitem 
--verbose or -v == verbose, more detailed info
  use -v -help to show settings: 
```
### Configuration

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
With ContentType `jpeg`, this is the 'full file path' of the image used in `OverlayMaxWidth`

#### Template
Used with ContentType `html` to select the Razor template file

#### 'Pre pend'
In ContentType `html` this is used to add text before the urls used in the html output

#### Append
In ContentType `jpeg` this used to add text after the current filename

#### Folder
When using ContentType `jpeg` there are child folders created with this name.
In the example there are 'sub folders' created with names 1000 and 500.
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

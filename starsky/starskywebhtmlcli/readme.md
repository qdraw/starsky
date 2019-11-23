# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskySyncCli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * __[starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  publish web images to a content package__
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyCore](../../starsky/starskycore/readme.md) _business logic (netstandard 2.0)_
    * [starskyGeoCore](../../starsky/starskygeocore/readme.md) _business geolocation logic (netstandard 2.0)_    
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starskyWebHtmlCli docs

### Introduction:

For example to generate content for a blog, the 'Web HTML Cli' can be used. This application is used to create thumbnail web images and 'pre render' html files.
All actions are customizable in the `appsettings.json`. There is a section called `publishProfiles` in `appsettings.json`.
The `publishProfiles` are executed during runtime.


### To the help dialog:
```sh
./starskywebhtmlcli --help
```

### The StarskyWebHtmlCli --Help window:

```sh
Starksy WebHtml Cli ~ Help:
--help or -h == help (this window)
--path or -p == parameter: (string) ; fullpath (select a folder), use '-p' for current directory
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


__Replacer in Path__

There is option to replace the `{AssemblyDirectory}` value with the path of the starsky assemblies.
This is __not__ using `AppSettings.BaseDirectoryProject` but the assemblies inside StarskyWebHtmlCli

#### Template

Used with ContentType `html` to select the Razor template file

#### 'Pre pend'

In ContentType `html` this is used to add text before the urls used in the html output.


__Replacer in 'Pre pend'__

There is option to replace the `{Name}` value with the slug-name of the item. A slug name is the name in lowercase and the spaces are replaced with dashes.


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

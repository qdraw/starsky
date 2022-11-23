# Web FTP CLI
## List of [Starsky](../../readme.md) Projects
 * [By App documentation](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * __[starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  copy a content package to a ftp service__
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskydesktop](../../starskydesktop/readme.md) _Desktop Application_
    * [Download Desktop App](https://docs.qdraw.nl/download/) _Windows and Mac OS version_
 * [Changelog](../../history.md) _Release notes and history_

## starskyWebFtpCli docs

Copy a content package to a ftp server. Run the `starskyWebHtmlCli` first, to create a 'content package'
A 'content package' is a folder with static html files and resized images.

### AppSettings

Use the `publishProfiles` by the `starskyWebHtmlCli` and add the `WebFtp` field.
```json
{
    "app" :{
        "WebFtp": "ftp://username%40qdraw.nl:secret@ftp.qdraw.nl",
        "publishProfiles": {
            "_default": [
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
                    "Path": "{AssemblyDirectory}/WebHtmlPublish/EmbeddedViews/qdrawlarge.png",
                    "Folder": "1000",
                    "Append": "_kl1k",
                    "Copy": "true"
                },
                {
                    "ContentType":  "jpeg",
                    "SourceMaxWidth":  500,
                    "OverlayMaxWidth":  200,
                    "Path": "{AssemblyDirectory}/WebHtmlPublish/EmbeddedViews/qdrawsmall.png",
                    "Folder": "500",
                    "Append": "_kl",
                    "Copy": "true"
                },
                {
                    "ContentType":  "moveSourceFiles",
                    "Folder": "orgineel",
                    "Copy": "false"
                },
                {
                    "ContentType":  "publishContent",
                    "Folder": "",
                    "Copy": "true"
                },
                {
                    "ContentType": "publishManifest",
                    "Folder": "",
                    "Copy": "true"
                }
            ],
            "no_logo_2000px": [
                {
                    "ContentType":  "jpeg",
                    "SourceMaxWidth":  2000,
                    "OverlayMaxWidth":  0,
                    "Folder": "2000",
                    "Append": "_kl2k",
                    "Copy": "true"
                }
            ]
        }
    }
}
```

# Business Logic

## List of [Starsky](../../readme.md) Projects

* [By App documentation](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
        * [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line
      interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a
      content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp
      service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes
      are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by
      generating smaller images_
    * __[Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) business logic
      libraries (.NET)__
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests (for .NET)_
* [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
* [Starsky Desktop](../../starskydesktop/readme.md) _Desktop Application_
    * [Download Desktop App](https://docs.qdraw.nl/download/) _Windows and Mac OS version_
* [Changelog](../../history.md) _Release notes and history_

## Starsky Business Logic docs

This is an overview of business logic

## Feature compare table

| Feature                                                 | Present |
|---------------------------------------------------------|---------|
| Anywhere secure access                                  | ✅       |
| Native iOS and Android mobile apps                      | ❌       |
| Mac OS and Windows Desktop app                          | ✅       |
| Access controls with permissions and roles              | ✴️      |
| User generation by Command Line                         | ✅       |
| Customized branded login page                           | ❌       |
| Out-of-the-box access from the web  (when hosted)       | ✅       |
| SaaS solution                                           | ❌       |
| Multi tenant support                                    | ❌       |
| Bulk metadata upload via CSV                            | ❌       |
| Bulk metadata edit via web interface                    | ✅       |
| Review, approve and publish uploads                     | ❌       |
| Batch or single file download                           | ✅       |
| Download permissions based on role                      | ❌       |
| Request access to file form                             | ❌       |
| Supports photos jpg, png, gif, tiff                     | ✅       |
| Supports video mp4 (H.264)                              | ✅       |
| Supports audio                                          | ❌       |
| Supports IPTC, EXIF and XMP metadata                    | ✅       |
| All major browsers supported (Chrome, Safari, Mozilla)  | ✅       |
| Internet Explorer support                               | ❌       |
| In-line editing in fields                               | ✅       |
| Localized platform English and Dutch                    | ✅       |
| Host the server version yourself using docker           | ✅       |
| Host the server version yourself on a Windows/Mac/Linux | ✅       |

| Icon | Meaning of icon       |
|------|-----------------------|
| ✅    | fully implemented     |
| ✴️   | is partly implemented |
| ❌    | not implemented       |

## Project structure

```
•
└── CommandLineInterface
|   └── starskyadmincli
|   └── starskygeocli
|   └── starskyimportercli
|   └── starskysynchronizecli
|   └── starskythumbnailcli
|   └── starskywebftpcli
|   └── starskywebhtmlcli
└── Feature
|   |       The Feature layer contains concrete features of the solution as understood by the business owners and editors of the solution, for example news, articles.
|   └── starsky.feature.export
|   |     Exporting list of files to zip archive
|   └── starsky.feature.geolookup
|   |     Looking up Geo Location from folders
|   └── starsky.feature.health
|   |     Health API to check database and dependencies
|   └── starsky.feature.import
|   |     Import and move to the right folder
|   └── starsky.feature.metaupdate
|   |     Update metadata photos/items on disk and in the database
|   └── starsky.feature.rename
|   |     Rename photos/items on disk and in the database
|   └── starsky.feature.webhtmlpublish
|   |     Copy webhtmlpublish-ed items to an ftp server
|   └── starsky.feature.webhtmlpublish
|         Generate html content with photos.
|   └── starsky.feature.realtime
|         Real-time features
|   └── starsky.feature.syncbackground
|         Background synchronization features
|   └── starsky.feature.search
|         Search features
|   └── starsky.feature.thumbnail
|         Thumbnail-related features
|   └── starsky.feature.settings
|         Settings features
|   └── starsky.feature.demo
|         Demo features
└── Foundation
|   |       Modules in the Foundation layer are conceptually abstract and do not contain presentation in the form of renderings or views
|   └── starsky.foundation.accountmanagement
|   |     Abstraction layer of User Mangement
|   └── starsky.foundation.database
|   |     EF Core abstractions and database mapping
|   └── starsky.foundation.http
|   |     To Get/Post to other API's
|   └── starsky.foundation.injection
|   |     Do dependency injection with a [Service]-tag
|   └── starsky.foundation.platform
|   |     Platform configuration, file name helpers, console abstractions, argument helpers and enum extensions
|   └── starsky.foundation.readmeta
|   |     Reading XMP, Exif, GPX and Video meta-data from files
|   └── starsky.foundation.realtime
|   |     WebSockets Middleware
|   └── starsky.foundation.storage
|   |     Filesystem abstractions
|   └── starsky.foundation.sync
|   |     Compare disk with database
|   └── starsky.foundation.thumbnailgeneration
|   |     Generate Thumbnails
|   └── starsky.foundation.writemeta
|         Write though Exiftool
|   └── starsky.foundation.worker
|         Worker-related functionalities
|   └── starsky.foundation.settings
|         Settings-related functionalities
|   └── starsky.foundation.native
|         Native OS-related functionalities
|   └── starsky.foundation.thumbnailmeta
|         Thumbnail meta-related functionalities
└── Project
    |    This means the actual cohesive website or channel output from the implementation, such as the page types, layout and graphical design  
    └── starsky.project.web
    |    Services and helpers needed for the web application, but not for other applications 
    └── starsky
          WebAPI presentation application (see ClientApp for more details about the UI)
```

# Starsky Business Logic
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskySyncCli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * __[Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) business logic libraries (netstandard 2.0)__
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _Desktop Application (Pre-alpha code)_

## Starsky Business Logic docs


## Project structure

```
•
└── CommandLineInterface
|   └── starskyadmincli
|   └── starskygeocli
|   └── starskyimportercli
|   └── starskysynccli
|   └── starskywebftpcli
|   └── starskywebhtmlcli
└── Feature
|   |       The Feature layer contains concrete features of the solution as understood by the business owners and editors of the solution, for example news, articles.
|   └── starsky.feature.geolookup
|   |     Looking up Geo Location from folders
|   └── starsky.feature.import
|   |     Import and move to the right folder
|   └── starsky.feature.webhtmlpublish
|         Generate html content with photos.
└── Foundation
|   |       Modules in the Foundation layer are conceptually abstract and do not contain presentation in the form of renderings or views 
|   └── starsky.foundation.database
|   |     EF Core abstractions
|   └── starsky.foundation.http
|   |     To Get from other API's
|   └── starsky.foundation.injection
|   |     Do dependency injection with a [Service]-tag
|   └── starsky.foundation.platform
|   |     Platform configuration, file name helpers, console abstractions, argument helpers and enum extensions
|   └── starsky.foundation.readmeta
|   |     Reading XMP, Exif, GPX and Video meta-data from files
|   └── starsky.foundation.storage
|   |     Filesystem abstractions
|   └── starsky.foundation.thumbnailgeneration
|   |     Generate Thumbnails
|   └── starsky.foundation.writemeta
|         Write though Exiftool
└── Project
    |    This means the actual cohesive website or channel output from the implementation, such as the page types, layout and graphical design  
    └── starskycore
    |    To be depricated and replaced with feature and foundation services  
    └── starsky
          WebAPI presentation application
```
# Starsky
## List of [Starsky](../../readme.md) Projects
 * [inotify-settings](../../inotify-settings/readme.md) _to setup auto indexing on linux_
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _mvc application / web interface_
    * [starskySyncCli](../../starsky/starskysynccli/readme.md)  _database command line interface_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * __[starskycore](../../starsky/starskycore/readme.md) business logic (netstandard 2.0)__
    * [starskyTests](../../starsky/starskyTests/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines_
 * [starsky-node-client](../../starsky-node-client/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _React-Native app (Pre-alpha code)_

## starsky/starskycore docs

Features that are included in starskycore
- Manage the database connection, using EntityFrameworkCore 
- Convert Base32 and Base64 hashes
- Exiftool bindings
- Check if a file is a folder
- Calc Geo Distance
- extension to Mime type
- Pbkdf2Hasher for passwords
- Rename files in sync with a database
- Middleware for MVC applications
  - Content Security Policy
  - Basic Authentication header
- Models for example FileIndexItem
- Background service to use in a MVC Application
- Creating Breadcrumbs
- Query service to handle caching in the database
- Import service to auto sort photos
- Reading XMP and IPTC meta data from photos
- Sync the filesystem with a database
- Handle User accounts
- Generate Thumbnails

The starskycore is used to embed in the other applications, it contains the most important business logic. This assembly is `.NETStandard2.0` and can be used with '.NET Core' and '.NET Framework 4.7'  

There are dependecies on:
- MetadataExtractor
- Microsoft.ApplicationInsights
- Microsoft.Extensions.Configuration
- SixLabors.ImageSharp
- TimeZoneConverter
- XmpCore
- Pomelo.EntityFrameworkCore.MySql
- Microsoft.EntityFrameworkCore
- System.ComponentModel.Annotations
- Microsoft.AspNetCore.Identity
- System.Buffers
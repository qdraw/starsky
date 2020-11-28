# Starsky Thumbnail Cli
## List of [Starsky](../../readme.md) Projects
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * [starskyAdminCli](../../starsky/starskyadmincli/readme.md)  _manage user accounts_
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * __[starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  speed web performance by generating smaller images__
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines (deprecated)_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _Desktop Application (Pre-alpha code)_

## starskyThumbnailCli docs

Run a cron task to speed web performance by generating smaller images.

Use the following argument: `-t true` and specify the path. This can be done by providing `-p`, then you enter an absolute path or by `-s`, after which you indicate a relative path
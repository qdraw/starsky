# Starsky Admin Cli
## List of [Starsky](../../readme.md) Projects
 * [starsky (sln)](../../starsky/readme.md) _database photo index & import index project_
    * [starsky](../../starsky/starsky/readme.md) _web api application / interface_
      *  [clientapp](../../starsky/starsky/clientapp/readme.md) _react front-end application_
    * [starskyImporterCli](../../starsky/starskyimportercli/readme.md)  _import command line interface_
    * [starskyGeoCli](../../starsky/starskygeocli/readme.md)  _gpx sync and reverse 'geo tagging'_
    * [starskyWebHtmlCli](../../starsky/starskywebhtmlcli/readme.md)  _publish web images to a content package_
    * [starskyWebFtpCli](../../starsky/starskywebftpcli/readme.md)  _copy a content package to a ftp service_
    * __[starskyAdminCli](../../starsky/starskyadmincli/readme.md)  manage user accounts__
    * [starskySynchronizeCli](../../starsky/starskysynchronizecli/readme.md)  _check if disk changes are updated in the database_
    * [starskyThumbnailCli](../../starsky/starskythumbnailcli/readme.md)  _speed web performance by generating smaller images_
    * [Starsky Business Logic](../../starsky/starskybusinesslogic/readme.md) _business logic libraries (netstandard 2.0)_
    * [starskyTest](../../starsky/starskytest/readme.md)  _mstest unit tests_
 * [starsky.netframework](../../starsky.netframework/readme.md) _Client for older machines (deprecated)_
 * [starsky-tools](../../starsky-tools/readme.md) _nodejs tools to add-on tasks_
 * [starskyapp](../../starskyapp/readme.md) _Desktop Application_
 * [Changelog](../../history.md) _Release notes and history_

## Options for the Admin CLI

### Account creation
This first question is 'What is the username/email?'
When you enter an email address that is new you will able to register that account
```
We are going to create an account.
What is the password?
```

### Update existing account
Currently you can only delete accounts by email address or toggle a user role between normal user and admin user

The Application ask the following question:
```
What is the username/email?
```

```
Do you want to
2. remove account
3. Toggle User Role
```

> Please note. When you toggle a User Role, in the web interface you need to logout and login again

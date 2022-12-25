# Config Options

 Changing values in `docker-compose.yml` or in Advanced Settings always **requires a restart** to take effect. Open a terminal, run `docker compose stop` and then
  `docker compose up -d` to restart all services.


## Web Application
There are a few options that can be changed in the web application. 

These options are available though `appsettings.json` and environment variables.

There are no mandatory options, but its recommended to change the storageFolder
to a folder on your local machine where the picture should be located.

Environment variables are always preferred over `appsettings.json` values.
and should prefix with `app__` and replace `:` with `__` and `.` with `_`.
So `app__databaseType` is the same as `"app":{"databaseType":"mysql"}` in `appsettings.json`.

- [See advanced configuration options for the web application](../advanced-options/starsky/starsky/readme.md#recommend-settings)

# Command line options

There are separate command line applications that target the specific needs.
See the [Advanced options](../advanced-options/starsky/readme.md) for more information.

Add the command line argument `--help` option to see all available options.
The options are configured in `appsettings.json` and environment variables and command line arguments.


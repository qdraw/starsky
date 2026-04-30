---
sidebar_position: 1
---

# Overview Config Options

Changing values in `docker-compose.yml` or in Advanced Settings always **requires a restart** to
take effect. Open a terminal, run `docker compose stop` and then
`docker compose up -d` to restart all services.

## Web Application

There are a few options that can be changed in the web application.

These options are available though `appsettings.json` and environment variables.

There are no mandatory options, but it's recommended to change the storageFolder
to a folder on your local machine where the picture should be located.

Environment variables are always preferred over `appsettings.json` values.
and should prefix with `app__` and replace `:` with `__` and `.` with `_`.
So `app__databaseType` is the same as `"app":{"databaseType":"mysql"}` in `appsettings.json`.

- [See advanced configuration options for the web application](../../advanced-options/starsky/starsky/readme.md#recommend-settings)

# Command line options

There are separate command line applications that target the specific needs.
See the [Advanced options](../../advanced-options/starsky/readme.md) for more information.

Add the command line argument `--help` option to see all available options.
The options are configured in `appsettings.json` and environment variables and command line
arguments.

## Manual Overwrite settings for Desktop

If you want to manually overwrite the settings, you can use the `appsettings.local.json` file.
This file is located for macOS `~/Library/Application Support/starsky/appsettings.local.json`
and Windows `C:\Users\<username>\AppData\Local\starsky\appsettings.local.json`.
When using the desktop app the environment variable `app__AppSettingsLocalPath` is used

## Configuration overwrite order

Starsky reads configuration files and environment variables in a fixed order — later items override earlier ones. Use this order when you need to know which value wins when the same key is present in multiple places:

1. `appsettings.json` (project folder)
2. `appsettings.default.json` (project folder)
3. `appsettings.patch.json` (project folder)
4. `appsettings.{machineName}.json` (machine-specific)
5. `appsettings.{machineName}.patch.json` (machine-specific patch)
6. Environment variable: `app__appsettingspath` (path to an `appsettings.json` to load)
7. Environment variable: `app__appsettingslocalpath` (path to an `appsettings.local.json` to load)
8. Specific environment variables (for example `app__storageFolder`, `app__databaseConnection`, etc.)

Notes:
- Environment variables use the `app__` prefix and replace `:` with `__` and `.` with `_` (for example `app__databaseType`).
- Environment variables and the `app__appsettings*` paths are a convenient way to override settings without editing files.

### Import Backup (example)

The Import Backup feature can be configured either via environment variables or in `appsettings.json`. This is an example, it can be used with all features in the appSettings

Environment variable example:

```json
"app__ImportBackup__Enabled": "true",
"app__ImportBackup__StorageFolder": "/data/backup_camera/1/"
```

Or `appsettings.json` section:

```json
"ImportBackup": {
	"Enabled": "false",
	"StorageFolder": ""
}
```

When enabled, imported files are copied to the configured storage folder using a fixed filename structure (for example `20260326_082007_Schermafbeelding-2026-03-26-om-082007.png`).

For more details see the Import documentation: [Import Backup](../../features/import/import.md#import-backup)


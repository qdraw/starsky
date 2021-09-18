[< starsky/starsky-tools docs](../readme.md)

# Dropbox Import

Imports from Dropbox to a local Starsky installation

## Create a new app on the DBX Platform

See :https://www.dropbox.com/developers/apps/create

-   Scoped access (1 option)
-   Select: Full Dropbox

In the next screen you will see and need this:
App key = DROPBOX_CLIENT_ID
App secret = DROPBOX_CLIENT_SECRET

## Run Dropbox setup to get an refresh token

```
node  dropbox-setup.js
```

Copy the url that is shown in the script and login to dropbox

And copy the values at the end of script to the .env file

## Edit: `.env`

```sh
DROPBOX_CLIENT_ID=??
DROPBOX_CLIENT_SECRET=??
DROPBOX_REFRESH_TOKEN=??
STARSKYIMPORTERCLI=/opt/starsky/starskyimportercli
```

## install

```sh
npm ci
```

## run

```sh
npm run start
```

And ready :)

## Command line args settings

The following options can be specified

```
--path or -p > to specify the path in the dropbox
--colorclass > as import option int between 0-8 (no string or name)
--structure > where to store in the database, make sure you use the right pattern
escaping structure: use 3 escape characters e.g. \\\\\\d.ext
```

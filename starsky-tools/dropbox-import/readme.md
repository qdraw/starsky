[< starsky/starsky-tools docs](../readme.md)

# Dropbox Import

Imports from Dropbox to a local Starsky installation

## Edit: `.env`
```sh
DROPBOX_ACCESSTOKEN=
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

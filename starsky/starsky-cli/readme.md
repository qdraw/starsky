# Starsky Indexer Help:
The goal of this wrapper is to get command line access to the photo index database

## To get help:
```sh
starskycli --help
```

## The StarskyCli --Help window:
```
--help or -h == help (this window)
--subpath or -s == parameter: (string) ; path inside the index, default '/'
--path or -p == parameter: (string) ; fullpath, search and replace first part of the filename '/'
--index or -i == parameter: (bool) ; enable indexing, default true
--thumbnail or -t == parameter: (bool) ; enable thumbnail, default false
--orphanfolder or -o == To delete files without a parent folder (heavy cpu usage), default false
--verbose or -v == verbose, more detailed info
--databasetype or -d == Overwrite EnvironmentVariable for DatabaseType
--basepath or -b == Overwrite EnvironmentVariable for STARSKY_BASEPATH
--connection or -c == Overwrite EnvironmentVariable for DefaultConnection
--thumbnailtempfolder or -f == Overwrite EnvironmentVariable for ThumbnailTempFolder
--exiftoolpath or -e == Overwrite EnvironmentVariable for ExifToolPath
  use -v -help to show settings:
```

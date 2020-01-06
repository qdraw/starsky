[< starsky/starsky docs](readme.md)

# Rest Api docs

The autorisation using the rest api is done though Basic Auth or Cookie Auth.

__This list is outdated please check Swagger for latest version__

## Rest API Table of contents

- [Get PageType	Archive](#get-pagetype-archive)
- [Get PageType DetailView](#get-pagetype-detailview)
- [Exif Info](#exif-info)
- [Exif Update](#exif-update)
- [Rename](#rename)
- [File Delete](#file-delete)
- [Thumbnail](#thumbnail)
- [Thumbnail Json](#thumbnail-json)
- [Download Photo](#download-photo)
- [Direct import](#direct-import)
- [Form import](#form-import)
- [Import Exif Overwrites (shared feature)](#import-exif-overwrites-shared-feature)
- [Search](#search)
- [Remove cache](#remove-cache)
- [Environment info](#environment-info)
- [SubpathRelative Redirect](#subpathRelative-redirect)

## Get PageType Archive
Endpoint `/starsky/?f=/&json=true`
For browsing the folders. Please use  `"pageType": "Archive"` to check the page type.
The querystring name `f` is used for the file path in releative/subpath style
The status: Default, is not used in this view, the page view is a cached database view of the content

>> Escape the `+`-sign with `%2B` to avoid 404 results

```json
{
    "uri":"/starsky/?f=/&json=true",
    "method":"GET",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```
### Expected `/starsky/?f=/&json=true` response:
```json
{
  "fileIndexItems": [
    {
        "filePath": "/2018/01-dif/__crashy_20180721_222159__i kopie 4.jpg",
        "fileName": "__crashy_20180721_222159__i kopie 4.jpg",
        "fileHash": "XSY2XKG3LYGTWCKZQZCVURQS5Y",
        "fileCollectionName": "__crashy_20180721_222159__i kopie 4",
        "parentDirectory": "/2018/01-dif",
        "isDirectory": false,
        "keywords": [
            "test",
            "test2"
        ],
        "tags": "test, test2",
        "status": "Default",
        "description": "the description",
        "title": "Title",
        "dateTime": "2018-07-21T22:21:59",
        "addToDatabase": "2018-09-20T18:18:33.341613",
        "latitude": 0,
        "longitude": 0,
        "locationAltitude": 0,
        "locationCity": "",
        "locationState": "",
        "locationCountry": "",
        "colorClass": 7,
        "orientation": "Horizontal",
        "imageWidth": 960,
        "imageHeight": 596,
        "imageFormat": "jpg",
        "collectionPaths": []
        }
  ],
  "breadcrumb": [
    "/"
  ],
  "relativeObjects": {
    "nextFilePath": "/20180123_132404.jpg",
    "prevFilePath": "/2013"
  },
  "searchQuery": "2018",
  "pageType": "Archive",
  "subPath": "/"
}
```


## Get PageType DetailView
Api to get fast meta data about the picture.
- The querystring name `f` is used for the file path in releative/subpath style
Endpoint: `/starsky/?f=/image.jpg`
```json
{
    "uri":"/starsky/?f=/image.jpg&json=true",
    "method":"GET",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```

### Expected `/starsky/?f=/2018/t/image.dng` response:

```json
{
    "fileIndexItem": {
            "filePath": "/2018/01-dif/20180908_110315__DSC04577.jpg",
            "fileName": "20180908_110315__DSC04577.jpg",
            "fileHash": "6BNBOZ4T73C523F37EK43XQLPU",
            "fileCollectionName": "20180908_110315__DSC04577",
            "parentDirectory": "/2018/01-dif",
            "isDirectory": false,
            "keywords": [
                ""
            ],
            "tags": "",
            "status": "Default",
            "description": "",
            "title": "Dwingeloo",
            "dateTime": "2018-09-08T11:03:15",
            "addToDatabase": "2018-09-21T13:33:50.28786",
            "latitude": 52.8120277777,
            "longitude": 6.3959916666,
            "locationAltitude": 5,
            "locationCity": "Dwingeloo",
            "locationState": "Drenthe",
            "locationCountry": "Nederland",
            "colorClass": 0,
            "orientation": "Rotate270Cw",
            "imageWidth": 3872,
            "imageHeight": 2576,
            "imageFormat": "jpg",
            "collectionPaths": [
                "/2018/01-dif/20180908_110315__DSC04577.jpg"
            ]
    },
    "relativeObjects": {
        "nextFilePath": null,
        "prevFilePath": null
    },
    "breadcrumb": [
        "/",
        "/2018",
        "/2018/t"
    ],
    "colorClassFilterList": [],
    "pageType": "DetailView",
    "isDirectory": false
}
```

## Exif Info
Get the current exif status of a file, only for files that are exist and editable.
Endpoint: `/starsky/Api/Info?f=/image.jpg` The querystring name `f` is used for file path's

```json
{
    "uri":"/starsky/Api/Info?f=/image.jpg",
    "method":"GET",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```
### Expected `/starsky/Api/Info?f=/image.jpg` response:
- Statuscode 203 with the content `read only` when readonly mode is active
- Uses a different scheme that is used by exiftool
- The querystring name `f` is used for the file path in releative/subpath style
- The querystring can be seperated by a `;`
- Ignore the `Prefs`-tag those are used to set `ColorClass` with Exiftool
- Uses  `Status` to tell if a file exist in the database
    - `Ok` is file loaded
    - `NotFoundNotInIndex` File does not exist in index
    - `NotFoundSourceMissing` The source file is missing
    - `ReadOnly` not allowed to overwrite this file
- Escape the `+`-sign with `%2B` to avoid 404 results

The response by the info request
```json
[
    {
            "id": -1,
            "filePath": "/2018/01-dif/20180705_160335_DSC01991 kopie 2.jpg",
            "fileName": "20180705_160335_DSC01991 kopie 2.jpg",
            "fileHash": "UKLH326XKEXI6A2ITYP7X3ST6Q",
            "fileCollectionName": "20180705_160335_DSC01991 kopie 2",
            "parentDirectory": "/2018/01-dif",
            "isDirectory": false,
            "tags": "test, 123",
            "status": "Ok",
            "description": "t6",
            "title": "some title",
            "dateTime": "2018-07-05T16:03:35",
            "addToDatabase": "0001-01-01T00:00:00",
            "latitude": 0,
            "longitude": 0,
            "colorClass": 0,
            "orientation": "Rotate180",
            "imageWidth": 5456,
            "imageHeight": 3632,
            "imageFormat": "jpg",
            "collectionPaths": []
        },
        {
            "id": -1,
            "filePath": "/2018/01-dif/20180705_160335_DSC01991 kopie 2.arw",
            "fileName": "20180705_160335_DSC01991 kopie 2.arw",
            "fileHash": "IF4BWCGCTQXGBOF4F4HJSDCAVI",
            "fileCollectionName": "20180705_160335_DSC01991 kopie 2",
            "parentDirectory": "/2018/01-dif",
            "isDirectory": false,
            "tags": "test, 123",
            "status": "Ok",
            "description": "t3",
            "title": "object name",
            "dateTime": "2018-07-05T16:03:35",
            "addToDatabase": "0001-01-01T00:00:00",
            "latitude": 0,
            "longitude": 0,
            "colorClass": 0,
            "orientation": "DoNotChange",
            "imageWidth": 5504,
            "imageHeight": 3656,
            "imageFormat": "tiff",
            "collectionPaths": []
        }
]
```
### Colorclass types
- Colorclass is a enum, and the values are:      
```cs
case "0":
    _colorClass = Color.None; // No color
case "8":
    _colorClass = Color.Trash; // Grey
case "7":
    _colorClass = Color.Extras; // Blue
case "6":
    _colorClass = Color.TypicalAlt; // Turquoise
case "5":
    _colorClass = Color.Typical; // Green
case "4":
    _colorClass = Color.SuperiorAlt; // Yellow
case "3":
    _colorClass = Color.Superior; // Orange
case "2":
    _colorClass = Color.WinnerAlt; // Red
case "1":
    _colorClass = Color.Winner; // Purple
```
### Rotation types (only for reference)
```cs
public enum Rotation
{
    DoNotChange = 0,
    Horizontal = 1,
    Rotate90Cw = 6,
    Rotate180 = 3,  
    Rotate270Cw = 8
}
```
Check for more types: https://www.daveperrett.com/articles/2012/07/28/exif-orientation-handling-is-a-ghetto/


## Exif Update
To update please request first [Exif Info](#exif-info).
Endpoint: `/starsky/Api/Update?f=/image.jpg`

### Supported types:
_Defined in the class `ExifToolCmdHelper`_
- Tags
- Description
- Title
- ColorClass (use integer value)
- Latitude (in decimal degrees)
- Longitude (in decimal degrees)
- LocationAltitude (in meters)
- LocationCity
- LocationState
- LocationCountry
- `rotateClock` is using `-1` or `1` to rotate the image relative to the current rotation tag
    -   `-1` is rotate image 270 degrees
    -   `1` is rotate image 90 degrees
- DateTime _(use this format: &DateTime=2018-05-05T16:03:35)_
    - There is no timezone support

### Notes
- The querystring name `f` is used for the file path in releative/subpath style
- The querystring support `;` file sepeartion for selecting multiple files
- Empty tags are always ignored
- Stack collections or Collections is a feature to update multiple files with the same name _(before the extension)_.

```json
{
    "uri":"/starsky/Api/Update?f=/image.jpg",
    "method":"POST",
    "headers":
    {
       "Content-Type":"application/json"
    },
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```

### Expected `/starsky/Api/Update?f=/image.jpg` response:
```json
[
    {
        "id": 13702,
        "filePath": "/2018/01-dif/20180705_160335_DSC01991 kopie 2.jpg",
        "fileName": "20180705_160335_DSC01991 kopie 2.jpg",
        "fileHash": null,
        "fileCollectionName": "20180705_160335_DSC01991 kopie 2",
        "parentDirectory": "/2018/01-dif",
        "isDirectory": false,
        "keywords": [],
        "tags": "dion, twello, test2",
        "status": "Ok",
        "description": "dion",
        "title": "dion2",
        "dateTime": "0001-01-01T00:00:00",
        "addToDatabase": "0001-01-01T00:00:00",
        "latitude": 0,
        "longitude": 0,
        "colorClass": 0,
        "orientation": "Rotate180",
        "imageWidth": 5456,
        "imageHeight": 3632,
        "imageFormat": "jpg",
        "collectionPaths": [
            "/2018/01-dif/20180705_160335_DSC01991 kopie 2.arw",
            "/2018/01-dif/20180705_160335_DSC01991 kopie 2.jpg"
        ]
    },
    {
        "id": 0,
        "filePath": "/2018/01-dif/20180705_160335_DSC01991 kopie 2.ar1w",
        "fileName": "/20180705_160335_DSC01991 kopie 2.ar1w",
        "fileHash": null,
        "fileCollectionName": "20180705_160335_DSC01991 kopie 2",
        "parentDirectory": "/2018/01-dif",
        "isDirectory": false,
        "keywords": [
            "dion",
            "twello",
            "test2"
        ],
        "tags": "dion, twello, test2",
        "status": "NotFoundNotInIndex",
        "description": "dion",
        "title": "",
        "dateTime": "0001-01-01T00:00:00",
        "addToDatabase": "0001-01-01T00:00:00",
        "latitude": 0,
        "longitude": 0,
        "colorClass": 0,
        "orientation": -1,
        "imageWidth": 0,
        "imageHeight": 0,
        "imageFormat": "unknown",
        "collectionPaths": []
    }
]
```
-  Statuscode 203. When trying to update a `read only` image. With the content `read only`
-  Error 404 When a image is `not in index`
-  This request returns the complete request, this is using the cached database view to present the latest status
-  To get all latest Info do a request to the Info-endpoint
- This Endpoint uses  `Status` to show if a file is updated. The file is only updated when the status is `Ok`
    - `Ok` is file updated
    - `NotFoundNotInIndex` File does not exist in index and the request failed
    - `NotFoundSourceMissing` The source file is missing and the request failed
    - `ReadOnly` not allowed to overwrite this file and the request failed

## Rename
### Alpha feature > not yet implemented in the front-end
Rename files or folder on disk and in the same request update the database.
Endpoint: `/starsky/Sync/Rename?f=/image.jpg&to=/image2.jpg`

```json
{
    "uri":"/starsky/Sync/Rename?f=/current&to=/future",
    "method":"POST",
    "headers":
    {
       "Content-Type":"application/json"
    },
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```

### Notes
- Statuscodes not implemented yet



## File Delete
To permanent delete a file from the file system and the database.
The tag: `!delete!` is used to mark a file that is in the Trash. This is required by this api.

### Notes
- The querystring name `f` is used for the file path in releative/subpath style
- The querystring support `;` file sepeartion for selecting multiple files
- the file must contain the keyword: !delete! (this is a requimenent)

```json
{
    "uri":"/starsky/Api/Delete?f=/image.jpg",
    "method":"DELETE",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```
### Expected `/starsky/Api/Delete?f=/image.jpg` response:
- Error 404, when it is in `read only` mode
- Error 404, when the file does not exist
- Statuscode 200, when the file is deleted.

## Thumbnail
To get an thumbnail of the image, the thumbnail is 1000 pixels width.
- The querystring after `Thumbnail/` is used for the Base32 hash of the orginal image
- This endpoint supports only 1 file per request
- Only `.jpg` as extension is optional supported (for example: `LNPE227BMTFMQWMIN7BE4X5ZOU.jpg`)


Endpoint: `/starsky/Api/Thumbnail/LNPE227BMTFMQWMIN7BE4X5ZOU`
```json
{
    "uri":"/starsky/Api/Thumbnail/LNPE227BMTFMQWMIN7BE4X5ZOU",
    "method":"GET",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}

```
### Thumbnail querystring options
- There is a orginal (source image) fallback, when using the `?issingleitem=True` query
  The orginal image will be loaded instead of the thumbnail
- The `retryThumbnail` is removing a thumbnail image, only when this Thumbnail is 0 bytes. The query string is  `?retryThumbnail=True`.
  When this is used in combination with  `isSingleItem` a orginal image is loaded.
- Check [Thumbnail Json](#thumbnail-json) for more information
- This endpoint supports only 1 file per request

### Expected `/starsky/Api/Thumbnail` response:
- A jpeg image
- A 204 / `NoContent()` result when a thumbnail is corrupt (or 0 bytes)
- A 404 Error page, when the base32 hash does not exist
- A 202 Error when "Thumbnail is not ready yet". There is no thumbnail generated.

## Thumbnail Json
Endpoint: `/starsky/Api/Thumbnail/LNPE227BMTFMQWMIN7BE4X5ZOU&json=true`
For checking if a thumbnail exist without loading the entire image
- This endpoint supports only 1 file per request

```json
{
    "uri":"/starsky/Api/Thumbnail/LNPE227BMTFMQWMIN7BE4X5ZOU&json=true",
    "method":"GET",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```

### Expected `/starsky/Api/Thumbnail&json=true` response:
- A 200 result with no content if the request is successfull (with the json tag enabled)
- A 204 / `NoContent()` result when a thumbnail is corrupt
- A 404 Error page, when the base32 hash does not exist
- A 202 Error when "Thumbnail is not ready yet"

## Download Photo
To get an orginal or to (re)generate a thumbnail.
- The querystring  `f` is used for the filepath of the orginal image
- When the querystring `isThumbnail` is used `true` a thumbnail we used or generated
- This endpoint supports only 1 file per request

Endpoint: `/starsky/Api/DownloadPhoto?f=/image.jpg`
```json
{
    "uri":"/starsky/Api/DownloadPhoto?f=/image.jpg",
    "method":"GET",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```
### Expected `/starsky/Api/DownloadPhoto` response:
- A thumbnail image with the type `image/jpeg`
- An orginal image or file



## Direct import
For importing using the structure configuration
The filename-header can be added in `base64` or as `string`.
`Content-type` is required, please use `image/jpeg`
- Endpoint: `/starsky/import`
- Import overwrites are disabled by default
- [Check #import-exif-overwrites for info about overwrite (all) imported files](#import-exif-overwrites)

```json
{
    "uri":"/starsky/import",
    "method":"POST",
    "headers":
    {
       "filename":"MjAxOC0wNy0yMCAyMC4xNC41Mi5qcGc="
    },
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    },
    "body":
    {
        "$content-type":"image/jpeg",
        "$content":""
    }
}
```
### Expected `/starsky/import` response:
- Error 206 when the response array is empty. The response array is empty when there are no items added.
- An array with the added items: (with direct input this is always one item)
```json
[
    "/2018/07/2018_07_22/20180722_220442__import_20180720_201452_2018-07-20 20.14.52.jpg"
]
```

## Form import
When using a form, the filename is extracted from the multipart. For the filename there is only string encoding support
- Endpoint: `/starsky/import`
- Import overwrites are disabled by default
- [Check #import-exif-overwrites for info about overwrite (all) imported files](#import-exif-overwrites)

```json
{
    "uri":"/starsky/import",
    "method":"POST",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    },
    "body":
    {
        "$content-type":"multipart/form-data",
        "$content":""
    }
}
```
### Expected `/starsky/import` response:
- Error 206 when the response array is empty. The response array is empty when there are no items added.
- When you have try to add duplicate items, those are not included in the list
- An array with the added items (the list can contain multiple items):
```json
[
    "/2018/07/2018_07_22/20180722_220442.jpg"
]
```

## Import Exif Overwrites (shared feature)
This is a feature that is used by:
- [Direct import](#direct-import)
- [Form import](#form-import)

### Headers
- `ColorClass` accepts an integer between `0` and `8`.
   The values are indicated in [Exif Info](#exif-info)
   This header defaults to `0`
-  `AgeFileFilter` is a filter that ignores files that are older than 2 years.
   By default is this filter enabled
-  `Structure` is a toggles a feature that overwrites the default Structure settings
    It only overwrites when this header has content (So not when it empty or null`)
    > If the structure is incorrect an Application Exception occurs.
      Structure requires to start with a `/` and the filename it must end with `.ext`
      With the [Environment info](#environment-info) `structureExampleNoSetting` value
      you can check the structure right now.


```json
{
    "This Json is missing Authorisation and HTTP-method": true,
    "headers":
    {
       "ColorClass":"1",
       "AgeFileFilter": "false",
       "Structure": "/yyyy/MM/yyyy_MM_dd*/HHmmss_yyyyMMdd_d.ext"
    }
}
```
### Expected _Import Exif Overwrites_ result:
- Do overwrites using exiftool and update it in the database

## Search
To search in the database.

- Querystring `t` is used for the search query
- Querystring `p` is used for the pagina number. The first page is page 0.
- Querystring `json` is to render json.
- The request returns a maximum of 20 results
Endpoint: `/Starky/Search?t=searchword&p=0&json=true`

### Search using POST
The POST-request is a redirect to a get query with the same searchquery and the same pagenumber

### Search using GET
```json
{
    "uri":"/Starky/Search?t=searchword&p=0&json=true",
    "method":"GET",
    "authentication":
    {
        "username":"username",
        "password":"*sanitized*",
        "type":"Basic"
    }
}
```

### Expected `/starsky/search` response:

```json
{
  "fileIndexItems": [
    {
      "id": 414,
      "filePath": "/2018/2018-content/20180104_164720_mbp.jpg",
      "fileName": "20180104_164720_mbp.jpg",
      "fileHash": "CNO4NOALCDWVNGY66XG4427LIE",
      "parentDirectory": "/2018/2018-content",
      "isDirectory": false,
      "tags": "searchword",
      "description": null,
      "title": "",
      "dateTime": "2018-01-04T16:47:20",
      "addToDatabase": "2018-07-13T11:52:43.647793",
      "latitude": 0,
      "longitude": 0,
      "colorClass": 0
    }
  ],
  "breadcrumb": [
    "/",
    "searchword"
  ],
  "searchQuery": "searchword",
  "pageNumber": 0,
  "lastPageNumber": 1,
  "searchCount": 1,
  "searchIn": [
    "Tags"
  ],
  "searchFor": [
    "searchword"
  ],
  "searchForOptions": [
    ":"
  ],
  "pageType": "Search",
  "elapsedSeconds": 0.003
}
```

- When there are no search results:  `"searchCount": 1` will be `0` and the `fileIndexItems` will be a empty array

## Remove cache
When using cache (`IMemoryCache`) is might sometimes useful to reset the cache.

### Expected `/starsky/api/removecache?f=/folder` response:
- A 302 redirect to `?/folder` even if cache is disabled.
- With `&json=true`
	-	"cache succesfull cleared"
	-  "ignored, please check if the 'f' path exist or use a  
   folder string to clear the cache"
	-  cache is disabled in config  

## Environment info

To get information about the configuration.
This endpoint does not require autorisation.

```json
{
    "uri":"/Starky/Api/Env",
    "method":"GET"
}
```
### Expected `/Starky/Api/Env` response:

```json
{
  "baseDirectoryProject": "/starsky/starsky/starsky/bin/Debug/netcoreapp2.0/",
  "storageFolder": "/data/photolib/",
  "verbose": true,
  "databaseType": 2,
  "databaseConnection": "Data Source=/starsky/starsky/starsky/bin/Debug/netcoreapp2.0//data.db",
  "structure": "/yyyy/\file.ext",
  "structureExampleNoSetting": "/2018/file.jpg",
  "thumbnailTempFolder": "/starsky/thumbnailTempFolder/",
  "exifToolPath": "/usr/local/bin/exiftool",
  "readOnlyFolders": ["/2013"],
  "addMemoryCache": true
}
```
- The setting `DatabaseConnection` is only visable in `-c|--configuration {Debug}`. The connection string is in production not publicly visible due security reasons.
- Never use `-c|--configuration {Debug}` in production. [Check the Microsoft documentation for more information](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run?tabs=netcore20)

### StructureExampleNoSetting
If the structure is incorrect an Application Exception occurs. Structure requires to start with a `/` and the filename it must end with `.ext`
With the  `structureExampleNoSetting` value you can check the structure today.
In the view `example.jpg` is used to replace `{filenamebase}`. The file `example.jpg` and would be replaced with the orginal filename.
- [Read more about Structure configuation (starskyimportercli docs) ](../../starsky/starskyimportercli/readme.md)


## SubpathRelative Redirect
Redirect or view path to relative paths using the structure-config (see /api/env)
```json
{
    "uri":"/redirect/SubpathRelative?value=1&json=true",
    "method":"GET"
}
```
### Expected `/Starky/redirect/SubpathRelative?value=1&json=true` response:
```json
{
    "/2018/01/2018_01_01"
}
```

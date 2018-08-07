[< starsky/starsky docs](readme.md)

# Rest Api docs

The autorisation using the rest api is done though Basic Auth or Cookie Auth.

## Rest API Table of contents

- [Get PageType	"Archive" ](#get-pagetypearchive)
- [Get PageType	"DetailView"](#get-pagetypedetailview)
- [Exif Info](#exif-info)
- [Exif Update](#exif-update)
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

## Get PageType	"Archive" 
Endpoint `/starsky/?f=/&json=true` 
For browsing the folders. Please use  `"pageType": "Archive"` to check the page type. 
The querystring name `f` is used for the file path in releative/subpath style
 
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
      "id": 338,
      "filePath": "/2018/01",
      "fileName": "01",
      "fileHash": "ZUCBXWAETG6ZO5UV5VGBZJ3E74",
      "parentDirectory": "/2018",
      "isDirectory": true,
      "tags": "",
      "description": null,
      "title": "",
      "dateTime": "0001-01-01T00:00:00",
      "addToDatabase": "2018-07-13T11:52:42.942471",
      "latitude": 0,
      "longitude": 0,
      "colorClass": 0,
      "orientation": "DoNotChange",
      "imageFormat": "unknown",
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



## Get PageType	"DetailView" 
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
        "id": 13611,
        "filePath": "/2018/t/image.dng",
        "fileName": "image.dng",
        "fileHash": "NKY7U4WGIK6NFYNIKOW7IZ56GI",
        "fileCollectionName": "image",
        "parentDirectory": "/2018/t",
        "isDirectory": false,
        "tags": "",
        "description": "",
        "title": "",
        "dateTime": "2018-08-06T21:11:10",
        "addToDatabase": "2018-08-07T15:06:44.616725",
        "latitude": 52.8214,
        "longitude": 7.0474805555,
        "colorClass": 0,
        "orientation": "Horizontal",
        "imageFormat": "tiff",
        "collectionPaths": [
            "/2018/t/image.dng"
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
Api to get data about the picture that is editable. This checks the file using Exiftool
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
- Gives a list of names that are used by exiftool
- The querystring name `f` is used for the file path in releative/subpath style
- Ignore `Prefs` those are used to set `ColorClass` with Exiftool

The response by the info request
```json
{
    "sourceFile": "/data/isight//2018/01-dif/20180705_160335_DSC01991 kopie 2.jpg",
    "colorClass": 0,
    "Caption-Abstract": "captipn123444",
    "prefs": null,
    "keywords": [
        "okay1",
        "okay2"
    ],
    "tags": "okay1, okay2",
    "allDatesDateTime": "0001-01-01T00:00:00",
    "orientation": 1
}
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
### Rotation types
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

For now this api end point is using this method:
`Update(string tags, string colorClass,
            string captionAbstract, string f, string orientation, bool collections = true)`
- The querystring name `f` is used for the file path in releative/subpath style
- Empty tags are always ignored
- Stack collections or Collections is a feature to update multiple files with the same name. This feature is not completely implemented yet.
- `orientation` is using a enum to rotate the image 

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
{
    "sourceFile": "/image.jpg",
    "colorClass": 0,
    "Caption-Abstract": null,
    "keywords": [
        "okay2"
    ],
    "tags": "okay2",
    "allDatesDateTime": "0001-01-01T00:00:00",
    "orientation": 1
}
```
-  Statuscode 203. When trying to update a `read only` image. With the content `read only`
-  Error 404 When a image is `not in index`
-  Only the values that are request are return. In this example Caption-Abstract has a value but it is not requested.
> Update replied only the values that are request to update. To get all Info do a request to the Info-endpoint

## File Delete
To permanent delete a file from the file system and the database.
The tag: `!delete!` is used to mark a file that is in the Trash. This is not required by this api.

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
To get an thumbnail of the image, the thumbnail is 1000px width.
- The querystring after `Thumbnail/` is used for the base32 hash of the orginal image

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
- There is a orginal fallback, when using the `?issingleitem=True` query
  The orginal image will be loaded instead of the thumbnail
- The `retryThumbnail` is removing a thumbnail image. When this is used in combination with
 `isSingleItem` a orginal image is loaded. The query string is  `?retryThumbnail=True`. 
- Check [Thumbnail Json](#thumbnail-json) for more information

### Expected `/starsky/Api/Thumbnail` response:
- A jpeg image
- A 204 / `NoContent()` result when a thumbnail is corrupt
- A 404 Error page, when the base32 hash does not exist
- A 409 Error when "Thumbnail is not ready yet"

## Thumbnail Json
Endpoint: `/starsky/Api/Thumbnail/LNPE227BMTFMQWMIN7BE4X5ZOU&json=true`
For checking if a thumbnail exist without loading the entire image

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

### Expected `/starsky/Api/Thumbnail` response:
- A 200 result with no content if the request is successfull
- A 204 / `NoContent()` result when a thumbnail is corrupt
- A 404 Error page, when the base32 hash does not exist
- A 409 Error when "Thumbnail is not ready yet"

## Download Photo
To get an orginal or to regenerate a thumbnail.
- The querystring  `f` is used for the filepath of the orginal image
- When the querystring `isThumbnail` is used `true` a thumbnail we used or generated

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
- An orginal image



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
- Error 409 when the response array is empty. The response array is empty when there are no items added.
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
- Error 409 when the response array is empty. The response array is empty when there are no items added.
- When you have try to add duplicate items, those are not included in the list 
- An array with the added items:
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
  "pageType": "Search",
  "elapsedSeconds": 0.003
}
```

- When there are no search results:  `"searchCount": 1` will be `0` and the `fileIndexItems` will be a empty array

## Remove cache
When using cache is might sometimes useful to reset the cache.

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

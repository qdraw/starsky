
# Rest Api docs

The autorisation using the rest api is done though Basic Auth or Cookie Auth.

## Rest API Table of contents

- [Get PageType	"Archive" ](#get-pagetypearchive)
- [Get PageType	"DetailView"](#get-pagetypedetailview)
- [Exif Info](#exif-info)
- [Exif Update](#exif-update)
- [Thumbnail](#thumbnail)
- [Thumbnail Json](#thumbnail-json)
- [Direct import](#direct-import)
- [Form import](#form-import)
- [Search](#search)

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
      "colorClass": 0
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
  "pageType": "Archive"
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

### Expected `/starsky/?f=/image.jpg` response:

```json
{
  "fileIndexItem": {
    "id": 2837,
    "filePath": "/image.jpg",
    "fileName": "image.jpg",
    "fileHash": "W7Y65KYA5YDU43CRKZKJSAURMI",
    "parentDirectory": "/",
    "isDirectory": false,
    "tags": "",
    "description": "Date and Time based on filename",
    "title": "",
    "dateTime": "2018-01-23T13:24:04",
    "addToDatabase": "2018-07-20T16:24:04.942452",
    "latitude": 0,
    "longitude": 0,
    "colorClass": 0
  },
  "relativeObjects": {
    "nextFilePath": null,
    "prevFilePath": null
  },
  "breadcrumb": [
    "/"
  ],
  "colorClassFilterList": [],
  "pageType": "DetailView"
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
- Colorclass is a enum, and the values are:      
```cs
case "0":
    _colorClass = Color.None;
case "8":
    _colorClass = Color.Trash;
case "7":
    _colorClass = Color.Extras;
case "6":
    _colorClass = Color.TypicalAlt;
case "5":
    _colorClass = Color.Typical;
case "4":
    _colorClass = Color.SuperiorAlt;
case "3":
    _colorClass = Color.Superior;
case "2":
    _colorClass = Color.WinnerAlt;
case "1":
    _colorClass = Color.Winner;
```

## Exif Update
To update please request first [Exif Info](#exif-info).
Endpoint: `/starsky/Api/Update?f=/image.jpg`

For now this api end point is using this method:
`Update(string tags, string colorClass, string captionAbstract, string f = "dbStylePath")`
- The querystring name `f` is used for the file path in releative/subpath style
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
  "colorClass": 0,
  "Caption-Abstract": null,
  "keywords": null,
  "tags": "",
  "allDatesDateTime": "0001-01-01T00:00:00"
}
```
-  Statuscode 203. When trying to update a `read only` image. With the content `read only`
-  Error 404 When a image is `not in index`


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


## Direct import
For importing using the structure configuration
The filename-header can be added in `base64` or as `string`.
`Content-type` is required, please use `image/jpeg`
Endpoint: `/starsky/import` 

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
Endpoint: `/starsky/import` 
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
    "/2018/07/2018_07_22/20180722_220442__import_20180720_201452_2018-07-20 20.14.52.jpg"
]
```


# Search
To search in the database.

- Querystring `t` is used for the search query
- Querystring `p` is used for the pagina number. The first page is page 0.
- Querystring `json` is to render json.
Endpoint: `/Starky/Search?t=searchword&p=0&json=true` 

## Search using POST
The POST-request is a redirect to a get query with the same searchquery and the same pagenumber

## Search using GET
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

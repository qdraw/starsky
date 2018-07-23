
# Rest Api docs

The autorisation using the rest api is done though Basic Auth or Cookie Auth.
- [Get PageType	"Archive" ](#get-pagetypearchive)
- [Get PageType	"DetailView"](#get-pagetypedetailview)
- [Exif Info](#exif-info)
- [Exif Update](#exif-update)
- [Direct import](#direct-import)
- [Form import](#form-import)

## Get PageType	"Archive" 
Endpoint `/starsky/?f=/&json=true` 
For browsing the folders. Please use  `"pageType": "Archive"` to check the page type. 
```
{
    "uri":"`/starsky/?f=/&json=true",
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
```
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
Endpoint: `/starsky/?f=/image.jpg` 
```
{
    "uri":"`/starsky/?f=/image.jpg&json=true",
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

```
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
Endpoint: `/starsky/Api/Info?f=/image.jpg`
```
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

```
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
```
{
  "colorClass": 0,
  "Caption-Abstract": null,
  "keywords": null,
  "tags": "",
  "allDatesDateTime": "0001-01-01T00:00:00"
}
```



## Direct import
For importing using the structure configuration
The filename-header can be added in `base64` or as `string`.
`Content-type` is required, please use `image/jpeg`
Endpoint: `/starsky/import` 

```
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
```
[
    "/2018/07/2018_07_22/20180722_220442__import_20180720_201452_2018-07-20 20.14.52.jpg"
]
```

## Form import
When using a form, the filename is extracted from the multipart. For the filename there is only string encoding support
Endpoint: `/starsky/import` 
```
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
```
[
    "/2018/07/2018_07_22/20180722_220442__import_20180720_201452_2018-07-20 20.14.52.jpg"
]
```




# Rest Api docs

The autorisation using the rest api is done though Basic Auth or Cookie Auth.

## Get PageType	"Archive" `/starsky/?f=/&json=true` 
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



## Get PageType	"DetailView" `/starsky/?f=/image.jpg` 
Api to get fast meta data about the picture. 
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

## Direct input `/starsky/import` 
The filename-header can be added in `base64` or as `string`.
`Content-type` is required, please use `image/jpeg`

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

## Form input `/starsky/import` 
When using a form, the filename is extracted from the multipart. For the filename there is only string encoding support

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



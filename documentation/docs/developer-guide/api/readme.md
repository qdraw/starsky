---
sidebar_position: 6
---

# API Endpoint Documentation
The API has two ways of authentication using Cookie Authentication via the `/api/account/login` endpoint and Basic Authentication

This document is auto generated

| Path                                              | Type  | Description                                                                     | 
|---------------------------------------------------|-------|---------------------------------------------------------------------------------|
| __/api/account/status__                           | GET   | Check the account status of the current logged in user                          |
| __/account/login__                                | GET   | Login form page (HTML)                                                          |
| __/api/account/login__                            | POST  | Login the current HttpContext in                                                |
| __/api/account/logout__                           | POST  | Logout the current HttpContext out                                              |
| __/account/logout__                               | GET   | Logout the current HttpContext and redirect to login                            |
| __/api/account/change-secret__                    | POST  | Update password for current user                                                |
| __/api/account/register__                         | POST  | Create a new user (you need a AF-token first)                                   |
| __/api/account/register/status__                  | GET   | Is the register form open                                                       |
| __/api/account/permissions__                      | GET   | List of current permissions                                                     |
| __/api/allowed-types/mimetype/sync__              | GET   | A (string) list of allowed MIME-types ExtensionSyncSupportedList                |
| __/api/allowed-types/mimetype/thumb__             | GET   | A (string) list of allowed ExtensionThumbSupportedList MimeTypes                |
| __/api/allowed-types/thumb__                      | GET   | Check if IsExtensionThumbnailSupported                                          |
| __/api/env__                                      | GET   | Show the runtime settings (dont allow AllowAnonymous)                           |
| __/api/env__                                      | POST  | Show the runtime settings (dont allow AllowAnonymous)                           |
| __/api/env/features__                             | GET   | Show features that used in the frontend app / menu                              |
| __/api/cache/list__                               | GET   | Get Database Cache (only the cache)                                             |
| __/api/remove-cache__                             | GET   | Delete Database Cache (only the cache)                                          |
| __/api/remove-cache__                             | POST  | Delete Database Cache (only the cache)                                          |
| __/api/delete__                                   | DELETE| Remove files from the disk, but the file must contain the !delete! (TrashKeyw...|
| _Parameters: f (subPaths, separated by dot comma), collections (true is to update files with the same name before                         _ |
| _ the extenstion)                                                                                                                         _ |
| __/api/disk/mkdir__                               | POST  | Make a directory (-p)                                                           |
| __/api/disk/rename__                              | POST  | Rename file/folder and update it in the database                                |
| _Parameters: f (from subPath), to (to subPath), collections (is collections bool), currentStatus (default is to not                       _ |
| _ included files that are removed in result)                                                                                              _ |
| __/api/download-sidecar__                         | GET   | Download sidecar file for example image.xmp                                     |
| __/api/download-photo__                           | GET   | Select manually the original or thumbnail                                       |
| _Parameters: f (string, 'sub path' to find the file), isThumbnail (true = 1000px thumb (if supported)), cache (true                       _ |
| _ = send client headers to cache)                                                                                                         _ |
| __/error__                                        | GET   | Return Error page (HTML)                                                        |
| __/api/export/create-zip__                        | POST  | Export source files to an zip archive                                           |
| _Parameters: f (subPath to files), collections (enable files with the same name (before the extension)), thumbnail                        _ |
| _ (export thumbnails)                                                                                                                     _ |
| __/api/export/zip/{f}.zip__                       | GET   | Get the exported zip, but first call 'createZip'use for example this url: /ex...|
| __/api/geo/status__                               | GET   | Get Geo sync status                                                             |
| __/api/geo/sync__                                 | POST  | Reverse lookup for Geo Information and/or add Geo location based on a GPX fil...|
| __/api/geo-reverse-lookup__                       | GET   | Reverse geo lookup                                                              |
| __/api/health__                                   | GET   | Check if the service has any known errors and return only a stringPublic API    |
| __/api/health/details__                           | GET   | Check if the service has any known errorsFor Authorized Users only              |
| __/api/health/application-insights__              | GET   | Add Application Insights script to user context                                 |
| __/api/health/version__                           | POST  | Check if Client/App version has a match with the API-versionthe parameter 've...|
| __/api/health/check-for-updates__                 | GET   | Check if Client/App version has a match with the API-version                    |
| __/search__                                       | POST  | Redirect to search GET page (HTML)                                              |
| __/search__                                       | GET   | Search GET page (HTML)                                                          |
| __/trash__                                        | GET   | Trash page (HTML)                                                               |
| __/import__                                       | GET   | Import page (HTML)                                                              |
| __/preferences__                                  | GET   | Preferences page (HTML)                                                         |
| __/account/register__                             | GET   | View the Register form (HTML)                                                   |
| __/api/import__                                   | POST  | Import a file using the structure format                                        |
| __/api/import/fromUrl__                           | POST  | Import file from web-url (only whitelisted domains) and import this file into...|
| _Parameters: fileUrl (the url), filename (the filename (optional, random used if empty)), structure (use structure                        _ |
| _ (optional))                                                                                                                             _ |
| __/api/import/history__                           | GET   | Today's imported files                                                          |
| __/api/import/thumbnail__                         | POST  | Upload thumbnail to ThumbnailTempFolderMake sure that the filename is correct...|
| __/api/index__                                    | GET   | The database-view of a directory                                                |
| _Parameters: f (subPath), colorClass (filter on colorClass (use int)), collections (to combine files with the same                        _ |
| _ name before the extension), hideDelete (ignore deleted files), sort (how to orderBy, defaults to fileName)                              _ |
| __/api/memory-cache-debug__                       | GET   | View data from the memory cache - use to debug                                  |
| __/api/info__                                     | GET   | Get realtime (cached a few minutes) about the file                              |
| _Parameters: f (subPaths split by dot comma), collections (true is to update files with the same name before the                          _ |
| _ extenstion)                                                                                                                             _ |
| __/api/replace__                                  | POST  | Search and Replace text in meta information                                     |
| _Parameters: f (subPath filepath to file, split by dot comma (;)), fieldName (name of fileIndexItem field e.g. Tags),                     _ |
| _ search (text to search for), replace (replace [search] with this text), collections (enable collections)                                _ |
| __/api/update__                                   | POST  | Update Exif and Rotation API                                                    |
| _Parameters: Id, FilePath, FileName, FileHash, FileCollectionName, ParentDirectory, IsDirectory, Tags, Status, Description,               _ |
| _Title, DateTime, AddToDatabase, LastEdited, Latitude, Longitude, LocationAltitude, LocationCity, LocationState,                          _ |
| _LocationCountry, LocationCountryCode, ColorClass, Orientation, ImageWidth, ImageHeight, ImageFormat, CollectionPaths,                    _ |
| _SidecarExtensions, SidecarExtensionsList, Aperture, ShutterSpeed, IsoSpeed, Software, MakeModel, Make, Model, LensModel,                 _ |
| _FocalLength, Size, ImageStabilisation, LastChanged, f (subPath filepath to file, split by dot comma (;)), append                         _ |
| _(only for stings, add update to existing items), collections (StackCollections bool, default true), rotateClock                          _ |
| _ (relative orientation -1 or 1)                                                                                                          _ |
| __/api/notification/notification__                | GET   | Get recent notificationsUse dateTime 2022-04-16T17:33:10.323974Z to get the l...|
| __/api/publish__                                  | GET   | Get all publish profilesTo see the entire config check appSettings              |
| __/api/publish/create__                           | POST  | Publish                                                                         |
| _Parameters: f (subPath filepath to file, split by dot comma (;)), itemName (itemName), publishProfileName (publishProfileName),          _ |
| _ force                                                                                                                                   _ |
| __/api/publish/exist__                            | GET   | To give the user UI feedback when submitting the itemNameTrue is not to conti...|
| __/redirect/sub-path-relative__                   | GET   | Redirect or view path to relative paths using the structure-config (see /api/...|
| __/api/search__                                   | GET   | Gets the list of search results (cached)                                        |
| __/api/search/relative-objects__                  | GET   | Get relative paths in a search queryDoes not cover multiple pages (so it ends...|
| __/api/search/trash__                             | GET   | List of files with the tag: !delete! (TrashKeyword.TrashKeywordString)Caching...|
| __/api/search/remove-cache__                      | POST  | Clear search cache to show the correct results                                  |
| __/api/suggest__                                  | GET   | Gets the list of search results (cached)                                        |
| __/api/suggest/all__                              | GET   | Show all items in the search suggest cache                                      |
| __/api/suggest/inflate__                          | GET   | To fill the cache with the data (only if cache is not already filled)           |
| __/api/synchronize__                              | POST  | Faster API to Check if directory is changed (not recursive)                     |
| __/api/synchronize__                              | GET   | Faster API to Check if directory is changed (not recursive)                     |
| __/api/thumbnail/small/{f}__                      | GET   | Get thumbnail for index pages (300 px or 150px or 1000px (based on whats there))|
| __/api/thumbnail/list-sizes/{f}__                 | GET   | Get overview of what exists by name                                             |
| __/api/thumbnail/{f}__                            | GET   | Get thumbnail with fallback to original source image.Return source image when...|
| _Parameters: f (one single fileHash (NOT path)), filePath (fallback FilePath), isSingleItem (true = load original),                       _ |
| _ json (text as output), extraLarge (give preference to extraLarge over large image)                                                      _ |
| __/api/thumbnail/zoom/{f}@{z}__                   | GET   | Get zoomed in image by fileHash.At the moment this is the source image          |
| __/api/thumbnail-generation__                     | POST  | Create thumbnails for a folder in the background                                |
| __/api/trash/detect-to-use-system-trash__         | GET   | Is the system trash supported                                                   |
| __/api/trash/move-to-trash__                      | POST  | (beta) Move a file to the trash                                                 |
| __/api/upload__                                   | POST  | Upload to specific folder (does not check if already has been imported)Use th...|
| __/api/upload-sidecar__                           | POST  | Upload sidecar file to specific folder (does not check if already has been im...|

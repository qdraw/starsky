﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starsky.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        private readonly IQuery _query;
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
        private readonly IReadMeta _readMeta;

        public ApiController(IQuery query, IExiftool exiftool, 
            AppSettings appSettings, IBackgroundTaskQueue queue,
            IReadMeta readMeta
            )
        {
            _appSettings = appSettings;
            _query = query;
            _exiftool = exiftool;
            _bgTaskQueue = queue;
            _readMeta = readMeta;
        }
        

        // Used for end2end test
        [HttpGet]
        [HttpHead]
        [ResponseCache(Duration = 30 )]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous] /// <=================================
        public IActionResult Env()
        {
            return Json(_appSettings);
        }

        private bool _isReadOnly(string f)
        {
            if (_appSettings.ReadOnlyFolders == null) return false;
            
            var result = _appSettings.ReadOnlyFolders.FirstOrDefault(f.Contains);
            return result != null;
        }
        
        

        private FileIndexItem.ExifStatus FileCollectionsCheck(DetailView detailView)
        {
            if (detailView == null)
            {
                return FileIndexItem.ExifStatus.NotFoundNotInIndex;
            }

            if (_isReadOnly(detailView.FileIndexItem.ParentDirectory)) return  FileIndexItem.ExifStatus.ReadOnly;

            foreach (var collectionPath in detailView.FileIndexItem.CollectionPaths)
            {
                var fullPathCollection = _appSettings.DatabasePathToFilePath(collectionPath);
                
                //For the situation that the file is not on disk but the only one in the list
                if (!System.IO.File.Exists(fullPathCollection) 
                    && detailView.FileIndexItem.CollectionPaths.Count == 1)
                {
                    return FileIndexItem.ExifStatus.NotFoundSourceMissing;  //
                }
                // When there are more items in the list
                if (!System.IO.File.Exists(fullPathCollection))
                {
                    detailView.FileIndexItem.CollectionPaths.Remove(collectionPath);
                }
            }

            if (detailView.FileIndexItem.CollectionPaths.Count == 0)
            {
                return FileIndexItem.ExifStatus.NotFoundSourceMissing;
            }

            return FileIndexItem.ExifStatus.Ok;
        }

        /// <summary>
        /// Update Exif and Rotation API
        /// </summary>
        /// <param name="f">subpath filepath to file, split by dot comma (;)</param>
        /// <param name="tags">use for keywords</param>
        /// <param name="colorClass">int 0-9, the colorclass to fast select images</param>
        /// <param name="captionAbstract">string to update description/caption abstract, emthy will be ignored</param>
        /// <param name="orientation">relative orentation -1 or 1</param>
        /// <param name="title">edit image title</param>
        /// <param name="collections">StackCollections bool</param>
        /// <param name="append">only for stings, add update to existing items</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Update(FileIndexItem inputModel, string f, bool append, bool collections, int orientation = 0)
        {
            // input devided by dot comma and blank values are removed
            var inputFilePaths = f.Split(";");
            inputFilePaths = inputFilePaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            // the result list
            var fileIndexResultsList = new List<FileIndexItem>();
                
            foreach (var subPath in inputFilePaths)
            {
                var detailView = _query.SingleItem(subPath,null,collections,false);
                var statusResults = FileCollectionsCheck(detailView);
                
                var statusModel = inputModel.Clone();
                statusModel.SetFilePath(subPath);
                statusModel.IsDirectory = false;
                
                // if one item fails, the status will added
                switch (statusResults)
                {
                    case FileIndexItem.ExifStatus.NotFoundNotInIndex:
                        statusModel.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
                        fileIndexResultsList.Add(statusModel);
                        continue;
                    case FileIndexItem.ExifStatus.NotFoundSourceMissing:
                        statusModel.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
                        fileIndexResultsList.Add(statusModel);
                        continue;
                    case FileIndexItem.ExifStatus.ReadOnly:
                        statusModel.Status = FileIndexItem.ExifStatus.ReadOnly;
                        fileIndexResultsList.Add(statusModel);
                        continue;
                }
                
                // Paths that are used
                var collectionSubPathList = detailView.FileIndexItem.CollectionPaths;
                // when not running in collections mode only update one file
                if(!collections) collectionSubPathList = new List<string> {subPath};
                
                var updatedExifFullPaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);

//                
//                // old hash codes
//                var oldHashCodes = FileHash.GetHashCode(updatedExifFullPaths.ToArray());
//                    
                
                for (int i = 0; i < collectionSubPathList.Count; i++)
                {
                    var comparedNamesList = FileIndexCompareHelper.Compare(detailView.FileIndexItem, statusModel);
                    // this one is good :)
                    detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
                    detailView.FileIndexItem.RelativeOrientation(orientation);
                    
                    fileIndexResultsList.Add(detailView.FileIndexItem);
                    
                    // Update >
                    _bgTaskQueue.QueueBackgroundWorkItem(async token =>
                    {
                        var exiftool = new ExifToolCmdHelper(_appSettings,_exiftool);
                        var exifUpdateFilePaths = new List<string>
                        {
                            _appSettings.DatabasePathToFilePath(detailView.FileIndexItem.FilePath)
                        };
                        exiftool.Update(detailView.FileIndexItem, exifUpdateFilePaths, comparedNamesList);
                        // > async > force you to read the file again
                         _readMeta.RemoveReadMetaCache(updatedExifFullPaths);
                    });
                    
                }
                _query.UpdateItem(fileIndexResultsList);
                
            }
            
            // When all items are not found
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);
            
            return Json(fileIndexResultsList);
        }


        

        
            
//        /// <summary>
//        /// Update Exif and Rotation API
//        /// </summary>
//        /// <param name="tags">use for keywords</param>
//        /// <param name="colorClass">int 0-9, the colorclass to fast select images</param>
//        /// <param name="captionAbstract">string to update description/caption abstract, emthy will be ignored</param>
//        /// <param name="f">subpath filepath to file, split by dot comma (;)</param>
//        /// <param name="orientation">relative orentation -1 or 1</param>
//        /// <param name="title">edit image title</param>
//        /// <param name="collections">StackCollections bool</param>
//        /// <param name="append">only for stings, add update to existing items</param>
//        /// <returns></returns>
//        [HttpPost("/api/v1/update")]
//        public IActionResult UpdateV1(string tags, string colorClass,
//            string captionAbstract, string f, int orientation, string title,
//            bool collections = true, bool append = false)
//        {
//            // input devided by dot comma and blank values are removed
//            var inputFilePaths = f.Split(";");
//            inputFilePaths = inputFilePaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();
//            
//            // the result list
//            var exifToolResultsList = new List<ExifToolModel>();
//                
//            foreach (var subPath in inputFilePaths)
//            {
//                var detailView = _query.SingleItem(subPath,null,collections,false);
//                var results = FileCollectionsCheck(detailView);
//                
//                // First create an update model
//                var updateModel = new ExifToolModel
//                {
//                    SourceFile = subPath,
//                    Status = ExifToolModel.ExifStatus.Ok
//                };
//                
//                // if one item fails, the status will added
//                switch (results)
//                {
//                    case ExifToolModel.ExifStatus.NotFoundNotInIndex:
//                        updateModel.Status = ExifToolModel.ExifStatus.NotFoundNotInIndex;
//                        exifToolResultsList.Add(updateModel);
//                        continue;
//                    case ExifToolModel.ExifStatus.NotFoundSourceMissing:
//                        updateModel.Status = ExifToolModel.ExifStatus.NotFoundSourceMissing;
//                        exifToolResultsList.Add(updateModel);
//                        continue;
//                    case ExifToolModel.ExifStatus.ReadOnly:
//                        updateModel.Status = ExifToolModel.ExifStatus.ReadOnly;
//                        exifToolResultsList.Add(updateModel);
//                        continue;
//                }
//
//                // Feature to add or update the strings
//                updateModel.Tags = AddOrAppendStings(tags,append,true,detailView.FileIndexItem.Tags);
//                updateModel.CaptionAbstract = AddOrAppendStings(captionAbstract,append,
//                    false,detailView.FileIndexItem.Description);
//                updateModel.ObjectName = AddOrAppendStings(title,append,
//                    false,detailView.FileIndexItem.Title);
//                
//                
//                // Parse ColorClass and add it
//                // This SetColorClass does return DoNotChange and all other tags
//                updateModel.ColorClass = detailView.FileIndexItem.GetColorClass(colorClass);
//                
//                // Parse Rotation; by reading it relative
//                updateModel.Orientation = detailView.FileIndexItem.RelativeOrientation(orientation);
//                
//                // Paths that are used
//                var collectionSubPathList = detailView.FileIndexItem.CollectionPaths;
//                // when not running in collections mode only update one file
//                if(!collections) collectionSubPathList = new List<string> {subPath};
//                var collectionFullPaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);
//                
//                // old hash codes
//                var oldHashCodes = FileHash.GetHashCode(collectionFullPaths.ToArray());
//                    
//                // Run Update program
//                // Run as non-blocking task to avoid files not being updated or corrupt
//                _bgTaskQueue.QueueBackgroundWorkItem(async token =>
//                {
//                    _exiftool.Update(updateModel, collectionFullPaths);
//                    // > async > force you to read the file again
//                    _readMeta.RemoveReadMetaCache(collectionFullPaths);
//                });
//    
//                // loop though the collection paths; even if it is one item
//                for (int i = 0; i < collectionSubPathList.Count; i++)
//                {
//                    UpdateSingleItemToDatabase(collectionSubPathList[i], 
//                        collectionFullPaths[i],
//                        exifToolResultsList, 
//                        updateModel, 
//                        oldHashCodes[i],
//                        orientation);
//                }
//            }
//            
//            // When all items are not found
//            if (exifToolResultsList.All(p => p.Status != ExifToolModel.ExifStatus.Ok))
//                return NotFound(exifToolResultsList);
//            
//            return Json(exifToolResultsList);
//        }
//
//        public string AddOrAppendStings(string inputString, bool append, bool commaseperate, string appendedString)
//        {
//            // >>   "objectName": "dion2dion",
//            // todo bug: append is prepend
//            
//            var inputStringBulder = new StringBuilder();
//            inputStringBulder.Append(inputString);
//            
//            if (append && !commaseperate)
//            {
//                inputStringBulder.Append(appendedString);
//            }
//
//            if (append && commaseperate && !string.IsNullOrEmpty(inputString))
//            {
//                inputStringBulder.Append(", " + appendedString);
//            }
//            return inputStringBulder.ToString();
//        }
//
//        public void UpdateSingleItemToDatabase(
//            string collectionSubPath, 
//            string collectionFullPath,
//            List<ExifToolModel> exifToolResultsList, 
//            ExifToolModel updateModel,
//            string oldHashCode,
//            int orientation)
//        {
//            var singleItem = _query.SingleItem(collectionSubPath,null,false,false);
//    
//            
//            // When adding new object >> Check ExifToolModel >> 
//            
//            
//            // make a new object to avoid references
//            var displayUpdateModel = new ExifToolModel(updateModel); 
//            displayUpdateModel.SourceFile = collectionSubPath;
//    
//            if (!string.IsNullOrEmpty(updateModel.Tags))
//            {
//                singleItem.FileIndexItem.Tags = updateModel.Tags;
//            }
//            
//            if (!string.IsNullOrEmpty(updateModel.CaptionAbstract))
//            {
//                singleItem.FileIndexItem.Description = updateModel.CaptionAbstract;
//            }
//            
//            if (!string.IsNullOrEmpty(updateModel.ObjectName))
//            {
//                singleItem.FileIndexItem.Title = updateModel.ObjectName;
//            }
//                    
//            // In the model there is a filter
//            if (updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
//            {
//                singleItem.FileIndexItem.ColorClass = updateModel.ColorClass;                
//            }
//    
//            exifToolResultsList.Add(displayUpdateModel);
//    
//            singleItem.FileIndexItem.FileHash = FileHash.GetHashCode(collectionFullPath);
//            // Rename Thumbnail
//            new Thumbnail(_appSettings).RenameThumb(oldHashCode, singleItem.FileIndexItem.FileHash);
//                    
//            //  // Don't update this when it not has changed
//            if (updateModel.Orientation != FileIndexItem.Rotation.DoNotChange)
//            {
//                singleItem.FileIndexItem.Orientation = updateModel.Orientation;
//                        
//                var thumbPath = _appSettings.ThumbnailTempFolder + singleItem.FileIndexItem.FileHash + ".jpg";
//                new Thumbnail(null).RotateThumbnail(thumbPath,orientation);
//            }
//            
//            // update item to the database
//            _query.UpdateItem(singleItem.FileIndexItem);
//        }


        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] {"f"})]
        public IActionResult Info(string f, bool collections = true)
        {
            // input devided by dot comma and blank values are removed
            var inputFilePaths = f.Split(";");
            inputFilePaths = inputFilePaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            // the result list
            var fileIndexResultsList = new List<FileIndexItem>();

            foreach (var subPath in inputFilePaths)
            {
                var detailView = _query.SingleItem(subPath, null, collections, false);
                var statusResults = FileCollectionsCheck(detailView);

                var statusModel = new FileIndexItem();
                statusModel.SetFilePath(subPath);
                statusModel.IsDirectory = false;

                // if one item fails, the status will added
                switch (statusResults)
                {
                    case FileIndexItem.ExifStatus.NotFoundNotInIndex:
                        statusModel.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
                        fileIndexResultsList.Add(statusModel);
                        continue;
                    case FileIndexItem.ExifStatus.NotFoundSourceMissing:
                        statusModel.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
                        fileIndexResultsList.Add(statusModel);
                        continue;
                    case FileIndexItem.ExifStatus.ReadOnly:
                        statusModel.Status = FileIndexItem.ExifStatus.ReadOnly;
                        fileIndexResultsList.Add(statusModel);
                        continue;
                }

                // Paths that are used
                var collectionSubPathList = detailView.FileIndexItem.CollectionPaths;
                // when not running in collections mode only update one file
                if (!collections) collectionSubPathList = new List<string> {subPath};
                var collectionFullPaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);

                fileIndexResultsList.AddRange(_readMeta.
                    ReadExifAndXmpFromFileAddFilePathHash(collectionFullPaths.ToArray()));
            }

            // When all items are not found
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);
            
            return Json(fileIndexResultsList);
        }
        

//        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] {"f"})]
//        public IActionResult Info(string f, bool collections = true)
//        {
//            // input devided by dot comma and blank values are removed
//            var inputFilePaths = f.Split(";");
//            inputFilePaths = inputFilePaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();
//
//            // the result list
//            var exifToolResultsList = new List<ExifToolModel>();
//
//            foreach (var subPath in inputFilePaths)
//            {
//                var detailView = _query.SingleItem(subPath, null, collections, false);
//                var results = FileCollectionsCheck(detailView);
//                
//                // First create an update model
//                var updateModel = new ExifToolModel
//                {
//                    SourceFile = subPath,
//                };
//                
//                // if one item fails, the status will added
//                switch (results)
//                {
//                    case ExifToolModel.ExifStatus.NotFoundNotInIndex:
//                        updateModel.Status = ExifToolModel.ExifStatus.NotFoundNotInIndex;
//                        exifToolResultsList.Add(updateModel);
//                        continue;
//                    case ExifToolModel.ExifStatus.NotFoundSourceMissing:
//                        updateModel.Status = ExifToolModel.ExifStatus.NotFoundSourceMissing;
//                        exifToolResultsList.Add(updateModel);
//                        continue;
//                    case ExifToolModel.ExifStatus.ReadOnly:
//                        updateModel.Status = ExifToolModel.ExifStatus.ReadOnly;
//                        exifToolResultsList.Add(updateModel);
//                        continue;
//                }
//                // Paths that are used
//                var collectionSubPathList = detailView.FileIndexItem.CollectionPaths;
//                // when not running in collections mode only update one file
//                if(!collections) collectionSubPathList = new List<string> {subPath};
//                var collectionFullPaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);
//                // loop though the collection paths; even if it is one item
//                for (int i = 0; i < collectionSubPathList.Count; i++)
//                {
//                    var fileItem = _readMeta.ReadExifAndXmpFromFile(collectionFullPaths[i]);
//                    var infoModel = new ExifToolModel
//                    {
//                        SourceFile = collectionSubPathList[i],
//                        CaptionAbstract = fileItem.Description,
//                        ColorClass = fileItem.ColorClass,
//                        Tags = fileItem.Tags,
//                        Orientation = fileItem.Orientation,
//                        ImageWidth = fileItem.ImageWidth,
//                        ImageHeight = fileItem.ImageHeight,
//                        ObjectName = fileItem.Title,
//                        Status = ExifToolModel.ExifStatus.Ok,
//                    };
//                    exifToolResultsList.Add(infoModel);
//                }
//
//            } //e/for
//            
//            // When all items are not found
//            if (exifToolResultsList.All(p => p.Status != ExifToolModel.ExifStatus.Ok))
//                return NotFound(exifToolResultsList);
//            
//            return Json(exifToolResultsList);
//        }


        [HttpDelete]
        public IActionResult Delete(string f = "dbStyleFilepath")
        {
            if (_isReadOnly(f)) return NotFound("afbeelding is in lees-only mode en kan niet worden verwijderd");

            var singleItem = _query.SingleItem(f,null,false,false);
            if (singleItem == null) return NotFound("not in index");
            if (!System.IO.File.Exists(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath,null,false,false).FileIndexItem;

            //  Remove Files if exist xmp file
            var fullFilePath = _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            var toDeletePaths =
                new List<string>
                {
                    fullFilePath,
                    fullFilePath.Replace(Path.GetExtension(fullFilePath), ".xmp"),
                    fullFilePath.Replace(Path.GetExtension(fullFilePath), ".XMP")
                };

            foreach (var toDelPath in toDeletePaths)
            {
                if (System.IO.File.Exists(toDelPath))
                {
                    System.IO.File.Delete(toDelPath);
                }
            }
            // End Remove files
            
            _query.RemoveItem(item);

            return Json(item);
        }

        [ResponseCache(Duration = 90000, VaryByQueryKeys = new [] { "f", "json", 
            "retryThumbnail", "isSingleitem"} )]
        [HttpGet("/api/thumbnail/{f}")]
        [HttpHead("/api/thumbnail/{f}")]
        [IgnoreAntiforgeryToken]
        public IActionResult Thumbnail(
            string f, 
            bool isSingleitem = false, 
            bool json = false,
            bool retryThumbnail = false)
        {
            // f is Hash
            // isSingleItem => detailview
            // Retry thumbnail => is when you press reset thumbnail
            // json, => to don't waste the users bandwith.
            
            var thumbPath = _appSettings.ThumbnailTempFolder + f + ".jpg";

            if (Files.IsFolderOrFile(thumbPath) == FolderOrFileModel.FolderOrFileTypeList.File)
            {
                // When a file is corrupt show error + Delete
                var imageFormat = Files.GetImageFormat(thumbPath);
                if (imageFormat == Files.ImageFormat.unknown)
                {
                    if (!retryThumbnail)
                    {
                        Console.WriteLine("image is corrupt");
                        SetExpiresResponseHeadersToZero();
                        return NoContent();
                    }
                    System.IO.File.Delete(thumbPath);
                }
                
                // When using the api to check using javascript
                // use the cached version of imageFormat, otherwise you have to check if it deleted
                if (imageFormat != Files.ImageFormat.unknown)
                {
                    if (json) return Json("OK");

                    // thumbs are always in jpeg
                    FileStream fs = System.IO.File.OpenRead(thumbPath);
                    return File(fs, "image/jpeg");
                }
            }
            
            
            var sourcePath = _query.GetItemByHash(f);
            if (sourcePath == null) return NotFound("not in index");
            
            var sourceFullPath = _appSettings.DatabasePathToFilePath(sourcePath);

            if (!System.IO.File.Exists(thumbPath) &&
                System.IO.File.Exists(sourceFullPath))
            {
                if (!isSingleitem)
                {
                    // "Photo exist in database but " + "isSingleItem flag is Missing"
                    SetExpiresResponseHeadersToZero();
                    Response.StatusCode = 409; // A conflict, that the thumb is not generated yet
                    return Json("Thumbnail is not ready yet");
                }
                
                var fileExtensionWithoutDot = Path.GetExtension(sourceFullPath).Remove(0, 1).ToLower();
                    
                if (Files.ExtensionThumbSupportedList.Contains(fileExtensionWithoutDot.ToLower()))
                {
                    FileStream fs1 = System.IO.File.OpenRead(sourceFullPath);
                    return File(fs1, MimeHelper.GetMimeType(fileExtensionWithoutDot));
                }
                Response.StatusCode = 409; // A conflict, that the thumb is not generated yet
                return Json("Thumbnail is not supported; for example you try to view a raw file");
            }

            return NotFound("There is no thumbnail image " + thumbPath + " and no source image "+ sourcePath );
            // When you have duplicate files and one of them is removed and there is no thumbnail generated yet you might get an false error
        }

        
        public void SetExpiresResponseHeadersToZero()
        {
            Request.HttpContext.Response.Headers.Remove("Cache-Control");
            Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

            Request.HttpContext.Response.Headers.Remove("Pragma");
            Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

            Request.HttpContext.Response.Headers.Remove("Expires");
            Request.HttpContext.Response.Headers.Add("Expires", "0");
        }

        [HttpGet]
        [HttpHead]
        public IActionResult DownloadPhoto(string f, bool isThumbnail = true)
        {
            // f = subpath/filepath
            if (f.Contains("?isthumbnail")) return NotFound("please use &isthumbnail= instead of ?isthumbnail=");

            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index " + f);

            var sourceFullPath = _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            if (!System.IO.File.Exists(sourceFullPath))
                return NotFound("source image missing " + sourceFullPath );

            // Return full image
            if (!isThumbnail)
            {
                FileStream fs = System.IO.File.OpenRead(sourceFullPath);
                // Return the right mime type
                return File(fs, MimeHelper.GetMimeTypeByFileName(sourceFullPath));
            }

            // Return Thumbnail
            
            var thumbPath = _appSettings.ThumbnailTempFolder + singleItem.FileIndexItem.FileHash + ".jpg";

            // If File is corrupt delete it
            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
            {
                System.IO.File.Delete(thumbPath);
            }

            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.notfound)
            {
                if (Files.IsFolderOrFile(_appSettings.ThumbnailTempFolder) ==
                    FolderOrFileModel.FolderOrFileTypeList.Deleted)
                {
                    return NotFound("Thumb base folder " + _appSettings.ThumbnailTempFolder + " not found");
                }
                
                var searchItem = new FileIndexItem
                {
                    FileName = _appSettings.FullPathToDatabaseStyle(sourceFullPath)
                        .Split("/").LastOrDefault(),
                    ParentDirectory = Breadcrumbs.BreadcrumbHelper(_appSettings.
                        FullPathToDatabaseStyle(sourceFullPath)).LastOrDefault(),
                    FileHash = FileHash.GetHashCode(sourceFullPath)
                };
                
                // When you have a different tag in the database than on disk
                thumbPath = _appSettings.ThumbnailTempFolder + searchItem.FileHash + ".jpg";
                    
                var isSuccesCreateAThumb = new Thumbnail(_appSettings).CreateThumb(searchItem);
                if (!isSuccesCreateAThumb)
                {
                    Response.StatusCode = 500;
                    return Json("Thumbnail generation failed");
                }

                FileStream fs2 = System.IO.File.OpenRead(thumbPath);
                return File(fs2, "image/jpeg");
            }

            var getExiftool = _exiftool.Info(sourceFullPath);
//            _exiftool.Update(getExiftool, sourceFullPath);

            FileStream fs1 = System.IO.File.OpenRead(thumbPath);
            return File(fs1, "image/jpeg");
        }

        [HttpGet]
        [HttpPost]
        public IActionResult RemoveCache(string f = "/", bool json = false)
        {
            //For folder paths only
            if (!_appSettings.AddMemoryCache)
            {
                Response.StatusCode = 412;
                if(!json) return RedirectToAction("Index", "Home", new { f = f });
                return Json("cache disabled in config");
            }

            var singleItem = _query.SingleItem(f);
            if (singleItem != null && singleItem.IsDirectory)
            {
                var displayFileFolders = _query.DisplayFileFolders(f);
                _query.RemoveCacheParentItem(displayFileFolders,f);
                if(!json) return RedirectToAction("Index", "Home", new { f = f });
                return Json("cache succesfull cleared");
            }

            if(!json) return RedirectToAction("Index", "Home", new { f = f });
            return BadRequest("ignored, please check if the 'f' path exist or use a folder string to clear the cache");
        }
        
        
    }
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;

        public ApiController(IQuery query, IExiftool exiftool, 
            AppSettings appSettings, IBackgroundTaskQueue queue,
            IReadMeta readMeta, IServiceScopeFactory scopeFactory
            )
        {
            _appSettings = appSettings;
            _query = query;
            _exiftool = exiftool;
            _bgTaskQueue = queue;
            _readMeta = readMeta;
            _scopeFactory = scopeFactory;
        }
        

        /// <summary>
        /// Used for end2end test, show config data
        /// </summary>
        /// <returns>config data, except connection strings</returns>
        [HttpGet]
        [HttpHead]
        [ResponseCache(Duration = 30 )]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous] /// <=================================
        public IActionResult Env()
        {
            return Json(_appSettings);
        }

        
        
        
        /// <summary>
        /// Check the status of a file based on DetailView object
        /// </summary>
        /// <param name="detailView">The element used on the web</param>
        /// <returns>ExifStatus enum</returns>
        private FileIndexItem.ExifStatus FileCollectionsCheck(DetailView detailView)
        {
            if (detailView == null)
            {
                return FileIndexItem.ExifStatus.NotFoundNotInIndex;
            }

            if (detailView.IsDirectory && _appSettings.IsReadOnly(detailView.SubPath))
            {
                return FileIndexItem.ExifStatus.DirReadOnly;
            }

            if (detailView.IsDirectory)
            {
                return FileIndexItem.ExifStatus.NotFoundIsDir;
            }

            if (_appSettings.IsReadOnly(detailView.FileIndexItem.ParentDirectory)) return  FileIndexItem.ExifStatus.ReadOnly;

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
        /// Does deside if the loop should be stopped, true = stop
        /// Uses FileCollectionsCheck
        /// </summary>
        /// <param name="statusModel">the main object to return later</param>
        /// <param name="statusResults">the status by FileCollectionsCheck</param>
        /// <param name="fileIndexResultsList">list of object that will be returned</param>
        /// <returns>If true skip the next code</returns>
        public bool ReturnExifStatusError(FileIndexItem statusModel, FileIndexItem.ExifStatus statusResults, List<FileIndexItem> fileIndexResultsList )
        {
            switch (statusResults)
            {
                case FileIndexItem.ExifStatus.NotFoundIsDir:
                    statusModel.IsDirectory = true;
                    statusModel.Status = FileIndexItem.ExifStatus.NotFoundIsDir;
                    fileIndexResultsList.Add(statusModel);
                    return true;
                case FileIndexItem.ExifStatus.DirReadOnly:
                    statusModel.IsDirectory = true;
                    statusModel.Status = FileIndexItem.ExifStatus.DirReadOnly;
                    fileIndexResultsList.Add(statusModel);
                    return true;
                case FileIndexItem.ExifStatus.NotFoundNotInIndex:
                    statusModel.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
                    fileIndexResultsList.Add(statusModel);
                    return true;
                case FileIndexItem.ExifStatus.NotFoundSourceMissing:
                    statusModel.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
                    fileIndexResultsList.Add(statusModel);
                    return true;
                case FileIndexItem.ExifStatus.ReadOnly:
                    statusModel.Status = FileIndexItem.ExifStatus.ReadOnly;
                    fileIndexResultsList.Add(statusModel);
                    return true;
            }
            return false;
        }

        
        /// <summary>
        /// Add to comparedNames list ++ add to detailview
        /// </summary>
        /// <param name="rotateClock">-1 or 1</param>
        /// <param name="detailView">main db object</param>
        /// <param name="comparedNamesList">list of types that are changes</param>
        private void RotatonCompare(int rotateClock, DetailView detailView, ICollection<string> comparedNamesList)
        {
            // Do orientation / Rotate if needed (after compare)
            if (!FileIndexItem.IsRelativeOrientation(rotateClock)) return;
            // run this on detailview => statusModel is always default
            detailView.FileIndexItem.SetRelativeOrientation(rotateClock);
            comparedNamesList.Add(nameof(detailView.FileIndexItem.Orientation));
        }

        /// <summary>
        /// Run the Orientation changes on the thumbnail (only relative)
        /// </summary>
        /// <param name="rotateClock">-1 or 1</param>
        /// <param name="detailView">object contains filehash</param>
        private void RotationThumbnailExcute(int rotateClock, DetailView detailView)
        {
            var thumbnailFullPath = new Thumbnail(_appSettings).GetThumbnailPath(detailView.FileIndexItem.FileHash);

            // Do orientation
            if(FileIndexItem.IsRelativeOrientation(rotateClock)) new Thumbnail(null).RotateThumbnail(thumbnailFullPath,rotateClock);
        }

        /// <summary>
        /// Add a thumbnail to list to update exif with exiftool
        /// </summary>
        /// <param name="toUpdateFilePath">the fullpath of the source file, only the raw or jpeg</param>
        /// <param name="detailView">main object with filehash</param>
        /// <returns>a list with a thumb full path (if exist) and the source fullpath</returns>
        private List<string> AddThumbnailToExifChangeList(string toUpdateFilePath, DetailView detailView)
        {
            // To Add an Thumbnail to the 'to update list for exiftool'
            var exifUpdateFilePaths = new List<string>
            {
                toUpdateFilePath           
            };
            var thumbnailFullPath = new Thumbnail(_appSettings).GetThumbnailPath(detailView.FileIndexItem.FileHash);
            if (Files.IsFolderOrFile(thumbnailFullPath) == FolderOrFileModel.FolderOrFileTypeList.File)
            {
                exifUpdateFilePaths.Add(thumbnailFullPath);
            }
            return exifUpdateFilePaths;
        }

        /// <summary>
        /// Update Exif and Rotation API
        /// </summary>
        /// <param name="f">subpath filepath to file, split by dot comma (;)</param>
        /// <param name="tags">use for keywords</param>
        /// <param name="colorClass">int 0-9, the colorclass to fast select images</param>
        /// <param name="description">string to update description/caption abstract, emthy will be ignored</param>
        /// <param name="rotateClock">relative orentation -1 or 1</param>
        /// <param name="title">edit image title</param>
        /// <param name="collections">StackCollections bool</param>
        /// <param name="append">only for stings, add update to existing items</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Update(FileIndexItem inputModel, string f, bool append, bool collections,  int rotateClock = 0)
        {
            var inputFilePaths = SplitInputFilePaths(f);
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
                if(ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
                    
                var collectionSubPathList = GetCollectionSubPathList(detailView, collections, subPath);
                var collectionFullPaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);
                
                for (int i = 0; i < collectionSubPathList.Count; i++)
                {
                    // Check if extension is supported for ExtensionExifToolSupportedList
                    // Not all files are able to write with exiftool
                    if(!Files.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
                    {
                        detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
                        fileIndexResultsList.Add(detailView.FileIndexItem);
                        continue;
                    }
                    
                    var comparedNamesList = FileIndexCompareHelper.Compare(detailView.FileIndexItem, statusModel, append);

                    RotatonCompare(rotateClock, detailView, comparedNamesList);
                    
                    // this one is good :)
                    detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;

                    // When it done this will be removed,
                    // to avoid conflicts
                    _readMeta.UpdateReadMetaCache(collectionFullPaths[i],detailView.FileIndexItem);
                    _query.CacheUpdateItem(new List<FileIndexItem>{detailView.FileIndexItem.Clone()});
                    
                    // Update >
                    _bgTaskQueue.QueueBackgroundWorkItem(async token =>
                    {
                        var exiftool = new ExifToolCmdHelper(_appSettings,_exiftool);
                        var toUpdateFilePath = _appSettings.DatabasePathToFilePath(detailView.FileIndexItem.FilePath);

                        var exifUpdateFilePaths = AddThumbnailToExifChangeList(toUpdateFilePath, detailView);

                        RotationThumbnailExcute(rotateClock, detailView);
                        
                        // Do an Exif Sync for all files
                        exiftool.Update(detailView.FileIndexItem, exifUpdateFilePaths , comparedNamesList);
                        
                        // change thumbnail names after the orginal is changed
                        var newFileHash = FileHash.GetHashCode(toUpdateFilePath);
                        new Thumbnail(_appSettings).RenameThumb(detailView.FileIndexItem.FileHash,newFileHash);
                        // Update the hash
                        detailView.FileIndexItem.FileHash = newFileHash;
                        
                        // Do a database sync
                        new QueryBackgroundTask(_scopeFactory,_query).UpdateItem(detailView.FileIndexItem);
                        
                        // > async > force you to read the file again
                        // do not include thumbs in MetaCache
                        // only the full path url of the source image
                        _readMeta.RemoveReadMetaCache(toUpdateFilePath);
                    });
                    
                    // The hash is not correct
                    fileIndexResultsList.Add(detailView.FileIndexItem);
                }
            }
            
            // When all items are not found
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);

            // Clone an new item in the list to display
            var returnNewResultList = new List<FileIndexItem>();
            foreach (var item in fileIndexResultsList)
            {
                var citem = item.Clone();
                citem.FileHash = null;
                returnNewResultList.Add(citem);
            }
                        
            return Json(returnNewResultList);
        }

        /// <summary>
        /// Split a list with devided by dot comma and blank values are removed
        /// </summary>
        /// <param name="f">input filepaths</param>
        /// <returns>string array with sperated strings</returns>
        private string[] SplitInputFilePaths(string f)
        {
            // input devided by dot comma and blank values are removed
            var inputFilePaths = f.Split(";");
            inputFilePaths = inputFilePaths.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            return inputFilePaths;
        }

        /// <summary>
        /// If conllections enalbed return list of subpaths
        /// </summary>
        /// <param name="detailView">the base fileIndexItem</param>
        /// <param name="collections">bool, to enable</param>
        /// <param name="subPath">the file orginal requested in subpath style</param>
        /// <returns></returns>
        private List<string> GetCollectionSubPathList(DetailView detailView, bool collections, string subPath)
        {
            // Paths that are used
            var collectionSubPathList = detailView.FileIndexItem.CollectionPaths;
            // when not running in collections mode only update one file
            if (!collections) collectionSubPathList = new List<string> {subPath};
            return collectionSubPathList;
        }

        /// <summary>
        /// Get realtime (cached a few minutes) about the file
        /// </summary>
        /// <param name="f">subpaths split by dot comma</param>
        /// <param name="collections">true is to update files with the same name before the extenstion</param>
        /// <returns></returns>
        [ResponseCache(Duration = 30, VaryByQueryKeys = new[] {"f"})]
        public IActionResult Info(string f, bool collections = true)
        {
            var inputFilePaths = SplitInputFilePaths(f);
            // the result list
            var fileIndexResultsList = new List<FileIndexItem>();

            foreach (var subPath in inputFilePaths)
            {
                var detailView = _query.SingleItem(subPath, null, collections, false);
                
                // Check if extension is supported for ExtensionExifToolSupportedList
                // Not all files are able to write with exiftool
                if(!Files.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
                {
                    detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
                    fileIndexResultsList.Add(detailView.FileIndexItem);
                    continue;
                }
                var statusResults = FileCollectionsCheck(detailView);

                var statusModel = new FileIndexItem();
                statusModel.SetFilePath(subPath);
                statusModel.IsDirectory = false;

                if(ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
                            
                var collectionSubPathList = GetCollectionSubPathList(detailView, collections, subPath);
                var collectionFullPaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);

                var fileCompontentList = _readMeta.ReadExifAndXmpFromFileAddFilePathHash(collectionFullPaths.ToArray());
                fileIndexResultsList.AddRange(fileCompontentList);
            }

            // returns read only
            if (fileIndexResultsList.All(p => p.Status == FileIndexItem.ExifStatus.ReadOnly))
            {
                Response.StatusCode = 203; // is readonly
                return Json(fileIndexResultsList);
            }
                
            // When all items are not found
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);
            
            return Json(fileIndexResultsList);
        }

        /// <summary>
        /// Remove files from the disk, but the file must contain the !delete! tag
        /// </summary>
        /// <param name="f">subpaths, seperated by dot comma</param>
        /// <param name="collections">true is to update files with the same name before the extenstion</param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(string f, bool collections = true)
        {
            var inputFilePaths = SplitInputFilePaths(f);
            // the result list
            var fileIndexResultsList = new List<FileIndexItem>();

            foreach (var subPath in inputFilePaths)
            {
                var detailView = _query.SingleItem(subPath, null, collections, false);
                var statusResults = FileCollectionsCheck(detailView);

                var statusModel = new FileIndexItem();
                statusModel.SetFilePath(subPath);
                statusModel.IsDirectory = false;

                if(ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
                
                var collectionSubPathList = GetCollectionSubPathList(detailView, collections, subPath);
                var collectionFullDeletePaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);

                // display the to delete items
                for (int i = 0; i < collectionSubPathList.Count; i++)
                {
                    var collectionSubPath = collectionSubPathList[i];
                    var detailViewItem = _query.SingleItem(collectionSubPath, null, collections, false);

                    /// Allow only files that contains the delete tag
                    if (!detailViewItem.FileIndexItem.Tags.Contains("!delete!"))
                    {
                        detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Unauthorized;
                        fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
                        collectionFullDeletePaths[i] = null;
                        continue;
                    }
                    
                    // delete thumb
                    collectionFullDeletePaths.Add(new Thumbnail(_appSettings)
                        .GetThumbnailPath(detailViewItem.FileIndexItem.FileHash));
                    // add to display
                    fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
                    // remove from db
                    _query.RemoveItem(detailViewItem.FileIndexItem);
                }

                
                // Add xmp to file to delete
                var singleFilePath = collectionFullDeletePaths.FirstOrDefault();
                if (singleFilePath == null) continue;
                collectionFullDeletePaths.Add(singleFilePath.Replace(Path.GetExtension(singleFilePath), ".xmp"));
                collectionFullDeletePaths.Add(singleFilePath.Replace(Path.GetExtension(singleFilePath), ".XMP"));

                // Remove the file from disk
                Files.DeleteFile(collectionFullDeletePaths);
            }
            
            // When all items are not found
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);

            return Json(fileIndexResultsList);
        }
  

        /// <summary>
        /// Http Endpoint to get fullsize image or thumbnail
        /// </summary>
        /// <param name="f">one single file</param>
        /// <param name="isSingleitem">true = load orginal</param>
        /// <param name="json">text as output</param>
        /// <param name="retryThumbnail">true = remove thumbnail if corrupt</param>
        /// <returns></returns>
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
                    Response.StatusCode = 202; // A conflict, that the thumb is not generated yet
                    return Json("Thumbnail is not ready yet");
                }
                
                if (Files.IsExtensionThumbnailSupported(sourceFullPath))
                {
                    FileStream fs1 = System.IO.File.OpenRead(sourceFullPath);
                    var fileExtensionWithoutDot = Path.GetExtension(sourceFullPath).Remove(0, 1).ToLower();
                    return File(fs1, MimeHelper.GetMimeType(fileExtensionWithoutDot));
                }
                Response.StatusCode = 409; // A conflict, that the thumb is not generated yet
                return Json("Thumbnail is not supported; for example you try to view a raw file");
            }

            return NotFound("There is no thumbnail image " + thumbPath + " and no source image "+ sourcePath );
            // When you have duplicate files and one of them is removed and there is no thumbnail generated yet you might get an false error
        }

        /// <summary>
        /// Force Http context to no browser cache
        /// </summary>
        public void SetExpiresResponseHeadersToZero()
        {
            Request.HttpContext.Response.Headers.Remove("Cache-Control");
            Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

            Request.HttpContext.Response.Headers.Remove("Pragma");
            Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

            Request.HttpContext.Response.Headers.Remove("Expires");
            Request.HttpContext.Response.Headers.Add("Expires", "0");
        }

        /// <summary>
        /// Select manualy the orginal or thumbnail
        /// </summary>
        /// <param name="f">string, subpath to find the file</param>
        /// <param name="isThumbnail">true = 1000px thumb (if supported)</param>
        /// <returns>FileStream with image</returns>
        [HttpGet]
        [HttpHead]
        public IActionResult DownloadPhoto(string f, bool isThumbnail = true)
        {
            // f = subpath/filepath
            if (f.Contains("?isthumbnail")) return NotFound("please use &isthumbnail = "+
                                                            "instead of ?isthumbnail= ");

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
                    
                var isSuccesCreateAThumb = new Thumbnail(_appSettings,_exiftool).CreateThumb(searchItem);
                if (!isSuccesCreateAThumb)
                {
                    Response.StatusCode = 500;
                    return Json("Thumbnail generation failed");
                }

                FileStream fs2 = System.IO.File.OpenRead(thumbPath);
                return File(fs2, "image/jpeg");
            }

            FileStream fs1 = System.IO.File.OpenRead(thumbPath);
            return File(fs1, "image/jpeg");
        }

        /// <summary>
        /// Delete Database Cache
        /// </summary>
        /// <param name="f">subpath</param>
        /// <param name="json">return status</param>
        /// <returns>redirect or if json enabled a status</returns>
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

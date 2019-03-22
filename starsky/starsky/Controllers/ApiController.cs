﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;

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
	    private readonly IStorage _iStorage;

        public ApiController(
            IQuery query, IExiftool exiftool, 
            AppSettings appSettings, IBackgroundTaskQueue queue,
            IReadMeta readMeta,
			IStorage iStorage)
        {
            _appSettings = appSettings;
            _query = query;
            _exiftool = exiftool;
            _bgTaskQueue = queue;
            _readMeta = readMeta;
	        _iStorage = iStorage;

        }

	    /// <summary>
	    /// The database-view of a directory
	    /// </summary>
	    /// <param name="f">subPath</param>
	    /// <param name="colorClass">filter on colorClass (use int)</param>
	    /// <param name="json">to not show as webpage</param>
	    /// <param name="collections">to combine files with the same name before the extension</param>
	    /// <param name="hidedelete">ignore deleted files</param>
	    /// <returns></returns>
	    /// <response code="200">returns a list of items from the database</response>
	    /// <response code="404">subpath not found in the database</response>
		[HttpGet("/api")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(ArchiveViewModel),200)]
		[ProducesResponseType(404)]
		public IActionResult Index(
			string f = "/",
			string colorClass = null,
			bool json = true,
			bool collections = true,
			bool hidedelete = true
		)
		{
			return new HomeController(_query, _appSettings).Index(f, colorClass, json, collections, hidedelete);
		}

	    /// <summary>
        /// Show the runtime settings (allow AllowAnonymous)
        /// </summary>
        /// <returns>config data, except connection strings</returns>
	    /// <response code="200">returns the runtime settings of Starsky</response>
	    [HttpHead("/api/env")]
        [HttpGet("/api/env")]
        [IgnoreAntiforgeryToken]
	    [Produces("application/json")]
	    [ProducesResponseType(typeof(AppSettings),200)]
        [AllowAnonymous] // <----------------------------------------
        public IActionResult Env()
	    {
		    var appSettings = _appSettings.CloneToDisplay();
            return Json(appSettings);
        }
        






        /// <summary>
        /// Update Exif and Rotation API
        /// </summary>
        /// <param name="f">subPath filepath to file, split by dot comma (;)</param>
        /// <param name="inputModel">tags: use for keywords
        /// colorClass: int 0-9, the colorClass to fast select images
        /// description: string to update description/caption abstract, empty will be ignore
        /// title: edit image title</param>
        /// <param name="collections">StackCollections bool, default true</param>
        /// <param name="append">only for stings, add update to existing items</param>
        /// <param name="rotateClock">relative orientation -1 or 1</param>
        /// <returns>update json</returns>
        /// <response code="200">the item including the updated content</response>
        /// <response code="404">item not found in the database or on disk</response>
        [IgnoreAntiforgeryToken]
		[ProducesResponseType(typeof(List<FileIndexItem>),200)]
		[ProducesResponseType(typeof(List<FileIndexItem>),404)]
		[HttpPost("/api/update")]
        [Produces("application/json")]
		public IActionResult Update(FileIndexItem inputModel, string f, bool append, bool collections = true,  int rotateClock = 0)
		{
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
			
			// Per file stored  <string{fileHash}, List<string>{FileIndexItem.name (e.g. Tags) that are changed}
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			
			foreach (var subPath in inputFilePaths)
			{
				var detailView = _query.SingleItem(subPath,null,collections,false);
				var statusResults = new StatusCodesHelper(_appSettings,_iStorage).FileCollectionsCheck(detailView);
				
				var statusModel = inputModel.Clone();
				statusModel.IsDirectory = false;
				statusModel.SetFilePath(subPath);
				
				// if one item fails, the status will added
				if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;

				if ( detailView == null ) throw new ArgumentNullException(nameof(detailView));
				
				
				var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
                
				// loop to update
				foreach ( var collectionSubPath in collectionSubPathList )
				{
					var collectionsDetailView = _query.SingleItem(collectionSubPath, null, collections, false);
					
					// Check if extension is supported for ExtensionExifToolSupportedList
					// Not all files are able to write with exiftool
					if(!ExtensionRolesHelper.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
					{
						collectionsDetailView.FileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
						fileIndexResultsList.Add(detailView.FileIndexItem);
						continue;
					}

					// Compare Rotation and All other tags
					new UpdateService(_query, _exiftool, _appSettings, _readMeta,_iStorage)
						.CompareAllLabelsAndRotation(changedFileIndexItemName,
							collectionsDetailView, statusModel, append, rotateClock);
					
					// this one is good :)
					collectionsDetailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
					
					// When it done this will be removed,
					// to avoid conflicts
					_readMeta.UpdateReadMetaCache(collectionSubPath,collectionsDetailView.FileIndexItem);
					
					// update database cache
					_query.CacheUpdateItem(new List<FileIndexItem>{collectionsDetailView.FileIndexItem});
					
					// The hash in FileIndexItem is not correct
					fileIndexResultsList.Add(collectionsDetailView.FileIndexItem);
				}
            }
			
			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				new UpdateService(_query,_exiftool,_appSettings, _readMeta,_iStorage)
					.Update(changedFileIndexItemName,fileIndexResultsList,inputModel,collections, append, rotateClock);
			});
            
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
	    /// Work in progress: Search and Replace text
	    /// </summary>
	    /// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	    /// <param name="fieldName">name of fileIndexItem field e.g. Tags</param>
	    /// <param name="search">text to search for</param>
	    /// <param name="replace">replace [search] with this text</param>
	    /// <param name="collections">enable collections</param>
	    /// <returns>list of changed files</returns>
	    [HttpPost("/api/replace")]
	    [Produces("application/json")]
	    public IActionResult Replace(string f, string fieldName, string search, string replace, bool collections = true)
	    {
		    var fileIndexResultsList = new ReplaceService(_query, _appSettings, _iStorage)
			    .Replace(f, fieldName, search, replace, collections);
		    
			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				var resultsOkList =
					fileIndexResultsList.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList();
				
				foreach ( var inputModel in resultsOkList )
				{
					// The differences are specified before update
					var changedFileIndexItemName = new Dictionary<string, List<string>>
					{
						{ 
							inputModel.FilePath, new List<string>
							{
								fieldName
							} 
						}
					};
					
					new UpdateService(_query,_exiftool,_appSettings, _readMeta,_iStorage)
						.Update(changedFileIndexItemName,new List<FileIndexItem>{inputModel}, inputModel, collections, false, 0);
					
				}
			});
					
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
        /// Get realtime (cached a few minutes) about the file
        /// </summary>
        /// <param name="f">subPaths split by dot comma</param>
        /// <param name="collections">true is to update files with the same name before the extenstion</param>
        /// <returns>info of object</returns>
        /// <response code="200">the item on disk</response>
        /// <response code="404">item not found on disk</response>
        /// <response code="203">you are not allowed to edit this item</response>
        [HttpGet("/api/info")]
        [ProducesResponseType(typeof(List<FileIndexItem>),200)]
        [ProducesResponseType(typeof(List<FileIndexItem>),404)]
        [ProducesResponseType(typeof(List<FileIndexItem>),203)]
        [Produces("application/json")]
        public IActionResult Info(string f, bool collections = true)
        {
            var inputFilePaths = PathHelper.SplitInputFilePaths(f);
            // the result list
            var fileIndexResultsList = new List<FileIndexItem>();

            foreach (var subPath in inputFilePaths)
            {
                var detailView = _query.SingleItem(subPath, null, collections, false);
                
                // Check if extension is supported for ExtensionExifToolSupportedList
                // Not all files are able to write with exifTool
                if(detailView != null && !ExtensionRolesHelper.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
                {
                    detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
                    fileIndexResultsList.Add(detailView.FileIndexItem);
                    continue;
                }
                var statusResults = new StatusCodesHelper(_appSettings,_iStorage).FileCollectionsCheck(detailView);

                var statusModel = new FileIndexItem(subPath);

                if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
	            
	            if ( detailView == null ) throw new ArgumentNullException(nameof(detailView));

                var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);

	            foreach ( var collectionSubPath in collectionSubPathList )
	            {
		            var collectionItem = new FileIndexItem(collectionSubPath)
		            {
			            Status = FileIndexItem.ExifStatus.Ok,
			            CollectionPaths = collectionSubPathList,
			            ImageFormat = ExtensionRolesHelper.MapFileTypesToExtension(collectionSubPath)
		            };
		            fileIndexResultsList.Add(_readMeta.ReadExifAndXmpFromFile(collectionItem));
	            }
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
        /// <returns>list of deleted files</returns>
        /// <response code="200">file is gone</response>
        /// <response code="404">item not found on disk or !delete! tag is missing</response>
        [HttpDelete("/api/delete")]
        [ProducesResponseType(typeof(List<FileIndexItem>),200)]
        [ProducesResponseType(typeof(List<FileIndexItem>),404)]
        [Produces("application/json")]
        public IActionResult Delete(string f, bool collections = true)
        {
            var inputFilePaths = PathHelper.SplitInputFilePaths(f);
            // the result list
            var fileIndexResultsList = new List<FileIndexItem>();

            foreach (var subPath in inputFilePaths)
            {
                var detailView = _query.SingleItem(subPath, null, collections, false);
                var statusResults = new StatusCodesHelper(_appSettings,_iStorage).FileCollectionsCheck(detailView);

                var statusModel = new FileIndexItem(subPath);

                if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
                
                var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
                var collectionFullDeletePaths = _appSettings.DatabasePathToFilePath(collectionSubPathList);

                // display the to delete items
                for (int i = 0; i < collectionSubPathList.Count; i++)
                {
                    var collectionSubPath = collectionSubPathList[i];
                    var detailViewItem = _query.SingleItem(collectionSubPath, null, collections, false);

                    // Allow only files that contains the delete tag
                    if (!detailViewItem.FileIndexItem.Tags.Contains("!delete!"))
                    {
                        detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Unauthorized;
                        fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
                        collectionFullDeletePaths[i] = null;
                        continue;
                    }
	                // return a Ok, which means the file is deleted
	                detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;

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
                FilesHelper.DeleteFile(collectionFullDeletePaths);
            }
            
            // When all items are not found
	        // ok = file is deleted
            if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
                return NotFound(fileIndexResultsList);

            return Json(fileIndexResultsList);
        }
  

        /// <summary>
        /// Http Endpoint to get full size image or thumbnail
        /// </summary>
        /// <param name="f">one single file</param>
        /// <param name="isSingleitem">true = load orginal</param>
        /// <param name="json">text as output</param>
        /// <param name="retryThumbnail">true = remove thumbnail if corrupt</param>
        /// <returns>thumbnail or status</returns>
        /// <response code="200">returns content of the file or when json is true, "OK"</response>
        /// <response code="404">item not found on disk</response>
        /// <response code="409">Conflict, you did try get for example a thumbnail of a raw file</response>
        /// <response code="209">"Thumbnail is not ready yet"</response>
        [HttpGet("/api/thumbnail/{f}")]
        [ProducesResponseType(200)] // file
        [ProducesResponseType(404)] // not found
        [ProducesResponseType(409)] // raw
        [ProducesResponseType(209)] // "Thumbnail is not ready yet"
        [IgnoreAntiforgeryToken]
        public IActionResult Thumbnail(
            string f, 
            bool isSingleitem = false, 
            bool json = false,
            bool retryThumbnail = false)
        {
            // f is Hash
            // isSingleItem => detailView
            // Retry thumbnail => is when you press reset thumbnail
            // json, => to don't waste the users bandwith.

	        // For serving jpeg files
	        if ( Path.HasExtension(f) && Path.GetExtension(f) == ".jpg" )
	        {
		        f = f.Remove(f.Length - 4);
	        }
	        
            var thumbPath = _appSettings.ThumbnailTempFolder + f + ".jpg";

            if (FilesHelper.IsFolderOrFile(thumbPath) == FolderOrFileModel.FolderOrFileTypeList.File)
            {
                // When a file is corrupt show error + Delete
                var imageFormat = ExtensionRolesHelper.GetImageFormat(thumbPath);
                if (imageFormat == ExtensionRolesHelper.ImageFormat.unknown)
                {
                    if (!retryThumbnail)
                    {
                        Console.WriteLine("image is corrupt");
                        SetExpiresResponseHeadersToZero();
                        return NoContent(); // 204
                    }
                    System.IO.File.Delete(thumbPath);
                }
                
                // When using the api to check using javascript
                // use the cached version of imageFormat, otherwise you have to check if it deleted
                if (imageFormat != ExtensionRolesHelper.ImageFormat.unknown)
                {
                    if (json) return Json("OK");

                    // thumbs are always in jpeg
                    FileStream fs = System.IO.File.OpenRead(thumbPath);
                    return File(fs, "image/jpeg");
                }
            }
            
            // Cached view of item
            var sourcePath = _query.GetSubPathByHash(f);
            if (sourcePath == null) return NotFound("not in index");
            var sourceFullPath = _appSettings.DatabasePathToFilePath(sourcePath);

	        
	        // Need to check again for recently moved files
	        if (!System.IO.File.Exists(sourceFullPath))
	        {
		        // remove from cache
		        _query.ResetItemByHash(f);
		        // query database agian
		        sourcePath = _query.GetSubPathByHash(f);
		        if (sourcePath == null) return NotFound("not in index");
		        sourceFullPath = _appSettings.DatabasePathToFilePath(sourcePath);
	        }

            if (System.IO.File.Exists(sourceFullPath))
            {
                if (!isSingleitem)
                {
                    // "Photo exist in database but " + "isSingleItem flag is Missing"
                    SetExpiresResponseHeadersToZero();
                    Response.StatusCode = 202; // A conflict, that the thumb is not generated yet
                    return Json("Thumbnail is not ready yet");
                }
                
                if (ExtensionRolesHelper.IsExtensionThumbnailSupported(sourceFullPath))
                {
                    FileStream fs1 = System.IO.File.OpenRead(sourceFullPath);
                    var fileExtensionWithoutDot = Path.GetExtension(sourceFullPath).Remove(0, 1).ToLower();
                    return File(fs1, MimeHelper.GetMimeType(fileExtensionWithoutDot));
                }
                Response.StatusCode = 409; // A conflict, that the thumb is not generated yet
                return Json("Thumbnail is not supported; for example you try to view a raw file");
            }

            return NotFound("There is no thumbnail image " + thumbPath + " and no source image "+ sourcePath );
            // When you have duplicate files and one of them is removed and there is no thumbnail
            // generated yet you might get an false error
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
        /// <response code="200">returns content of the file or when json is true, "OK"</response>
        /// <response code="404">source image missing</response>
        /// <response code="500">"Thumbnail generation failed"</response>
        [HttpGet("/api/downloadPhoto")]
        [ProducesResponseType(200)] // file
        [ProducesResponseType(404)] // not found
        [ProducesResponseType(500)] // "Thumbnail generation failed"
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
            if (ExtensionRolesHelper.GetImageFormat(thumbPath) == ExtensionRolesHelper.ImageFormat.unknown)
            {
                System.IO.File.Delete(thumbPath);
            }

            if (ExtensionRolesHelper.GetImageFormat(thumbPath) == ExtensionRolesHelper.ImageFormat.notfound)
            {
                if (FilesHelper.IsFolderOrFile(_appSettings.ThumbnailTempFolder) ==
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
	                FileHash = singleItem.FileIndexItem.FileHash // not loading it from disk to make it faster
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
        /// Delete Database Cache (only the cache)
        /// </summary>
        /// <param name="f">subpath</param>
        /// <param name="json">return status</param>
        /// <returns>redirect or if json enabled a status</returns>
        /// <response code="200">when json is true, "cache successful cleared"</response>
        /// <response code="412">"cache disabled in config"</response>
        /// <response code="400">ignored, please check if the 'f' path exist or use a folder string to clear the cache</response>
        /// <response code="302">redirect back to the url</response>
        [HttpGet("/api/RemoveCache")]
        [HttpPost("/api/RemoveCache")]
        [ProducesResponseType(200)] // "cache successful cleared"
        [ProducesResponseType(412)] // "cache disabled in config"
        [ProducesResponseType(400)] // "ignored, please check if the 'f' path exist or use a folder string to clear the cache"
        [ProducesResponseType(302)] // redirect back to the url
        public IActionResult RemoveCache(string f = "/", bool json = false)
        {
            //For folder paths only
            if (!_appSettings.AddMemoryCache)
            {
				Response.StatusCode = 412;
				if(!json) return RedirectToAction("Index", "Home", new { f });
				return Json("cache disabled in config");
            }

            var singleItem = _query.SingleItem(f);
            if (singleItem != null && singleItem.IsDirectory)
            {
                _query.RemoveCacheParentItem(f);
                if(!json) return RedirectToAction("Index", "Home", new { f });
                return Json("cache successful cleared");
            }

            if(!json) return RedirectToAction("Index", "Home", new { f });
            return BadRequest("ignored, please check if the 'f' path exist or use a folder string to clear the cache");
        }

    }
}

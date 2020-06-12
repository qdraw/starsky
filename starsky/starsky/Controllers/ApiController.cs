using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.update.Services;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Helpers;
using starskycore.Services;

namespace starsky.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        private readonly IQuery _query;
        private readonly IExifTool _exifTool;
        private readonly AppSettings _appSettings;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
        private readonly IReadMeta _readMeta;
	    private readonly IStorage _iStorage;
	    private readonly IStorage _thumbnailStorage;

	    public ApiController(
            IQuery query, IExifTool exifTool, 
            AppSettings appSettings, IBackgroundTaskQueue queue,
			ISelectorStorage selectorStorage, IMemoryCache memoryCache)
        {
            _appSettings = appSettings;
            _query = query;
            _exifTool = exifTool;
            _bgTaskQueue = queue;
            _iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
            _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
            _readMeta = new ReadMeta(_iStorage,_appSettings, memoryCache);
        }
       
  //       /// <summary>
  //       /// Update Exif and Rotation API
  //       /// </summary>
  //       /// <param name="f">subPath filepath to file, split by dot comma (;)</param>
  //       /// <param name="inputModel">tags: use for keywords
  //       /// colorClass: int 0-9, the colorClass to fast select images
  //       /// description: string to update description/caption abstract, empty will be ignore
  //       /// title: edit image title</param>
  //       /// <param name="collections">StackCollections bool, default true</param>
  //       /// <param name="append">only for stings, add update to existing items</param>
  //       /// <param name="rotateClock">relative orientation -1 or 1</param>
  //       /// <returns>update json</returns>
  //       /// <response code="200">the item including the updated content</response>
  //       /// <response code="404">item not found in the database or on disk</response>
  //       /// <response code="401">User unauthorized</response>
  //       [IgnoreAntiforgeryToken]
		// [ProducesResponseType(typeof(List<FileIndexItem>),200)]
		// [ProducesResponseType(typeof(List<FileIndexItem>),404)]
		// [HttpPost("/api/update-legacy")]
  //       [Produces("application/json")]
		// public IActionResult Update(FileIndexItem inputModel, string f, bool append, bool collections = true,  int rotateClock = 0)
		// {
		// 	var inputFilePaths = PathHelper.SplitInputFilePaths(f);
		// 	// the result list
		// 	var fileIndexResultsList = new List<FileIndexItem>();
		// 	
		// 	// Per file stored key = string[fileHash] item => List <string> FileIndexItem.name (e.g. Tags) that are changed
		// 	var changedFileIndexItemName = new Dictionary<string, List<string>>();
		// 	
		// 	foreach (var subPath in inputFilePaths)
		// 	{
		// 		var detailView = _query.SingleItem(subPath,null,collections,false);
		// 		var statusResults = new StatusCodesHelper(_appSettings,_iStorage).FileCollectionsCheck(detailView);
		// 		
		// 		var statusModel = inputModel.Clone();
		// 		statusModel.IsDirectory = false;
		// 		statusModel.SetFilePath(subPath);
		// 		
  //
		// 		// Readonly is not allowed
		// 		if(new StatusCodesHelper().ReadonlyDenied(statusModel, statusResults, fileIndexResultsList)) continue;
		// 		
		// 		// if one item fails, the status will added
		// 		if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
  //
		// 		if ( detailView == null ) throw new InvalidDataException("DetailView is null " + nameof(detailView));
		// 		
		// 		
		// 		var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
  //               
		// 		// loop to update
		// 		foreach ( var collectionSubPath in collectionSubPathList )
		// 		{
		// 			var collectionsDetailView = _query.SingleItem(collectionSubPath, null, collections, false);
		// 			
		// 			// // Check if extension is supported for ExtensionExifToolSupportedList
		// 			// // Not all files are able to write with exifTool
		// 			// if(!ExtensionRolesHelper.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
		// 			// {
		// 			// 	collectionsDetailView.FileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
		// 			// 	fileIndexResultsList.Add(detailView.FileIndexItem);
		// 			// 	continue;
		// 			// }
  //
		// 			// Compare Rotation and All other tags
		// 			new UpdateService(_query, _exifTool, _readMeta,_iStorage, _thumbnailStorage)
		// 				.CompareAllLabelsAndRotation(changedFileIndexItemName,
		// 					collectionsDetailView, statusModel, append, rotateClock);
		// 			
		// 			// this one is good :)
		// 			collectionsDetailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
		// 			
		// 			// When it done this will be removed,
		// 			// to avoid conflicts
		// 			_readMeta.UpdateReadMetaCache(collectionSubPath,collectionsDetailView.FileIndexItem);
		// 			
		// 			// update database cache
		// 			_query.CacheUpdateItem(new List<FileIndexItem>{collectionsDetailView.FileIndexItem});
		// 			
		// 			// The hash in FileIndexItem is not correct
		// 			fileIndexResultsList.Add(collectionsDetailView.FileIndexItem);
		// 		}
  //           }
		// 	
		// 	// Update >
		// 	_bgTaskQueue.QueueBackgroundWorkItem(async token =>
		// 	{
		// 		new UpdateService(_query,_exifTool, _readMeta,_iStorage,_thumbnailStorage)
		// 			.Update(changedFileIndexItemName,fileIndexResultsList,inputModel,collections, append, rotateClock);
		// 	});
  //           
  //           // When all items are not found
  //           if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
  //               return NotFound(fileIndexResultsList);
  //
  //           // Clone an new item in the list to display
  //           var returnNewResultList = new List<FileIndexItem>();
  //           foreach ( var cloneItem in fileIndexResultsList.Select(item => item.Clone()) )
  //           {
	 //            cloneItem.FileHash = null;
	 //            returnNewResultList.Add(cloneItem);
  //           }
  //                       
  //           return Json(returnNewResultList);
  //       }

	    
	    /// <summary>
	    /// Search and Replace text in meta information 
	    /// </summary>
	    /// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	    /// <param name="fieldName">name of fileIndexItem field e.g. Tags</param>
	    /// <param name="search">text to search for</param>
	    /// <param name="replace">replace [search] with this text</param>
	    /// <param name="collections">enable collections</param>
	    /// <returns>list of changed files</returns>
	    /// <response code="200">Initialized replace job</response>
	    /// <response code="404">item(s) not found</response>
	    /// <response code="401">User unauthorized</response>
	    [HttpPost("/api/replace")]
	    [ProducesResponseType(typeof(List<FileIndexItem>),200)]
	    [ProducesResponseType(typeof(List<FileIndexItem>),404)]
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
					
					new UpdateService(_query,_exifTool, _readMeta,_iStorage,_thumbnailStorage)
						.Update(changedFileIndexItemName,new List<FileIndexItem>{inputModel}, inputModel, collections, false, 0);
					
				}
			});
					
			// When all items are not found
			if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
			{
				return NotFound(fileIndexResultsList);
			}

			// Clone an new item in the list to display
			var returnNewResultList = new List<FileIndexItem>();
			foreach ( var clonedItem in fileIndexResultsList.Select(item => item.Clone()) )
			{
				clonedItem.FileHash = null;
				returnNewResultList.Add(clonedItem);
			}
			
			return Json(returnNewResultList);
		}

	   


        // /// <summary>
        // /// Get realtime (cached a few minutes) about the file
        // /// </summary>
        // /// <param name="f">subPaths split by dot comma</param>
        // /// <param name="collections">true is to update files with the same name before the extenstion</param>
        // /// <returns>info of object</returns>
        // /// <response code="200">the item on disk</response>
        // /// <response code="404">item not found on disk</response>
        // /// <response code="203">you are not allowed to edit this item</response>
        // /// <response code="401">User unauthorized</response>
        // [HttpGet("/api/info")]
        // [ProducesResponseType(typeof(List<FileIndexItem>),200)]
        // [ProducesResponseType(typeof(List<FileIndexItem>),404)]
        // [ProducesResponseType(typeof(List<FileIndexItem>),203)]
        // [Produces("application/json")]
        // public IActionResult Info(string f, bool collections = true)
        // {
        //     var inputFilePaths = PathHelper.SplitInputFilePaths(f);
        //     // the result list
        //     var fileIndexResultsList = new List<FileIndexItem>();
        //
        //     foreach (var subPath in inputFilePaths)
        //     {
        //         var detailView = _query.SingleItem(subPath, null, collections, false);
        //        
        //         // Check if extension is supported for ExtensionExifToolSupportedList
        //         // Not all files are able to write with exifTool
        //         if(detailView != null && !ExtensionRolesHelper.IsExtensionExifToolSupported(detailView.FileIndexItem.FileName))
        //         {
	       //          detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
	       //          fileIndexResultsList.Add(detailView.FileIndexItem);
	       //          continue;
        //         }
        //
        //         var statusResults = new StatusCodesHelper(_appSettings,_iStorage).FileCollectionsCheck(detailView);
        //
        //         var statusModel = new FileIndexItem(subPath);
        //
        //         if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
	       //      
	       //      if ( detailView == null ) throw new InvalidDataException("Detailview is null " + nameof(detailView));
        //
        //         var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
        //
	       //      foreach ( var collectionSubPath in collectionSubPathList )
	       //      {
		      //       var collectionItem = _readMeta.ReadExifAndXmpFromFile(collectionSubPath);
		      //       
		      //       collectionItem.Status = statusResults;
		      //       collectionItem.CollectionPaths = collectionSubPathList;
		      //       collectionItem.ImageFormat =
			     //        ExtensionRolesHelper.MapFileTypesToExtension(collectionSubPath);
        //
		      //       fileIndexResultsList.Add(collectionItem);
	       //      }
        //     }
        //
        //     // returns read only
        //     if (fileIndexResultsList.All(p => p.Status == FileIndexItem.ExifStatus.ReadOnly))
        //     {
        //         Response.StatusCode = 203; // is readonly
        //         return Json(fileIndexResultsList);
        //     }
        //         
        //     // When all items are not found
        //     if (fileIndexResultsList.All(p => (p.Status != FileIndexItem.ExifStatus.Ok && p.Status != FileIndexItem.ExifStatus.Deleted)))
        //         return NotFound(fileIndexResultsList);
        //     
        //     return Json(fileIndexResultsList);
        // }

     //    /// <summary>
     //    /// Remove files from the disk, but the file must contain the !delete! tag
     //    /// </summary>
     //    /// <param name="f">subpaths, seperated by dot comma</param>
     //    /// <param name="collections">true is to update files with the same name before the extenstion</param>
     //    /// <returns>list of deleted files</returns>
     //    /// <response code="200">file is gone</response>
     //    /// <response code="404">item not found on disk or !delete! tag is missing</response>
     //    /// <response code="401">User unauthorized</response>
     //    [HttpDelete("/api/delete")]
     //    [ProducesResponseType(typeof(List<FileIndexItem>),200)]
     //    [ProducesResponseType(typeof(List<FileIndexItem>),404)]
     //    [Produces("application/json")]
     //    public IActionResult Delete(string f, bool collections = true)
     //    {
     //        var inputFilePaths = PathHelper.SplitInputFilePaths(f);
     //        // the result list
     //        var fileIndexResultsList = new List<FileIndexItem>();
     //
     //        foreach (var subPath in inputFilePaths)
     //        {
     //            var detailView = _query.SingleItem(subPath, null, collections, false);
     //            var statusResults = new StatusCodesHelper(_appSettings,_iStorage).FileCollectionsCheck(detailView);
     //
     //            var statusModel = new FileIndexItem(subPath);
     //
     //            // Readonly is not allowed
     //            if(new StatusCodesHelper().ReadonlyDenied(statusModel, statusResults, fileIndexResultsList)) continue;
     //
     //            if(new StatusCodesHelper().ReturnExifStatusError(statusModel, statusResults, fileIndexResultsList)) continue;
     //            
     //            var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
     //
     //            // display the to delete items
     //            for (int i = 0; i < collectionSubPathList.Count; i++)
     //            {
     //                var collectionSubPath = collectionSubPathList[i];
     //                var detailViewItem = _query.SingleItem(collectionSubPath, null, collections, false);
     //
     //                // Allow only files that contains the delete tag
     //                if (!detailViewItem.FileIndexItem.Tags.Contains("!delete!"))
     //                {
     //                    detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Unauthorized;
     //                    fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
     //                    continue;
     //                }
	    //             // return a Ok, which means the file is deleted
	    //             detailViewItem.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
     //
					// // remove thumbnail from disk
					// _thumbnailStorage.FileDelete(detailViewItem.FileIndexItem.FileHash);
     //
     //                fileIndexResultsList.Add(detailViewItem.FileIndexItem.Clone());
	    //             
     //                // remove item from db
     //                _query.RemoveItem(detailViewItem.FileIndexItem);
     //
	    //             // remove the sidecar file (if exist)
	    //             if ( ExtensionRolesHelper.IsExtensionForceXmp(detailViewItem.FileIndexItem
		   //              .FileName) )
	    //             {
		   //              _iStorage.FileDelete(
			  //               ExtensionRolesHelper.ReplaceExtensionWithXmp(detailViewItem
				 //                .FileIndexItem.FilePath));
	    //             }
	    //             
	    //             // and remove the actual file
	    //             _iStorage.FileDelete(detailViewItem.FileIndexItem.FilePath);
     //            }
     //        }
     //        
     //        // When all items are not found
	    //     // ok = file is deleted
     //        if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
     //            return NotFound(fileIndexResultsList);
     //
     //        return Json(fileIndexResultsList);
     //    }
  
        /// <summary>
        /// Delete Database Cache (only the cache)
        /// </summary>
        /// <param name="f">subpath</param>
        /// <returns>redirect or if json enabled a status</returns>
        /// <response code="200">when json is true, "cache successful cleared"</response>
        /// <response code="412">"cache disabled in config"</response>
        /// <response code="400">ignored, please check if the 'f' path exist or use a folder string to clear the cache</response>
        /// <response code="302">redirect back to the url</response>
        /// <response code="401">User unauthorized</response>
        [HttpGet("/api/RemoveCache")]
        [HttpPost("/api/RemoveCache")]
        [ProducesResponseType(200)] // "cache successful cleared"
        [ProducesResponseType(412)] // "cache disabled in config"
        [ProducesResponseType(400)] // "ignored, please check if the 'f' path exist or use a folder string to clear the cache"
        [ProducesResponseType(302)] // redirect back to the url
        public IActionResult RemoveCache(string f = "/")
        {
            //For folder paths only
            if (!_appSettings.AddMemoryCache)
            {
				Response.StatusCode = 412;
				return Json("cache disabled in config");
            }

            var singleItem = _query.SingleItem(f);
            if (singleItem != null && singleItem.IsDirectory)
            {
                _query.RemoveCacheParentItem(f);
                return Json("cache successful cleared");
            }

            return BadRequest("ignored, please check if the 'f' path exist or use a folder string to clear the cache");
        }

    }
}

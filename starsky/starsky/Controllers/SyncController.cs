using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.rename.Services;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Services;
using starskycore.ViewModels;

namespace starsky.Controllers
{
    [Authorize]
    public class SyncController : Controller
    {
        private readonly ISync _sync;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
        private readonly IQuery _query;
	    private readonly IStorage _iStorage;

        public SyncController(ISync sync, IBackgroundTaskQueue queue, IQuery query, ISelectorStorage selectorStorage)
        {
            _sync = sync;
            _bgTaskQueue = queue;
            _query = query;
	        _iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
        }

        /// <summary>
        /// Make a directory (-p)
        /// </summary>
        /// <param name="f">subPaths split by dot comma</param>
        /// <returns>list of changed files</returns>
        /// <response code="200">create the item on disk and in db</response>
        /// <response code="409">A conflict, Directory already exist</response>
        /// <response code="401">User unauthorized</response>
        [HttpPost("/sync/mkdir")]
        [ProducesResponseType(typeof(List<SyncViewModel>),200)]
        [ProducesResponseType(typeof(List<SyncViewModel>),409)]
        [ProducesResponseType(typeof(string),401)]
        [Produces("application/json")]	    
        public IActionResult Mkdir(string f)
        {
	        var inputFilePaths = PathHelper.SplitInputFilePaths(f).ToList();
	        var syncResultsList = new List<SyncViewModel>();

	        foreach ( var subPath in inputFilePaths.Select(subPath => PathHelper.RemoveLatestSlash(subPath)) )
	        {

		        var toAddStatus = new SyncViewModel
		        {
			        FilePath = subPath, 
			        Status = FileIndexItem.ExifStatus.Ok
		        };
			        
		        if ( _iStorage.ExistFolder(subPath) )
		        {
			        toAddStatus.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			        syncResultsList.Add(toAddStatus);
			        continue;
		        }
		        
		        // add to fs
		        _iStorage.CreateDirectory(subPath);
		        
		        // add to db
		        _sync.AddSubPathFolder(subPath);
		        
		        syncResultsList.Add(toAddStatus);
	        }
	        
	        // When all items are not found
	        if (syncResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
		        Response.StatusCode = 409; // A conflict, Directory already exist
	        
	        return Json(syncResultsList);
        }

        /// <summary>
        /// Do a file sync in a background process
        /// </summary>
        /// <param name="f">subPaths split by dot comma</param>
        /// <returns>list of changed files</returns>
        /// <response code="200">started sync as background job</response>
        /// <response code="401">User unauthorized</response>
        [ActionName("Index")]
        [ProducesResponseType(typeof(List<SyncViewModel>),200)]
        [ProducesResponseType(typeof(string),401)]
        [Produces("application/json")]	    
        public IActionResult SyncIndex(string f)
        {
            var inputFilePaths = PathHelper.SplitInputFilePaths(f).ToList();
            // the result list
            var syncResultsList = new List<SyncViewModel>();

            for (int i = 0; i < inputFilePaths.Count; i++)
            {
                var subPath = inputFilePaths[i];
	            subPath = PathHelper.RemoveLatestSlash(subPath);
	            if ( subPath == string.Empty ) subPath = "/";

	            var folderStatus = _iStorage.IsFolderOrFile(subPath);
				if ( folderStatus == FolderOrFileModel.FolderOrFileTypeList.Deleted )
				{
					var syncItem = new SyncViewModel
					{
						FilePath = subPath,
						Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
					};
					syncResultsList.Add(syncItem);
				}
				else if( folderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder)
				{
					var filesAndFoldersInDirectoryArray = _iStorage.GetAllFilesInDirectory(subPath)
						.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();

					var dirs = _iStorage.GetDirectoryRecursive(subPath);
					filesAndFoldersInDirectoryArray.AddRange(dirs);
					
					foreach ( var fileInDirectory in filesAndFoldersInDirectoryArray )
					{
						var syncItem = new SyncViewModel
						{
							FilePath = fileInDirectory,
							Status = FileIndexItem.ExifStatus.Ok
						};
						syncResultsList.Add(syncItem);
					}
				}
				else // single file
				{
					var syncItem = new SyncViewModel
					{
						FilePath = subPath,
						Status = FileIndexItem.ExifStatus.Ok
					};
					syncResultsList.Add(syncItem);
				}
	        
	            // Update >
				_bgTaskQueue.QueueBackgroundWorkItem(async token =>
				{
					_sync.SyncFiles(subPath,false);
					Console.WriteLine(">>> running clear cache "+ subPath);
					_query.RemoveCacheParentItem(subPath);
				});
	            
			}
			
			return Json(syncResultsList);
        }
			   
	    /// <summary>
	    /// Rename file/folder and update it in the database
	    /// </summary>
	    /// <param name="f">from subPath</param>
	    /// <param name="to">to subPath</param>
	    /// <param name="collections">is collections bool</param>
	    /// <returns>list of details form changed files</returns>
	    /// <response code="200">the item including the updated content</response>
	    /// <response code="404">item not found in the database or on disk</response>
	    /// <response code="401">User unauthorized</response>
	    [ProducesResponseType(typeof(List<FileIndexItem>),200)]
	    [ProducesResponseType(typeof(List<FileIndexItem>),404)]
		[HttpPost("/sync/rename")]
	    [Produces("application/json")]	    
		public IActionResult Rename(string f, string to, bool collections = true)
	    {
		    var rename = new RenameService(_query, _iStorage).Rename(f, to, collections);
		    
		    // When all items are not found
		    if (rename.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
			    return NotFound(rename);
		    
			return Json(rename);
		}

    }
}

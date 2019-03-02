using System.Collections.Generic;
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
    public class SyncController : Controller
    {
        private readonly ISync _sync;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
        private readonly IQuery _query;
        private readonly AppSettings _appSettings;
	    private readonly IStorage _iStorage;

        public SyncController(ISync sync, IBackgroundTaskQueue queue, IQuery query, AppSettings appSettings, IStorage iStorage)
        {
            _sync = sync;
            _bgTaskQueue = queue;
            _query = query;
            _appSettings = appSettings;
	        _iStorage = iStorage;
        }
        
        /// <summary>
        /// Do a file sync in a background process
        /// </summary>
        /// <param name="f">subpaths split by dot comma</param>
        /// <returns></returns>
        [ActionName("Index")]
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

	            var folderStatus = FilesHelper.IsFolderOrFile(_appSettings.DatabasePathToFilePath(subPath));
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
					var filesAndFoldersInDirectoryArray = FilesHelper.GetFilesInDirectory(_appSettings.DatabasePathToFilePath(subPath)).ToList();
					filesAndFoldersInDirectoryArray.AddRange(FilesHelper.GetDirectoryRecursive(_appSettings.DatabasePathToFilePath(subPath)));
					
					foreach ( var fileInDirectory in filesAndFoldersInDirectoryArray )
					{
						var syncItem = new SyncViewModel
						{
							FilePath = _appSettings.FullPathToDatabaseStyle(fileInDirectory),
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
					_query.RemoveCacheParentItem(subPath);
				});
	            
			}
			
			return Json(syncResultsList);
        }
			   
	    /// <summary>
	    /// Work in progress: Rename file/folder and update it in the database
	    /// </summary>
	    /// <param name="f"></param>
	    /// <param name="to"></param>
	    /// <param name="collections"></param>
	    /// <returns>list of details form changed files</returns>
		[HttpPost("/sync/rename")]
		public IActionResult Rename(string f, string to, bool collections = true)
	    {
			return Json(new RenameFs(_query,_sync,_iStorage).Rename(f,to,collections));
		}

    }
}

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

        public SyncController(ISync sync, IBackgroundTaskQueue queue, IQuery query, AppSettings appSettings)
        {
            _sync = sync;
            _bgTaskQueue = queue;
            _query = query;
            _appSettings = appSettings;
        }
        
        /// <summary>
        /// Do a file sync in a background process
        /// </summary>
        /// <param name="f">subpaths split by dot comma</param>
        /// <returns></returns>
        [ActionName("Index")]
        public IActionResult SyncIndex(string f)
        {
            var inputFilePaths = ConfigRead.SplitInputFilePaths(f).ToList();
            // the result list
            var syncResultsList = new List<SyncViewModel>();

            for (int i = 0; i < inputFilePaths.Count; i++)
            {
                var subPath = inputFilePaths[i];
	            subPath = ConfigRead.RemoveLatestSlash(subPath);
	            if ( subPath == string.Empty ) subPath = "/";

	            var folderStatus = Files.IsFolderOrFile(_appSettings.DatabasePathToFilePath(subPath));
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
					var filesAndFoldersInDirectoryArray = Files.GetFilesInDirectory(_appSettings.DatabasePathToFilePath(subPath)).ToList();
					filesAndFoldersInDirectoryArray.AddRange(Files.GetAllFilesDirectory(_appSettings.DatabasePathToFilePath(subPath)));
					
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
			   
	    
		[HttpPost("/sync/rename")]
		public IActionResult Rename(string f, string to, bool collections = true)
		{
			return Json(new RenameFs(_appSettings,_query,_sync).Rename(f,to,collections));
		}

    }
}

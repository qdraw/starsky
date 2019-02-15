using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskycore.Helpers
{
    public class RenameFs
    {
		private readonly AppSettings _appSettings;
		private readonly IQuery _query;
		private readonly ISync _sync;
	    private IStorage _iStorage;

	    public RenameFs(AppSettings appSettings, IQuery query, ISync isync, IStorage iStorage)
		{
			_query = query;
			_appSettings = appSettings;
			_sync = isync;
			_iStorage = iStorage;
		}

		/// <summary>Move or rename files and update the database</summary>
		/// <param name="f">subpath to file or folder</param>
		/// <param name="to">subpath location to move</param>
		/// <param name="collections">true = copy files with the same name</param>
		public List<FileIndexItem> Rename(string f, string to, bool collections = true)
		{
			// -- param name="addDirectoryIfNotExist">true = create an directory if an parent directory is missing</param>

			var inputFileSubPaths = PathHelper.SplitInputFilePaths(f);
			var toFileSubPaths = PathHelper.SplitInputFilePaths(to);
			
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
			
			for (var i = 0; i < inputFileSubPaths.Length; i++)
			{
				inputFileSubPaths[i] = PathHelper.RemoveLatestSlash(inputFileSubPaths[i]);
				inputFileSubPaths[i] = PathHelper.PrefixDbSlash(inputFileSubPaths[i]);

				var detailView = _query.SingleItem(inputFileSubPaths[i], null, collections, false);
				if (detailView == null) inputFileSubPaths[i] = null;
			}
			
			// To check if the file has a unique name (in database)
			for (var i = 0; i < toFileSubPaths.Length; i++)
			{
				toFileSubPaths[i] = PathHelper.RemoveLatestSlash(toFileSubPaths[i]);
				toFileSubPaths[i] = PathHelper.PrefixDbSlash(toFileSubPaths[i]);

				var detailView = _query.SingleItem(toFileSubPaths[i], null, collections, false);
				if (detailView != null) toFileSubPaths[i] = null;
			}
			
			// Remove null from list
			toFileSubPaths = toFileSubPaths.Where(p => p != null).ToArray();
			inputFileSubPaths = inputFileSubPaths.Where(p => p != null).ToArray();
			
			// Check if two list are the same lenght - Change this in the future BadRequest("f != to")
			if (toFileSubPaths.Length != inputFileSubPaths.Length || 
				toFileSubPaths.Length == 0 || inputFileSubPaths.Length == 0) 
			{ 
				// files that not exist
				fileIndexResultsList.Add(new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
				});
				return fileIndexResultsList;
	        }
			
			for (var i = 0; i < toFileSubPaths.Length; i++)
			{
				// options
				// 1. file to direct folder file.jpg /folder/ (not covered)
				// 2. folder to folder
				// 3. folder to existing folder > merge (not convered)
				// 4. file to file
				// 5. file to existing file > skip
				
				var inputFileSubPath = inputFileSubPaths[i];
				var toFileSubPath = toFileSubPaths[i];
				
				var detailView = _query.SingleItem(inputFileSubPath, null, collections, false);
				
				var toFileFullPath = _appSettings.DatabasePathToFilePath(toFileSubPath,false);
				var inputFileFullPath = _appSettings.DatabasePathToFilePath(inputFileSubPath);

				// The To location must be

				var toFileFullPathStatus = _iStorage.IsFolderOrFile(toFileFullPath);
				var inputFileFullPathStatus = _iStorage.IsFolderOrFile(inputFileFullPath);

				// we dont overwrite files
				if ( inputFileFullPathStatus == FolderOrFileModel.FolderOrFileTypeList.File && toFileFullPathStatus != FolderOrFileModel.FolderOrFileTypeList.Deleted)
				{
					fileIndexResultsList.Add(new FileIndexItem
					{
						Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
					});
					continue; //next
				} 

				
				var fileIndexItems = new List<FileIndexItem>();
				if ( inputFileFullPathStatus == FolderOrFileModel.FolderOrFileTypeList.Folder 
				     && toFileFullPathStatus == FolderOrFileModel.FolderOrFileTypeList.Deleted)
				{
					//move
					_iStorage.FolderMove(inputFileSubPath,toFileSubPath);
					
					fileIndexItems = _query.GetAllRecursive(inputFileSubPath);
					// Rename child items
					fileIndexItems.ForEach(p => 
						p.ParentDirectory = p.ParentDirectory.Replace(inputFileSubPath, toFileSubPath)
					);

				}
				else if ( inputFileFullPathStatus == FolderOrFileModel.FolderOrFileTypeList.Folder 
					&& toFileFullPathStatus == FolderOrFileModel.FolderOrFileTypeList.Folder)
				{
					
				}
				else if ( inputFileFullPathStatus == FolderOrFileModel.FolderOrFileTypeList.File) 
				{
					
					var parentSubFolder = Breadcrumbs.BreadcrumbHelper(toFileSubPath).LastOrDefault();

					// clear cache
					_query.RemoveCacheParentItem(parentSubFolder);
					
					if ( !_iStorage.ExistFolder(parentSubFolder) )
					{
						//var syncFiles = _isync.SyncFiles(fileIndexItem.FilePath).ToList();
						
						// todo: add folder feature in the future
						throw new DirectoryNotFoundException($"toFiledirFullPath {parentSubFolder} does not exist");
					}
					
					// Check if the parent folder exist in the database
					_sync.AddSubPathFolder(parentSubFolder);
					
					_iStorage.FileMove(inputFileSubPath,toFileSubPath);
				}
				
				// Rename parent item >eg the folder or file
				detailView.FileIndexItem.SetFilePath(toFileSubPath);
				fileIndexItems.Add(detailView.FileIndexItem);
	

				// To update the results
				_query.UpdateItem(fileIndexItems);

				
				fileIndexResultsList.AddRange(fileIndexItems);

			}

	        return fileIndexResultsList;
        }
    }
}

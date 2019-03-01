using System.Collections.Generic;
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

	    public RenameFs(AppSettings appSettings, IQuery query, ISync iSync, IStorage iStorage)
		{
			_query = query;
			_appSettings = appSettings;
			_sync = iSync;
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
			
			// check for the same input
			if ( inputFileSubPaths.SequenceEqual(toFileSubPaths) )
			{
				return new List<FileIndexItem>{new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.OperationNotSupported
				}};
			}
			
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
			
			for (var i = 0; i < inputFileSubPaths.Length; i++)
			{
				inputFileSubPaths[i] = PathHelper.RemoveLatestSlash(inputFileSubPaths[i]);
				inputFileSubPaths[i] = PathHelper.PrefixDbSlash(inputFileSubPaths[i]);

				var detailView = _query.SingleItem(inputFileSubPaths[i], null, collections, false);
				if (detailView == null) inputFileSubPaths[i] = null;
			}
			
			// To check if the file/or folder has a unique name (in database)
			for (var i = 0; i < toFileSubPaths.Length; i++)
			{
				toFileSubPaths[i] = PathHelper.RemoveLatestSlash(toFileSubPaths[i]);
				toFileSubPaths[i] = PathHelper.PrefixDbSlash(toFileSubPaths[i]);

				var detailView = _query.SingleItem(toFileSubPaths[i], null, collections, false);
				
				// skip for files
				if ( detailView == null) continue;
				// dirs are mergable
				if ( detailView.FileIndexItem.IsDirectory == false ) toFileSubPaths[i] = null;
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
				// 2. folder to folder (not covered)
				// 3. folder with child folders to folder (not covered)
				// 4. folder merge parent folder with current folder (not covered), /test/ => /test/test/
				// 5. folder to existing folder > merge (not covered)
				// 6. file to file
				// 7. file to existing file > skip

				var inputFileSubPath = inputFileSubPaths[i];
				var toFileSubPath = toFileSubPaths[i];
				
				var detailView = _query.SingleItem(inputFileSubPath, null, collections, false);
				
				// The To location must be
				var inputFileFolderStatus = _iStorage.IsFolderOrFile(inputFileSubPath);
				var toFileFolderStatus = _iStorage.IsFolderOrFile(toFileSubPath);

				// we dont overwrite files
				if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.File && toFileFolderStatus != FolderOrFileModel.FolderOrFileTypeList.Deleted)
				{
					fileIndexResultsList.Add(new FileIndexItem
					{
						Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
					});
					continue; //next
				} 

				
				var fileIndexItems = new List<FileIndexItem>();
				if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder 
				     && toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Deleted)
				{
					//move entire folder
					_iStorage.FolderMove(inputFileSubPath,toFileSubPath);
					
					fileIndexItems = _query.GetAllRecursive(inputFileSubPath);
					// Rename child items
					fileIndexItems.ForEach(p => 
						p.ParentDirectory = p.ParentDirectory.Replace(inputFileSubPath, toFileSubPath)
					);

				}
				else if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder 
					&& toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder)
				{
					// 1. Get Direct child files
					// 2. Get Direct folder and child folders
					// 3. move child files
					// 4. remove old folder
					
					// Store Child folders
					var directChildFolders = new List<string>();
					directChildFolders.AddRange(_iStorage.GetDirectoryRecursive(inputFileSubPath));

					// Store direct files
					var directChildItems = new List<string>();
					directChildItems.AddRange(_iStorage.GetAllFilesInDirectory(inputFileSubPath));
					
					// rename child folders
					foreach ( var inputChildFolder in directChildFolders )
					{
						// First FileSys (with folders)
						var outputChildItem = inputChildFolder.Replace(inputFileSubPath, toFileSubPath);
						_iStorage.FolderMove(inputChildFolder,outputChildItem);
					}

					// rename child files
					foreach ( var inputChildItem in directChildItems )
					{
						// First FileSys
						var outputChildItem = inputChildItem.Replace(inputFileSubPath, toFileSubPath);
						_iStorage.FileMove(inputChildItem,outputChildItem);
					}
					
					// Replace all Recursive items in Query
					// Does only replace in existing database items
					fileIndexItems = _query.GetAllRecursive(inputFileSubPath);
					// Rename child items
					fileIndexItems.ForEach(p => 
						p.ParentDirectory = p.ParentDirectory.Replace(inputFileSubPath, toFileSubPath)
					);
					
					// remove parent folder
					//_query.RemoveItem(_query.SingleItem(inputFileSubPath).FileIndexItem);
					
				}
				else if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.File) 
				{
					
					var parentSubFolder = Breadcrumbs.BreadcrumbHelper(toFileSubPath).LastOrDefault();

					// clear cache
					_query.RemoveCacheParentItem(parentSubFolder);
					
					// add folder to file system
					if ( !_iStorage.ExistFolder(parentSubFolder) )
					{
						_iStorage.CreateDirectory(parentSubFolder);
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

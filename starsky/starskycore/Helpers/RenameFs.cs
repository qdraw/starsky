using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.query.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starskycore.Interfaces;

namespace starskycore.Helpers
{
    public class RenameFs
    {
		private readonly IQuery _query;
		private readonly ISync _sync;
		private readonly IStorage _iStorage;

		public RenameFs(IQuery query, ISync iSync, IStorage iStorage)
		{
			_query = query;
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
				var inputFileSubPath = PathHelper.RemoveLatestSlash(inputFileSubPaths[i]);
				inputFileSubPaths[i] = PathHelper.PrefixDbSlash(inputFileSubPath);

				var detailView = _query.SingleItem(inputFileSubPaths[i], null, collections, false);
				if (detailView == null) inputFileSubPaths[i] = null;
			}
			
			// To check if the file/or folder has a unique name (in database)
			for (var i = 0; i < toFileSubPaths.Length; i++)
			{
				var toFileSubPath = PathHelper.RemoveLatestSlash(toFileSubPaths[i]);
				toFileSubPaths[i] = PathHelper.PrefixDbSlash(toFileSubPath);

				var detailView = _query.SingleItem(toFileSubPaths[i], null, collections, false);
				
				// skip for files
				if ( detailView == null) continue;
				// dirs are mergable (isdir=false)
				if (!detailView.FileIndexItem.IsDirectory) toFileSubPaths[i] = null;
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

				var fileIndexItems = new List<FileIndexItem>();
				if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder 
				     && toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Deleted)
				{
					// move entire folder
					_iStorage.FolderMove(inputFileSubPath,toFileSubPath);
					
					fileIndexItems = _query.GetAllRecursive(inputFileSubPath);
					// Rename child items
					fileIndexItems.ForEach(p =>
						{
							p.ParentDirectory =
								p.ParentDirectory.Replace(inputFileSubPath, toFileSubPath);
							p.Status = FileIndexItem.ExifStatus.Ok;
						}
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
						{
							p.ParentDirectory =
								p.ParentDirectory.Replace(inputFileSubPath, toFileSubPath);
							p.Status = FileIndexItem.ExifStatus.Ok;
						}
					);
					
					// todo: remove folder from disk + remove duplicate database item 
					// remove duplicate item from list
					_query.GetObjectByFilePath(inputFileSubPath);

				}
				else if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.File 
				          && toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.File)
				{
					// overwrite a file
					fileIndexResultsList.Add(new FileIndexItem
					{
						Status = FileIndexItem.ExifStatus.OperationNotSupported
					});
				}
				else if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.File
				          && toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Deleted) 
				{
					// toFileSubPath should contain the full subpath
					
					// when trying to rename something wrongs
					var fileName = FilenamesHelper.GetFileName(toFileSubPath);
					if ( !FilenamesHelper.IsValidFileName(fileName) )
					{
						fileIndexResultsList.Add(new FileIndexItem
						{
							Status = FileIndexItem.ExifStatus.OperationNotSupported
						});
						continue; //next
					}
					
					// from/input cache should be cleared
					var inputParentSubFolder = Breadcrumbs.BreadcrumbHelper(inputFileSubPath).LastOrDefault();
					_query.RemoveCacheParentItem(inputParentSubFolder);

					var toParentSubFolder = Breadcrumbs.BreadcrumbHelper(toFileSubPath).LastOrDefault();
					// clear cache (to FileSubPath parents)
					_query.RemoveCacheParentItem(toParentSubFolder);

					// add folder to file system
					if ( !_iStorage.ExistFolder(toParentSubFolder) )
					{
						_iStorage.CreateDirectory(toParentSubFolder);
					}
					
					// Check if the parent folder exist in the database
					_sync.AddSubPathFolder(toParentSubFolder);
					
					_iStorage.FileMove(inputFileSubPath,toFileSubPath);
				}
				else if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.File
				          && toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder )
				{
					// toFileSubPath must be the to copy directory, the filename is kept the same

					// update to support UpdateItem
					toFileSubPath = toFileSubPath + "/" + FilenamesHelper.GetFileName(inputFileSubPath);
					
					// you can't move the file to the same location
					if ( inputFileSubPath == toFileSubPath )
					{
						fileIndexResultsList.Add(new FileIndexItem
						{
							Status = FileIndexItem.ExifStatus.OperationNotSupported
						});
						continue; //next
					}
					
					// from/input cache should be cleared
					var inputParentSubFolder = Breadcrumbs.BreadcrumbHelper(inputFileSubPath).LastOrDefault();
					_query.RemoveCacheParentItem(inputParentSubFolder);
					
					// clear cache // parentSubFolder (to FileSubPath parents)
					var toParentSubFolder = Breadcrumbs.BreadcrumbHelper(toFileSubPath).LastOrDefault();
					_query.RemoveCacheParentItem(toParentSubFolder); 
					
					// Check if the parent folder exist in the database // parentSubFolder
					_sync.AddSubPathFolder(toParentSubFolder);
					
					_iStorage.FileMove(inputFileSubPath, toFileSubPath);
				} 
				
				// Rename parent item >eg the folder or file
				detailView.FileIndexItem.SetFilePath(toFileSubPath);
				detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
				fileIndexItems.Add(detailView.FileIndexItem);
	
				// To update the results
				_query.UpdateItem(fileIndexItems);

				fileIndexResultsList.AddRange(fileIndexItems);
			}

	        return fileIndexResultsList;
        }

    }
}

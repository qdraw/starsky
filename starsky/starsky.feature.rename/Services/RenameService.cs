using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.rename.Services
{
    public class RenameService
    {
		private readonly IQuery _query;
		private readonly IStorage _iStorage;

		public RenameService(IQuery query,  IStorage iStorage)
		{
			_query = query;
			_iStorage = iStorage;
		}

		/// <summary>Move or rename files and update the database</summary>
		/// <param name="f">subPath to file or folder</param>
		/// <param name="to">subPath location to move</param>
		/// <param name="collections">true = copy files with the same name</param>
		public List<FileIndexItem> Rename(string f, string to, bool collections = true)
		{
			// -- param name="addDirectoryIfNotExist">true = create an directory if an parent directory is missing</param>

			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = 
				InputOutputSubPathsPreflight(f, to, collections);

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
							var parentDirectory = p.ParentDirectory
								.Replace(inputFileSubPath, toFileSubPath);
							p.ParentDirectory = parentDirectory;
							p.Status = FileIndexItem.ExifStatus.Ok;
						}
					);

				}
				else if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder 
					&& toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder)
				{
					FromFolderToFolder(inputFileSubPath, toFileSubPath, fileIndexItems);
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
					// toFileSubPath should contain the full subPath
					FromFileToDeleted(inputFileSubPath, toFileSubPath, 
						fileIndexResultsList);
				}
				else if ( inputFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.File
				          && toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder )
				{
					toFileSubPath = GetFileName(toFileSubPath, inputFileSubPath);
					// toFileSubPath must be the to copy directory, the filename is kept the same
					FromFileToFolder(inputFileSubPath, toFileSubPath, fileIndexResultsList);
				} 
				
				// Rename parent item >eg the folder or file
				detailView.FileIndexItem.SetFilePath(toFileSubPath);
				detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
				fileIndexItems.Add(detailView.FileIndexItem);
	
				// To update the file that is changed
				_query.UpdateItem(fileIndexItems);

				fileIndexResultsList.AddRange(fileIndexItems);
			}

	        return fileIndexResultsList;
        }

		private string GetFileName(string toFileSubPath, string inputFileSubPath)
		{
			// Needed to create SetFilePath() for item that is copied, not the folder
			// no double slash when moving to root folder
			return toFileSubPath == "/" ? $"/{FilenamesHelper.GetFileName(inputFileSubPath)}" 
				: $"{toFileSubPath}/{FilenamesHelper.GetFileName(inputFileSubPath)}";
		}
		
		/// <summary>
		/// Checks for inputs that denied the request
		/// </summary>
		/// <param name="f">list of filePaths in string format (dot comma separated)</param>
		/// <param name="to">list of filePaths in string format  (dot comma separated)</param>
		/// <param name="collections">is Collections enabled</param>
		/// <returns>1) Tuple of the input output string - when fails this two array's has no items
		/// 2) the list of fileIndex Items.
		/// This contains only values when something is wrong and the request is denied</returns>
		internal Tuple<Tuple<string[],string[]>,List<FileIndexItem>> InputOutputSubPathsPreflight
			(string f, string to, bool collections)
		{
			var inputFileSubPaths = PathHelper.SplitInputFilePaths(f).ToList();
			var toFileSubPaths = PathHelper.SplitInputFilePaths(to).ToList();

			// check for the same input
			if ( inputFileSubPaths.SequenceEqual(toFileSubPaths) )
			{
				return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
					new Tuple<string[], string[]>(new string[0], new string[0]),
					new List<FileIndexItem>
					{
						new FileIndexItem
						{
							Status = FileIndexItem.ExifStatus.OperationNotSupported
						}
					}
				);
			}
			
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
			
			for (var i = 0; i < inputFileSubPaths.Count; i++)
			{
				var inputFileSubPath = PathHelper.RemoveLatestSlash(inputFileSubPaths[i]);
				inputFileSubPaths[i] = PathHelper.PrefixDbSlash(inputFileSubPath);

				var detailView = _query.SingleItem(inputFileSubPaths[i], null, collections, false);
				if ( detailView == null )
				{
					inputFileSubPaths[i] = null;
				}
			}
			
			// To check if the file/or folder has a unique name (in database)
			for (var i = 0; i < toFileSubPaths.Count; i++)
			{
				var toFileSubPath = PathHelper.RemoveLatestSlash(toFileSubPaths[i]);
				toFileSubPaths[i] = PathHelper.PrefixDbSlash(toFileSubPath);

				// to move
				var detailView = _query.SingleItem(toFileSubPaths[i], null, collections, false);
				
				// skip for files
				if ( detailView == null )
				{
					// do NOT set null because you move to location that currently doesn't exist
					// and avoid mixing up the order of files
					continue;
				}
				// dirs are mergeable, when it isn't a directory
				if ( detailView.FileIndexItem.IsDirectory == false )
				{
					toFileSubPaths[i] = null;
				}
			}
			
			// // Remove null from list
			// toFileSubPaths = toFileSubPaths.Where(p => p != null).ToList();
			// inputFileSubPaths = inputFileSubPaths.Where(p => p != null).ToList();

			// Check if two list are the same Length - Change this in the future BadRequest("f != to")
			if (toFileSubPaths.Count != inputFileSubPaths.Count || 
			    toFileSubPaths.Count == 0 || inputFileSubPaths.Count == 0) 
			{ 
				// files that not exist
				fileIndexResultsList.Add(new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
				});
			}

			// remove both values when ONE OF those two values are null
			for ( var i = 0; i < toFileSubPaths.Count; i++ )
			{
				if ( toFileSubPaths[i] != null && inputFileSubPaths[i] != null ) continue;
				toFileSubPaths.RemoveAt(i);
				inputFileSubPaths.RemoveAt(i);
				fileIndexResultsList.Add(new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
				});
			}

			// when moving a file that does not exist (/non-exist.jpg to /non-exist2.jpg)
			if ( inputFileSubPaths.Count != toFileSubPaths.Count )
			{
				return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
					new Tuple<string[], string[]>(new string[0], new string[0]), 
					fileIndexResultsList
				);
			}

			if ( !collections )
			{
				return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
					new Tuple<string[], string[]>(inputFileSubPaths.ToArray(), toFileSubPaths.ToArray()), 
					fileIndexResultsList
				);
			}
			return CollectionAddPreflight(inputFileSubPaths, toFileSubPaths, fileIndexResultsList);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inputFileSubPaths"></param>
		/// <param name="toFileSubPaths"></param>
		/// <param name="fileIndexResultsList"></param>
		/// <returns></returns>
		private Tuple<Tuple<string[], string[]>, List<FileIndexItem>> CollectionAddPreflight(
			IReadOnlyList<string> inputFileSubPaths, IReadOnlyList<string> toFileSubPaths,
			List<FileIndexItem> fileIndexResultsList)
		{
			var inputCollectionFileSubPaths = new List<string>();
			var toCollectionFileSubPaths = new List<string>();

			for (var i = 0; i < inputFileSubPaths.Count; i++)
			{
				var collectionPaths = _query.SingleItem(inputFileSubPaths[i], 
					null, true, false).FileIndexItem.CollectionPaths;
				inputCollectionFileSubPaths.AddRange(collectionPaths);
				// one file could have move than 1 collections files
				for ( var j = 0; j < collectionPaths.Count; j++ )
				{
					toCollectionFileSubPaths.Add(toFileSubPaths[i]);
				}
			}

			return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
				new Tuple<string[], string[]>(inputCollectionFileSubPaths.ToArray(), toCollectionFileSubPaths.ToArray()), 
				fileIndexResultsList
			);
		}


		private void MoveSidecarFile(string inputFileSubPath, string toFileSubPath)
		{
			var inputFileSubPathJsonSidecarFile =
				JsonSidecarLocation.JsonLocation(inputFileSubPath);
			var toSidecarFile = JsonSidecarLocation.JsonLocation(toFileSubPath);
			if ( _iStorage.ExistFile(inputFileSubPathJsonSidecarFile) )
			{
				_iStorage.FileMove(inputFileSubPathJsonSidecarFile,toSidecarFile);
			}
		}

		private void FromFolderToFolder(string inputFileSubPath, string toFileSubPath,
			List<FileIndexItem> fileIndexItems)
		{
			if ( fileIndexItems == null ) throw new ArgumentNullException(nameof(fileIndexItems));
			
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

		private void FromFileToDeleted(string inputFileSubPath, string toFileSubPath,
			List<FileIndexItem> fileIndexResultsList)
		{
			
			// when trying to rename something wrongs
			var fileName = FilenamesHelper.GetFileName(toFileSubPath);
			if ( !FilenamesHelper.IsValidFileName(fileName) )
			{
				fileIndexResultsList.Add(new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.OperationNotSupported
				});
				return; //next
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
			_query.AddParentItemsAsync(toParentSubFolder).ConfigureAwait(false);
					
			_iStorage.FileMove(inputFileSubPath,toFileSubPath);
			MoveSidecarFile(inputFileSubPath, toFileSubPath);
		}

		private void FromFileToFolder(string inputFileSubPath, string toFileSubPath, List<FileIndexItem> fileIndexResultsList)
		{
			// you can't move the file to the same location
			if ( inputFileSubPath == toFileSubPath )
			{
				fileIndexResultsList.Add(new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.OperationNotSupported
				});
				return; //next
			}
					
			// from/input cache should be cleared
			var inputParentSubFolder = Breadcrumbs.BreadcrumbHelper(inputFileSubPath).LastOrDefault();
			_query.RemoveCacheParentItem(inputParentSubFolder);
					
			// clear cache // parentSubFolder (to FileSubPath parents)
			var toParentSubFolder = Breadcrumbs.BreadcrumbHelper(toFileSubPath).LastOrDefault();
			_query.RemoveCacheParentItem(toParentSubFolder); 
					
			// Check if the parent folder exist in the database // parentSubFolder
			_query.AddParentItemsAsync(toParentSubFolder).ConfigureAwait(false);
					
			_iStorage.FileMove(inputFileSubPath, toFileSubPath);
			MoveSidecarFile(inputFileSubPath, toFileSubPath);
		}

    }
}

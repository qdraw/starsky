using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

		/// <summary>Move or rename files and update the database.
		/// The services also returns the source folders/files as NotFoundSourceMissing</summary>
		/// <param name="f">subPath to file or folder</param>
		/// <param name="to">subPath location to move</param>
		/// <param name="collections">true = copy files with the same name</param>
		public async Task<List<FileIndexItem>> Rename(string f, string to, bool collections = true)
		{
			// -- param name="addDirectoryIfNotExist">true = create an directory if an parent directory is missing</param>

			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = 
				InputOutputSubPathsPreflight(f, to, collections);

			for (var i = 0; i < toFileSubPaths.Length; i++)
			{
				// options
				// 1. FromFolderToDeleted:
				//		folder rename
				// 2. FromFolderToFolder:
				//		folder with child folders to folder
				// 3. Not named
				//		file to file
				//		- overwrite a file is not supported
				// 4. FromFileToDeleted:
				//		rename a file to new location
				// 5. FromFileToFolder:
				//		file to direct folder file.jpg  -> /folder/ 
				// 6. folder merge parent folder with current folder (not covered), /test/ => /test/test/
				
				var inputFileSubPath = inputFileSubPaths[i];
				var toFileSubPath = toFileSubPaths[i];
				var detailView = _query.SingleItem(inputFileSubPath, null, collections, false);

				// The To location must be
				var inputFileFolderStatus = _iStorage.IsFolderOrFile(inputFileSubPath);
				var toFileFolderStatus = _iStorage.IsFolderOrFile(toFileSubPath);

				var fileIndexItems = new List<FileIndexItem>();
				switch ( inputFileFolderStatus )
				{
					case FolderOrFileModel.FolderOrFileTypeList.Folder when toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Deleted:
						await FromFolderToDeleted(inputFileSubPath, toFileSubPath, fileIndexResultsList, detailView);
						break;
					case FolderOrFileModel.FolderOrFileTypeList.Folder when toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder:
						await FromFolderToFolder(inputFileSubPath, toFileSubPath, fileIndexResultsList, detailView);
						break;
					case FolderOrFileModel.FolderOrFileTypeList.File when toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.File:
						// overwrite a file is not supported
						fileIndexResultsList.Add(new FileIndexItem
						{
							Status = FileIndexItem.ExifStatus.OperationNotSupported
						});
						break;
					case FolderOrFileModel.FolderOrFileTypeList.File when toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Deleted:
						// toFileSubPath should contain the full subPath
						await FromFileToDeleted(inputFileSubPath, toFileSubPath, 
							fileIndexResultsList, fileIndexItems, detailView);
						break;
					case FolderOrFileModel.FolderOrFileTypeList.File when toFileFolderStatus == FolderOrFileModel.FolderOrFileTypeList.Folder:
						toFileSubPath = GetFileName(toFileSubPath, inputFileSubPath);
						// toFileSubPath must be the to copy directory, the filename is kept the same
						await FromFileToFolder(inputFileSubPath, toFileSubPath, fileIndexResultsList, fileIndexItems, detailView);
						break;
				} 
			}
	        return fileIndexResultsList;
        }

		private async Task SaveToDatabaseAsync(List<FileIndexItem> fileIndexItems, 
			List<FileIndexItem> fileIndexResultsList, DetailView detailView, string toFileSubPath)
		{
			// Rename parent item >eg the folder or file
			detailView.FileIndexItem.SetFilePath(toFileSubPath);
			detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
			
			fileIndexItems.Add(detailView.FileIndexItem);
	
			// To update the file that is changed
			await _query.UpdateItemAsync(fileIndexItems);

			fileIndexResultsList.AddRange(fileIndexItems);
		}

		private async Task FromFolderToDeleted(string inputFileSubPath,
			string toFileSubPath, List<FileIndexItem> fileIndexResultsList,
			DetailView detailView)
		{
			// clean from cache
			_query.RemoveCacheParentItem(inputFileSubPath);
			
			var fileIndexItems = await _query.GetAllRecursiveAsync(inputFileSubPath);
			// Rename child items
			fileIndexItems.ForEach(p =>
				{
					var parentDirectory = p.ParentDirectory
						.Replace(inputFileSubPath, toFileSubPath);
					p.ParentDirectory = parentDirectory;
					p.Status = FileIndexItem.ExifStatus.Ok;
					p.Tags = p.Tags.Replace("!delete!", string.Empty);
				}
			);

			// when there is already a database item in the output folder, but not on disk
			// in the final step we going to update the database item to the new name
			var toCheckList = fileIndexItems.Select(p => p.FilePath).ToList();
			toCheckList.Add(toFileSubPath);
			var checkOutput =  await _query.GetObjectsByFilePathQueryAsync(toCheckList);
			foreach ( var item in checkOutput )
			{
				await _query.RemoveItemAsync(item);
			}
			
			// save before changing on disk
			await SaveToDatabaseAsync(fileIndexItems, fileIndexResultsList,
				detailView, toFileSubPath);
			
			// move entire folder
			_iStorage.FolderMove(inputFileSubPath,toFileSubPath);
			
			fileIndexResultsList.Add(new FileIndexItem(inputFileSubPath){Status = FileIndexItem.ExifStatus.NotFoundSourceMissing});
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
		/// <returns>Tuple that contains two items:
		/// item1) Tuple of the input output string - when fails this two array's has no items
		/// item2) the list of fileIndex Items.
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
				inputFileSubPaths[i] = PathHelper.PrefixDbSlash(PathHelper.RemovePrefixDbSlash(inputFileSubPath));

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
			
			// Check if two list are the same Length - Change this in the future BadRequest("f != to")
			// when moving a file that does not exist (/non-exist.jpg to /non-exist2.jpg)
			if (toFileSubPaths.Count != inputFileSubPaths.Count || 
			    toFileSubPaths.Count == 0 || inputFileSubPaths.Count == 0) 
			{ 
				// files that not exist
				fileIndexResultsList.Add(new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
				});
				return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
					new Tuple<string[], string[]>(new string[0], new string[0]), 
					fileIndexResultsList
				);
			}
			return CollectionAddPreflight(inputFileSubPaths, toFileSubPaths, fileIndexResultsList, collections);
		}

		/// <summary>
		/// Get the collections items when preflighting
		/// Returns as Tuple
		/// item1: inputFileSubPaths, toFileSubPaths
		/// item2: list of fileIndex Results (which contains only error cases)
		/// </summary>
		/// <param name="inputFileSubPaths">from where to copy (file or folder)</param>
		/// <param name="toFileSubPaths">copy to (file or folder)</param>
		/// <param name="fileIndexResultsList">results list</param>
		/// <param name="collections">enable file collections</param>
		/// <returns>inputFileSubPaths list, toFileSubPaths list and fileIndexResultsList</returns>
		private Tuple<Tuple<string[], string[]>, List<FileIndexItem>> CollectionAddPreflight(
			IReadOnlyList<string> inputFileSubPaths, IReadOnlyList<string> toFileSubPaths,
			List<FileIndexItem> fileIndexResultsList, bool collections)
		{
			if ( !collections )
			{
				return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
					new Tuple<string[], string[]>(inputFileSubPaths.ToArray(), toFileSubPaths.ToArray()), 
					fileIndexResultsList
				);
			}
			
			var inputCollectionFileSubPaths = new List<string>();
			var toCollectionFileSubPaths = new List<string>();

			for (var i = 0; i < inputFileSubPaths.Count; i++)
			{
				// When the input is a folder, just copy the array
				if ( _iStorage.ExistFolder(inputFileSubPaths[i]) )
				{
					inputCollectionFileSubPaths.Add(inputFileSubPaths[i]);
					toCollectionFileSubPaths.Add(toFileSubPaths[i]);
					continue;
				}
				
				// when it is a file update the 'to paths'
				var collectionPaths = _query.SingleItem(inputFileSubPaths[i], 
					null, true, false).FileIndexItem.CollectionPaths;
				inputCollectionFileSubPaths.AddRange(collectionPaths);

				for ( var j = 0; j < collectionPaths.Count; j++ )
				{
					var collectionItem = collectionPaths[j];
					// When moving to a folder
					if ( _iStorage.ExistFolder(toFileSubPaths[i]) )
					{
						toCollectionFileSubPaths.Add(toFileSubPaths[i]);
						continue;
					}
					
					var extensionWithoutDot = FilenamesHelper.GetFileExtensionWithoutDot(collectionItem);
					// when rename-ing the current file, but the other ones are implicit copied
					if ( j == 0 ) extensionWithoutDot = FilenamesHelper.GetFileExtensionWithoutDot(toFileSubPaths[i]);
					
					// Rename other sidecar files
					// From file to Deleted
					var parentFolder = FilenamesHelper.GetParentPath(toFileSubPaths[i]);
					var baseName = FilenamesHelper.GetFileNameWithoutExtension(toFileSubPaths[i]);
					toCollectionFileSubPaths.Add($"{parentFolder}/{baseName}.{extensionWithoutDot}");
				}
			}

			return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
				new Tuple<string[], string[]>(inputCollectionFileSubPaths.ToArray(), toCollectionFileSubPaths.ToArray()), 
				fileIndexResultsList
			);
		}

		/// <summary>
		/// Move sidecar files when those exist
		/// </summary>
		/// <param name="inputFileSubPath">from path</param>
		/// <param name="toFileSubPath">to path</param>
		private void MoveSidecarFile(string inputFileSubPath, string toFileSubPath)
		{
			// json sidecar move
			var jsonInputFileSubPathSidecarFile = JsonSidecarLocation
				.JsonLocation(inputFileSubPath);
			var jsonSidecarFile = JsonSidecarLocation.JsonLocation(toFileSubPath);
			
			if ( _iStorage.ExistFile(jsonInputFileSubPathSidecarFile) )
			{
				_iStorage.FileMove(jsonInputFileSubPathSidecarFile,jsonSidecarFile);
			}

			// xmp sidecar file move
			if ( !ExtensionRolesHelper.IsExtensionForceXmp(inputFileSubPath) )
			{
				return;
			}
			var xmpInputFileSubPathSidecarFile =  ExtensionRolesHelper
				.ReplaceExtensionWithXmp(inputFileSubPath);
			var xmpSidecarFile = ExtensionRolesHelper
				.ReplaceExtensionWithXmp(toFileSubPath);
			if ( _iStorage.ExistFile(xmpInputFileSubPathSidecarFile) )
			{
				_iStorage.FileMove(xmpInputFileSubPathSidecarFile, xmpSidecarFile);
			}
		}

		
		internal Task FromFolderToFolder(string inputFileSubPath, 
			string toFileSubPath, List<FileIndexItem> fileIndexResultsList, DetailView detailView)
		{
			if ( fileIndexResultsList == null )
			{
				throw new ArgumentNullException(nameof(fileIndexResultsList), "Should contain value");
			}
			return FromFolderToFolderAsync(inputFileSubPath, toFileSubPath, fileIndexResultsList,detailView);
		}
		
		/// <summary>
		/// Copy from a folder to a folder
		/// </summary>
		/// <param name="inputFileSubPath">from path</param>
		/// <param name="toFileSubPath">to path</param>
		/// <param name="fileIndexResultsList"></param>
		/// <param name="detailView"></param>
		/// <exception cref="ArgumentNullException">fileIndexItems is null</exception>
		private async Task FromFolderToFolderAsync(string inputFileSubPath, 
			string toFileSubPath, List<FileIndexItem> fileIndexResultsList, DetailView detailView)
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
			
			// Replace all Recursive items in Query
			// Does only replace in existing database items
			var fileIndexItems = await _query.GetAllRecursiveAsync(inputFileSubPath);
					
			// Rename child items
			fileIndexItems.ForEach(p =>
				{
					p.ParentDirectory =
						p.ParentDirectory.Replace(inputFileSubPath, toFileSubPath);
					p.Status = FileIndexItem.ExifStatus.Ok;
				}
			);
			
			// save before changing on disk
			await SaveToDatabaseAsync(fileIndexItems, fileIndexResultsList,
				detailView, toFileSubPath);
			
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
			
			// when renaming a folder it should warn the UI that it should remove the source item
			fileIndexResultsList.Add(new FileIndexItem(inputFileSubPath){Status = FileIndexItem.ExifStatus.NotFoundSourceMissing});
		}

		private async Task FromFileToDeleted(string inputFileSubPath, string toFileSubPath,
			List<FileIndexItem> fileIndexResultsList, List<FileIndexItem> fileIndexItems, DetailView detailView)
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
			var inputParentSubFolder = FilenamesHelper.GetParentPath(inputFileSubPath);
			_query.RemoveCacheParentItem(inputParentSubFolder);

			var toParentSubFolder = FilenamesHelper.GetParentPath(toFileSubPath);
			if ( string.IsNullOrEmpty(toParentSubFolder) ) toParentSubFolder = "/";
			
			// clear cache (to FileSubPath parents)
			_query.RemoveCacheParentItem(toParentSubFolder);
			
			// Check if the parent folder exist in the database
			await _query.AddParentItemsAsync(toParentSubFolder);

			// Save in database before change on disk
			await SaveToDatabaseAsync(fileIndexItems, fileIndexResultsList,
				detailView, toFileSubPath);
					
			// add folder to file system
			if ( !_iStorage.ExistFolder(toParentSubFolder) )
			{
				_iStorage.CreateDirectory(toParentSubFolder);
				fileIndexResultsList.Add(new FileIndexItem(toParentSubFolder){Status = FileIndexItem.ExifStatus.Ok});
			}
			
			_iStorage.FileMove(inputFileSubPath,toFileSubPath);
			MoveSidecarFile(inputFileSubPath, toFileSubPath);
			
			// when renaming a folder it should warn the UI that it should remove the source item
			fileIndexResultsList.Add(new FileIndexItem(inputFileSubPath){Status = FileIndexItem.ExifStatus.NotFoundSourceMissing});
		}

		private async Task FromFileToFolder(string inputFileSubPath, string toFileSubPath, 
			List<FileIndexItem> fileIndexResultsList, List<FileIndexItem> fileIndexItems, DetailView detailView)
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
			// when renaming a folder it should warn the UI that it should remove the source item
			fileIndexResultsList.Add(new FileIndexItem(inputFileSubPath){Status = FileIndexItem.ExifStatus.NotFoundSourceMissing});
			
			// from/input cache should be cleared
			var inputParentSubFolder = Breadcrumbs.BreadcrumbHelper(inputFileSubPath).LastOrDefault();
			_query.RemoveCacheParentItem(inputParentSubFolder);

			// clear cache // parentSubFolder (to FileSubPath parents)
			var toParentSubFolder = Breadcrumbs.BreadcrumbHelper(toFileSubPath).LastOrDefault();
			_query.RemoveCacheParentItem(toParentSubFolder); 
					
			// Check if the parent folder exist in the database // parentSubFolder
			await _query.AddParentItemsAsync(toParentSubFolder);

			await SaveToDatabaseAsync(fileIndexItems, fileIndexResultsList,
				detailView, toFileSubPath);
			
			// First update database and then update for diskwatcher
			_iStorage.FileMove(inputFileSubPath, toFileSubPath);
			MoveSidecarFile(inputFileSubPath, toFileSubPath);
		}

    }
}

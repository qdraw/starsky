using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.feature.rename.Models;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.rename.Services;

public class RenameService(IQuery query, IStorage iStorage)
{
	/// <summary>
	///     Move or rename files and update the database.
	///     The services also returns the source folders/files as NotFoundSourceMissing
	/// </summary>
	/// <param name="f">subPath to file or folder</param>
	/// <param name="to">subPath location to move</param>
	/// <param name="collections">true = copy files with the same name</param>
	public async Task<List<FileIndexItem>> Rename(string f, string to, bool collections = true)
	{
		// -- param name="addDirectoryIfNotExist">true = create a directory if a parent directory is missing</param>

		var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) =
			InputOutputSubPathsPreflight(f, to, collections);

		for ( var i = 0; i < toFileSubPaths.Length; i++ )
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
			var detailView = query.SingleItem(inputFileSubPath, null, collections, false);

			// The To location must be
			var inputFileFolderStatus = iStorage.IsFolderOrFile(inputFileSubPath);
			var toFileFolderStatus = iStorage.IsFolderOrFile(toFileSubPath);

			var fileIndexItems = new List<FileIndexItem>();
			switch ( inputFileFolderStatus )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Folder when toFileFolderStatus ==
					FolderOrFileModel.FolderOrFileTypeList.Deleted:
					await FromFolderToDeleted(inputFileSubPath, toFileSubPath,
						fileIndexResultsList, detailView);
					break;
				case FolderOrFileModel.FolderOrFileTypeList.Folder when toFileFolderStatus ==
					FolderOrFileModel.FolderOrFileTypeList.Folder:
					await FromFolderToFolder(inputFileSubPath, toFileSubPath,
						fileIndexResultsList, detailView!);
					break;
				case FolderOrFileModel.FolderOrFileTypeList.File when toFileFolderStatus ==
					FolderOrFileModel.FolderOrFileTypeList.File:
					// overwrite a file is not supported
					fileIndexResultsList.Add(new FileIndexItem
					{
						Status = FileIndexItem.ExifStatus.OperationNotSupported
					});
					break;
				case FolderOrFileModel.FolderOrFileTypeList.File when toFileFolderStatus ==
					FolderOrFileModel.FolderOrFileTypeList.Deleted:
					// toFileSubPath should contain the full subPath
					await RenameFromFileToDeleted(inputFileSubPath, toFileSubPath,
						fileIndexResultsList, fileIndexItems, detailView!);
					break;
				case FolderOrFileModel.FolderOrFileTypeList.File when toFileFolderStatus ==
					FolderOrFileModel.FolderOrFileTypeList.Folder:
					toFileSubPath = GetFileName(toFileSubPath, inputFileSubPath);
					// toFileSubPath must be the to copy directory, the filename is kept the same
					await FromFileToFolder(inputFileSubPath, toFileSubPath,
						fileIndexResultsList, fileIndexItems, detailView!);
					break;
			}

			// Reset Cache for the item that is renamed
			query.ResetItemByHash(detailView!.FileIndexItem!.FileHash!);
		}

		return fileIndexResultsList;
	}

	private async Task SaveToDatabaseAsync(List<FileIndexItem> fileIndexItems,
		List<FileIndexItem> fileIndexResultsList, DetailView detailView, string toFileSubPath)
	{
		// Rename parent item >eg the folder or file
		detailView.FileIndexItem!.SetFilePath(toFileSubPath);
		detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;

		fileIndexItems.Add(detailView.FileIndexItem);

		// To update the file that is changed
		await query.UpdateItemAsync(fileIndexItems);

		fileIndexResultsList.AddRange(fileIndexItems);
	}

	private async Task FromFolderToDeleted(string inputFileSubPath,
		string toFileSubPath, List<FileIndexItem> fileIndexResultsList,
		DetailView? detailView)
	{
		// clean from cache
		query.RemoveCacheParentItem(inputFileSubPath);

		var fileIndexItems = await query.GetAllRecursiveAsync(inputFileSubPath);
		// Rename child items
		fileIndexItems.ForEach(p =>
			{
				var parentDirectory = p.ParentDirectory!
					.Replace(inputFileSubPath, toFileSubPath);
				p.ParentDirectory = parentDirectory;
				p.Status = FileIndexItem.ExifStatus.Ok;
				p.Tags = p.Tags!.Replace(TrashKeyword.TrashKeywordString, string.Empty);
			}
		);

		// when there is already a database item in the output folder, but not on disk
		// in the final step we're going to update the database item to the new name
		var toCheckList = fileIndexItems.Select(p => p.FilePath).Cast<string>().ToList();
		toCheckList.Add(toFileSubPath);
		var checkOutput = await query.GetObjectsByFilePathQueryAsync(toCheckList);
		foreach ( var item in checkOutput )
		{
			await query.RemoveItemAsync(item);
		}

		// save before changing on disk
		await SaveToDatabaseAsync(fileIndexItems, fileIndexResultsList,
			detailView!, toFileSubPath);

		// move entire folder
		iStorage.FolderMove(inputFileSubPath, toFileSubPath);

		fileIndexResultsList.Add(new FileIndexItem(inputFileSubPath)
		{
			Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
		});
	}

	private static string GetFileName(string toFileSubPath, string inputFileSubPath)
	{
		// Needed to create SetFilePath() for item that is copied, not the folder
		// no double slash when moving to root folder
		return toFileSubPath == "/"
			? $"/{FilenamesHelper.GetFileName(inputFileSubPath)}"
			: $"{toFileSubPath}/{FilenamesHelper.GetFileName(inputFileSubPath)}";
	}

	/// <summary>
	///     Checks for inputs that denied the request
	/// </summary>
	/// <param name="f">list of filePaths in string format (dot comma separated)</param>
	/// <param name="to">list of filePaths in string format  (dot comma separated)</param>
	/// <param name="collections">is Collections enabled</param>
	/// <returns>
	///     Tuple that contains two items:
	///     item1) Tuple of the input output string - when fails this two array's has no items
	///     item2) the list of fileIndex Items.
	///     This contains only values when something is wrong and the request is denied
	/// </returns>
	internal Tuple<Tuple<string[], string[]>, List<FileIndexItem>> InputOutputSubPathsPreflight
		(string f, string to, bool collections)
	{
		var inputFileSubPaths = PathHelper.SplitInputFilePaths(f).Cast<string?>().ToList();
		var toFileSubPaths = PathHelper.SplitInputFilePaths(to).Cast<string?>().ToList();

		// check for the same input
		if ( inputFileSubPaths.SequenceEqual(toFileSubPaths) )
		{
			return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
				new Tuple<string[], string[]>([], []),
				[new FileIndexItem { Status = FileIndexItem.ExifStatus.OperationNotSupported }]
			);
		}

		// the result list
		var fileIndexResultsList = new List<FileIndexItem>();

		for ( var i = 0; i < inputFileSubPaths.Count; i++ )
		{
			var inputFileSubPath = PathHelper.RemoveLatestSlash(inputFileSubPaths[i]!);
			inputFileSubPaths[i] =
				PathHelper.PrefixDbSlash(PathHelper.RemovePrefixDbSlash(inputFileSubPath));

			var detailView = query.SingleItem(inputFileSubPaths[i]!, null, collections, false);
			if ( detailView == null )
			{
				inputFileSubPaths[i] = null;
			}
		}

		// To check if the file/or folder has a unique name (in database)
		for ( var i = 0; i < toFileSubPaths.Count; i++ )
		{
			var toFileSubPath = PathHelper.RemoveLatestSlash(toFileSubPaths[i]!);
			toFileSubPaths[i] = PathHelper.PrefixDbSlash(toFileSubPath);

			// to move
			var detailView = query.SingleItem(toFileSubPaths[i]!, null, collections, false);

			// skip for files
			if ( detailView?.FileIndexItem == null )
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
			if ( toFileSubPaths[i] != null && inputFileSubPaths[i] != null )
			{
				continue;
			}

			toFileSubPaths.RemoveAt(i);
			inputFileSubPaths.RemoveAt(i);
			fileIndexResultsList.Add(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
			});
		}

		// Check if two list are the same Length - Change this in the future BadRequest("f != to")
		// when moving a file that does not exist (/non-exist.jpg to /non-exist2.jpg)
		if ( toFileSubPaths.Count != inputFileSubPaths.Count ||
		     toFileSubPaths.Count == 0 || inputFileSubPaths.Count == 0 )
		{
			// files that not exist
			fileIndexResultsList.Add(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
			});
			return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
				new Tuple<string[], string[]>(Array.Empty<string>(), Array.Empty<string>()),
				fileIndexResultsList
			);
		}

		return CollectionAddPreflight(inputFileSubPaths!, toFileSubPaths!, fileIndexResultsList,
			collections);
	}

	/// <summary>
	///     Get the collections items when preflighting
	///     Returns as Tuple
	///     item1: inputFileSubPaths, toFileSubPaths
	///     item2: list of fileIndex Results (which contains only error cases)
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
				new Tuple<string[], string[]>(inputFileSubPaths.ToArray(),
					toFileSubPaths.ToArray()),
				fileIndexResultsList
			);
		}

		var inputCollectionFileSubPaths = new List<string>();
		var toCollectionFileSubPaths = new List<string>();

		for ( var i = 0; i < inputFileSubPaths.Count; i++ )
		{
			// When the input is a folder, just copy the array
			if ( iStorage.ExistFolder(inputFileSubPaths[i]) )
			{
				inputCollectionFileSubPaths.Add(inputFileSubPaths[i]);
				toCollectionFileSubPaths.Add(toFileSubPaths[i]);
				continue;
			}

			// when it is a file update the 'to paths'
			var querySingleItemCollections = query.SingleItem(inputFileSubPaths[i],
				null, true, false);
			var collectionPaths = querySingleItemCollections!.FileIndexItem!.CollectionPaths;

			inputCollectionFileSubPaths.AddRange(collectionPaths);

			for ( var j = 0; j < collectionPaths.Count; j++ )
			{
				var collectionItem = collectionPaths[j];
				// When moving to a folder
				if ( iStorage.ExistFolder(toFileSubPaths[i]) )
				{
					toCollectionFileSubPaths.Add(toFileSubPaths[i]);
					continue;
				}

				var extensionWithoutDot =
					FilenamesHelper.GetFileExtensionWithoutDot(collectionItem);
				// when rename-ing the current file, but the other ones are implicit copied
				if ( j == 0 )
				{
					extensionWithoutDot =
						FilenamesHelper.GetFileExtensionWithoutDot(toFileSubPaths[i]);
				}

				// Rename other sidecar files
				// From file to Deleted
				var parentFolder =
					PathHelper.AddSlash(FilenamesHelper.GetParentPath(toFileSubPaths[i]));
				var baseName = FilenamesHelper.GetFileNameWithoutExtension(toFileSubPaths[i]);
				toCollectionFileSubPaths.Add($"{parentFolder}{baseName}.{extensionWithoutDot}");
			}
		}

		return new Tuple<Tuple<string[], string[]>, List<FileIndexItem>>(
			new Tuple<string[], string[]>(inputCollectionFileSubPaths.ToArray(),
				toCollectionFileSubPaths.ToArray()),
			fileIndexResultsList
		);
	}

	/// <summary>
	///     Move sidecar files when those exist
	/// </summary>
	/// <param name="inputFileSubPath">from path</param>
	/// <param name="toFileSubPath">to path</param>
	private void MoveSidecarFile(string inputFileSubPath, string toFileSubPath)
	{
		// json sidecar move
		var jsonInputFileSubPathSidecarFile = JsonSidecarLocation
			.JsonLocation(inputFileSubPath);
		var jsonSidecarFile = JsonSidecarLocation.JsonLocation(toFileSubPath);

		if ( iStorage.ExistFile(jsonInputFileSubPathSidecarFile) )
		{
			iStorage.FileMove(jsonInputFileSubPathSidecarFile, jsonSidecarFile);
		}

		// xmp sidecar file move
		if ( !ExtensionRolesHelper.IsExtensionForceXmp(inputFileSubPath) )
		{
			return;
		}

		var xmpInputFileSubPathSidecarFile = ExtensionRolesHelper
			.ReplaceExtensionWithXmp(inputFileSubPath);
		var xmpSidecarFile = ExtensionRolesHelper
			.ReplaceExtensionWithXmp(toFileSubPath);
		if ( iStorage.ExistFile(xmpInputFileSubPathSidecarFile) )
		{
			iStorage.FileMove(xmpInputFileSubPathSidecarFile, xmpSidecarFile);
		}
	}


	internal Task FromFolderToFolder(string inputFileSubPath,
		string toFileSubPath, List<FileIndexItem> fileIndexResultsList, DetailView detailView)
	{
		if ( fileIndexResultsList == null )
		{
			throw new ArgumentNullException(nameof(fileIndexResultsList),
				"Should contain value");
		}

		return FromFolderToFolderAsync(inputFileSubPath, toFileSubPath, fileIndexResultsList,
			detailView);
	}

	/// <summary>
	///     Copy from a folder to a folder
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
		directChildFolders.AddRange(iStorage.GetDirectoryRecursive(inputFileSubPath)
			.Select(p => p.Key));

		// Store direct files
		var directChildItems = new List<string>();
		directChildItems.AddRange(iStorage.GetAllFilesInDirectory(inputFileSubPath));

		// Replace all Recursive items in Query
		// Does only replace in existing database items
		var fileIndexItems = await query.GetAllRecursiveAsync(inputFileSubPath);

		// Rename child items
		fileIndexItems.ForEach(p =>
			{
				p.ParentDirectory = p.ParentDirectory!
					.Replace(inputFileSubPath, toFileSubPath);
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
			var outputChildItem = inputChildFolder
				.Replace(inputFileSubPath, toFileSubPath);
			iStorage.FolderMove(inputChildFolder, outputChildItem);
		}

		// rename child files
		foreach ( var inputChildItem in directChildItems )
		{
			// First FileSys
			var outputChildItem = inputChildItem.Replace(inputFileSubPath, toFileSubPath);
			iStorage.FileMove(inputChildItem, outputChildItem);
		}

		// when renaming a folder it should warn the UI that it should remove the source item
		fileIndexResultsList.Add(new FileIndexItem(inputFileSubPath)
		{
			Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
		});
	}

	private async Task RenameFromFileToDeleted(string inputFileSubPath, string toFileSubPath,
		List<FileIndexItem> fileIndexResultsList, List<FileIndexItem> fileIndexItems,
		DetailView detailView)
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
		query.RemoveCacheParentItem(inputParentSubFolder);

		var toParentSubFolder = FilenamesHelper.GetParentPath(toFileSubPath);
		if ( string.IsNullOrEmpty(toParentSubFolder) )
		{
			toParentSubFolder = "/";
		}

		// clear cache (to FileSubPath parents)
		query.RemoveCacheParentItem(toParentSubFolder);

		// Check if the parent folder exist in the database
		await query.AddParentItemsAsync(toParentSubFolder);

		// Save in database before change on disk
		await SaveToDatabaseAsync(fileIndexItems, fileIndexResultsList,
			detailView, toFileSubPath);

		// add folder to file system
		if ( !iStorage.ExistFolder(toParentSubFolder) )
		{
			iStorage.CreateDirectory(toParentSubFolder);
			fileIndexResultsList.Add(
				new FileIndexItem(toParentSubFolder) { Status = FileIndexItem.ExifStatus.Ok });
		}

		iStorage.FileMove(inputFileSubPath, toFileSubPath);
		MoveSidecarFile(inputFileSubPath, toFileSubPath);

		// when renaming a folder it should warn the UI that it should remove the source item
		fileIndexResultsList.Add(new FileIndexItem(inputFileSubPath)
		{
			Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
		});
	}

	private async Task FromFileToFolder(string inputFileSubPath, string toFileSubPath,
		List<FileIndexItem> fileIndexResultsList, List<FileIndexItem> fileIndexItems,
		DetailView detailView)
	{
		// you can't move the file to the same location
		// or if it already exists
		if ( inputFileSubPath == toFileSubPath || iStorage.ExistFile(toFileSubPath) )
		{
			fileIndexResultsList.Add(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.OperationNotSupported
			});
			return; //next
		}

		// when renaming a folder it should warn the UI that it should remove the source item
		fileIndexResultsList.Add(new FileIndexItem(inputFileSubPath)
		{
			Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
		});

		// from/input cache should be cleared
		var inputParentSubFolder =
			Breadcrumbs.BreadcrumbHelper(inputFileSubPath).LastOrDefault();
		query.RemoveCacheParentItem(inputParentSubFolder!);

		// clear cache // parentSubFolder (to FileSubPath parents)
		var toParentSubFolder = Breadcrumbs.BreadcrumbHelper(toFileSubPath).LastOrDefault();
		query.RemoveCacheParentItem(toParentSubFolder!);

		// Check if the parent folder exist in the database // parentSubFolder
		await query.AddParentItemsAsync(toParentSubFolder!);

		await SaveToDatabaseAsync(fileIndexItems, fileIndexResultsList,
			detailView, toFileSubPath);

		// First update database and then update for disk watcher
		iStorage.FileMove(inputFileSubPath, toFileSubPath);
		MoveSidecarFile(inputFileSubPath, toFileSubPath);
	}

	/// <summary>
	///     Preview batch rename operation without modifying files
	/// </summary>
	/// <param name="filePaths">List of source file paths to rename</param>
	/// <param name="tokenPattern">
	///     Rename pattern with tokens
	///     (e.g., {yyyy}{MM}{dd}_{filenamebase}{seqn}.ext)
	/// </param>
	/// <param name="collections">Include related sidecar files</param>
	/// <returns>List of batch rename mappings with preview of new names</returns>
	public List<BatchRenameMapping> PreviewBatchRename(
		List<string> filePaths, string tokenPattern, bool collections = true)
	{
		if ( filePaths.Count == 0 )
		{
			return [];
		}

		var pattern = new RenameTokenPattern(tokenPattern);
		if ( !pattern.IsValid )
		{
			return filePaths.Select(fp => new BatchRenameMapping
			{
				SourceFilePath = fp,
				HasError = true,
				ErrorMessage = $"Invalid pattern: {string.Join(", ", pattern.Errors)}"
			}).ToList();
		}

		// Get all file items from database
		var mappings = new List<BatchRenameMapping>();
		var fileItems = new Dictionary<string, FileIndexItem>();

		foreach ( var filePath in filePaths )
		{
			var detailView = query.SingleItem(filePath,
				null, collections, false);
			if ( detailView?.FileIndexItem == null )
			{
				mappings.Add(new BatchRenameMapping
				{
					SourceFilePath = filePath,
					HasError = true,
					ErrorMessage = "File not found in database"
				});
				continue;
			}

			fileItems[filePath] = detailView.FileIndexItem;
		}

		// Generate mappings for valid files
		var validMappings = new List<BatchRenameMapping>();
		foreach ( var (key, fileItem) in fileItems )
		{
			try
			{
				var parentPath = fileItem.ParentDirectory;
				var newFileName = pattern.GenerateFileName(fileItem);
				var newFilePath = $"{parentPath}/{newFileName}";

				var mapping = new BatchRenameMapping
				{
					SourceFilePath = key,
					// where to
					TargetFilePath = newFilePath,
					// SequenceNumber follows in a later step
					SequenceNumber = 0
				};

				// Add related files (sidecars)
				if ( collections )
				{
					mapping.RelatedFilePaths = GetRelatedFilePaths(key, newFilePath);
				}

				validMappings.Add(mapping);
			}
			catch ( Exception ex )
			{
				mappings.Add(new BatchRenameMapping
				{
					SourceFilePath = key,
					HasError = true,
					ErrorMessage = $"Failed to generate filename: {ex.Message}"
				});
			}
		}

		// Assign sequence numbers for duplicate target names
		AssignSequenceNumbers(validMappings, pattern, fileItems);

		mappings.AddRange(validMappings);
		return mappings;
	}

	/// <summary>
	///     Execute batch rename operation
	/// </summary>
	/// <param name="mappings">List of rename mappings from preview</param>
	/// <param name="collections">Include related sidecar files</param>
	/// <returns>List of updated file index items with status</returns>
	public async Task<List<FileIndexItem>> ExecuteBatchRenameAsync(
		List<BatchRenameMapping> mappings, bool collections = true)
	{
		if ( mappings.Count == 0 )
		{
			return [];
		}

		var results = new List<FileIndexItem>();

		// Filter out error mappings
		var validMappings = mappings.Where(m => !m.HasError).ToList();

		foreach ( var mapping in validMappings )
		{
			try
			{
				var detailView = query.SingleItem(mapping.SourceFilePath, null, collections, false);
				if ( detailView?.FileIndexItem == null )
				{
					results.Add(new FileIndexItem(mapping.SourceFilePath)
					{
						Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
					});
					continue;
				}

				// Use existing FromFileToDeleted logic to perform rename
				var fileIndexItems = new List<FileIndexItem>();
				await RenameFromFileToDeleted(mapping.SourceFilePath, mapping.TargetFilePath,
					results, fileIndexItems, detailView);
			}
			catch ( Exception ex )
			{
				results.Add(new FileIndexItem(mapping.SourceFilePath)
				{
					Status = FileIndexItem.ExifStatus.OperationNotSupported, Tags = ex.Message
				});
			}
		}

		return results;
	}

	/// <summary>
	///     Get all related file paths (sidecars) for a given file
	/// </summary>
	private List<(string source, string target)> GetRelatedFilePaths(string sourceFilePath,
		string targetFilePath)
	{
		var related = new List<(string, string)>();

		// Check for JSON sidecar
		var sourceJson = JsonSidecarLocation.JsonLocation(sourceFilePath);
		if ( iStorage.ExistFile(sourceJson) )
		{
			var targetJson = JsonSidecarLocation.JsonLocation(targetFilePath);
			related.Add(( sourceJson, targetJson ));
		}

		// Check for XMP sidecar
		if ( !ExtensionRolesHelper.IsExtensionForceXmp(sourceFilePath) )
		{
			return related;
		}

		var sourceXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(sourceFilePath);
		if ( !iStorage.ExistFile(sourceXmp) )
		{
			return related;
		}

		var targetXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(targetFilePath);
		related.Add(( sourceXmp, targetXmp ));

		return related;
	}

	/// <summary>
	///     Assign sequence numbers to files with identical target names
	/// </summary>
	private void AssignSequenceNumbers(
		List<BatchRenameMapping> mappings,
		RenameTokenPattern pattern,
		Dictionary<string, FileIndexItem> fileItems)
	{
		// Group by original filename base and datetime
		var grouped = mappings
			.GroupBy(m =>
			{
				var fileItem = fileItems[m.SourceFilePath];
				var fileName = fileItem.FileName ?? string.Empty;
				return new
				{
					OriginalBase =
						string.IsNullOrEmpty(fileName)
							? string.Empty
							: FilenamesHelper.GetFileNameWithoutExtension(fileName),
					fileItem.DateTime
				};
			})
			.ToList();

		var sequence = 0;
		foreach ( var group in grouped.OrderBy(g => g.Key.OriginalBase)
			         .ThenBy(g => g.Key.DateTime) )
		{
			foreach ( var mapping in group )
			{
				var fileItem = fileItems[mapping.SourceFilePath];
				var parentPath = fileItem.ParentDirectory ?? "/";
				var newFileName = pattern.GenerateFileName(fileItem, sequence);
				var newFilePath = $"{parentPath}/{newFileName}";
				mapping.TargetFilePath = newFilePath;
				mapping.SequenceNumber = sequence;
				if ( mapping.RelatedFilePaths.Count > 0 )
				{
					mapping.RelatedFilePaths =
						GetRelatedFilePaths(mapping.SourceFilePath, newFilePath);
				}
			}

			sequence++;
		}
	}
}

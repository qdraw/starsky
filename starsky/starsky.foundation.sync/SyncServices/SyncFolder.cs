#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices
{

	public sealed class SyncFolder
	{
		private readonly AppSettings _appSettings;
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private IQuery _query;
		private readonly IStorage _subPathStorage;
		private readonly IConsole _console;
		private readonly Duplicate _duplicate;
		private readonly IWebLogger _logger;
		private readonly IMemoryCache? _memoryCache;
		private readonly SyncIgnoreCheck _syncIgnoreCheck;

		public SyncFolder(AppSettings appSettings, IQuery query, 
			ISelectorStorage selectorStorage, IConsole console, IWebLogger logger, IMemoryCache? memoryCache)
		{
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_appSettings = appSettings;
			_setupDatabaseTypes = new SetupDatabaseTypes(appSettings);
			_query = query;
			_console = console;
			_duplicate = new Duplicate(_query);
			_logger = logger;
			_memoryCache = memoryCache;
			_syncIgnoreCheck = new SyncIgnoreCheck(appSettings, console);
		}

		public async Task<List<FileIndexItem>> Folder(string inputSubPath,
			ISynchronize.SocketUpdateDelegate? updateDelegate = null, DateTime? childDirectoriesAfter = null)
		{
			var subPaths = new List<string> {inputSubPath};	
			
			subPaths.AddRange(_subPathStorage.GetDirectoryRecursive(inputSubPath)
				.Where(p =>
				{
					if ( childDirectoriesAfter is not { Year: > 2000 } ) return true;
					return p.Value >= childDirectoriesAfter;
				})
				.Select(p => p.Key));
			
			// Loop trough all folders recursive
			var resultChunkList = await subPaths.ForEachAsync(
				async subPath =>
				{
					var allResults = new List<FileIndexItem>();
					var query = new QueryFactory(_setupDatabaseTypes, _query,_memoryCache, _appSettings, _logger).Query();
					// get only direct child files and folders and NOT recursive
					var fileIndexItems = await query!.GetAllObjectsAsync(subPath);
					fileIndexItems = await new Duplicate(query).RemoveDuplicateAsync(fileIndexItems);
				
					// And check files within this folder
					var pathsOnDisk = _subPathStorage.GetAllFilesInDirectory(subPath)
						.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();

					if ( fileIndexItems.Any() ) _console.Write("⁘");
				
					var indexItems = await LoopOverFolder(fileIndexItems, pathsOnDisk, updateDelegate, false);
					allResults.AddRange(indexItems);

					var dirItems = (await CheckIfFolderExistOnDisk(fileIndexItems)).Where(p => p != null).ToList();
					if ( dirItems.Any() )
					{
						allResults.AddRange(dirItems!);
					}

					await query.DisposeAsync();
					return allResults;
				}, _appSettings.MaxDegreesOfParallelism);
			
			// Convert chunks into one list
			var allResults = new List<FileIndexItem>();
			foreach ( var resultChunk in resultChunkList )
			{
				allResults.AddRange(resultChunk);
			}
			
			// query.DisposeAsync is called to avoid memory usage
			_query = new QueryFactory(_setupDatabaseTypes, _query, _memoryCache, _appSettings, _logger).Query()!;
			
			// // remove the duplicates from a large list of folders
			var folderList = await _query.GetObjectsByFilePathQueryAsync(subPaths);
			folderList = await _duplicate.RemoveDuplicateAsync(folderList);

			await CompareFolderListAndFixMissingFolders(subPaths, folderList);

			var parentItems = (await AddParentFolder(inputSubPath, allResults))
				.Where( p => p.Status != FileIndexItem.ExifStatus.OkAndSame).ToList();
			if ( parentItems.Any() )
			{
				allResults.AddRange(parentItems);
			}

			var socketUpdates = allResults.Where(p =>
				p.Status is FileIndexItem.ExifStatus.Ok
					or FileIndexItem.ExifStatus.Deleted).ToList();
			
			if ( updateDelegate != null && socketUpdates.Any() )
			{
				await updateDelegate(socketUpdates);
			}
			
			return allResults;
		}
	
		internal async Task CompareFolderListAndFixMissingFolders(List<string> subPaths, List<FileIndexItem> folderList)
		{
			if ( subPaths.Count == folderList.Count ) return;
			
			foreach ( var path in subPaths.Where(path => folderList.All(p => p.FilePath != path) &&
				_subPathStorage.ExistFolder(path) && !_syncIgnoreCheck.Filter(path) ) )
			{
				await _query.AddItemAsync(new FileIndexItem(path)
				{
					IsDirectory = true,
					AddToDatabase = DateTime.UtcNow,
					ColorClass = ColorClassParser.Color.None
				});
			}
		}
	
		internal async Task<List<FileIndexItem>> AddParentFolder(string subPath, List<FileIndexItem>? allResults)
		{
			allResults ??= new List<FileIndexItem>();
			// used to check later if the item already exists, to avoid duplicates
			var filePathsAllResults = allResults.Select(p => p.FilePath).ToList();

			if ( allResults.All(p => p.FilePath != subPath))
			{
				var subPathStatus = _subPathStorage.IsFolderOrFile(subPath);
				var exifStatus = subPathStatus ==
					FolderOrFileModel.FolderOrFileTypeList.Deleted
						? FileIndexItem.ExifStatus.NotFoundSourceMissing : FileIndexItem.ExifStatus.Ok;
				// if the status is not found, we assume its a folder, but that can't be checked
				var isDirectory = subPathStatus ==
					FolderOrFileModel.FolderOrFileTypeList.Folder || exifStatus == FileIndexItem.ExifStatus.NotFoundSourceMissing;
				
				allResults.Add(new FileIndexItem(subPath){
					IsDirectory = isDirectory, 
					Status = exifStatus});
			}

			List<string> Merge(IReadOnlyCollection<FileIndexItem> allResults1)
			{
				var parentDirectoriesStart = allResults1
					.Where(p => p.FilePath != null)
					.Select(p => p.ParentDirectory).ToList();
				parentDirectoriesStart.AddRange(allResults1
					.Where(p => p.IsDirectory == true)
					.Select(p => p.FilePath));
				return  parentDirectoriesStart.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList()!;
			}

			var parentDirectories = Merge(allResults);

			var itemsInDatabase = await _query.GetObjectsByFilePathAsync(parentDirectories, false);

			var newItems = new List<FileIndexItem>();

			foreach ( var databaseItem in itemsInDatabase )
			{
				parentDirectories.Remove(databaseItem.FilePath!);
			}

			foreach ( var parentDirectory in parentDirectories )
			{
				var status = _subPathStorage.ExistFolder(parentDirectory)
					? FileIndexItem.ExifStatus.Ok
					: FileIndexItem.ExifStatus.NotFoundSourceMissing;
				var item = new FileIndexItem(parentDirectory)
				{
					Status = status,
					IsDirectory = true,
					LastEdited = DateTime.UtcNow,
					ImageFormat = ExtensionRolesHelper.ImageFormat.unknown
				};

				if ( filePathsAllResults.Contains(item.FilePath!) )
				{
					item.Status = FileIndexItem.ExifStatus.OkAndSame;
				}
				
				newItems.Add(item);
			}
			
			await _query.AddRangeAsync(newItems.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList());

			return newItems;
		}

		private async Task<List<FileIndexItem>> LoopOverFolder(
			IReadOnlyCollection<FileIndexItem> fileIndexItems,
			IReadOnlyCollection<string> pathsOnDisk,
			ISynchronize.SocketUpdateDelegate? updateDelegate, bool addParentFolder)
		{
			var fileIndexItemsOnlyFiles = fileIndexItems
				.Where(p => p.IsDirectory == false).ToList();
			
			var pathsToUpdateInDatabase = PathsToUpdateInDatabase(fileIndexItemsOnlyFiles, pathsOnDisk);
			if ( !pathsToUpdateInDatabase.Any() ) return new List<FileIndexItem>();

			var resultChunkList = await pathsToUpdateInDatabase.Chunk(50).ForEachAsync(
				async chunks =>
				{
					var subPathInFiles = chunks.ToList();
				
					var query = new QueryFactory(_setupDatabaseTypes, _query,_memoryCache, _appSettings, _logger).Query();
					var syncMultiFile = new SyncMultiFile(_appSettings, query,
						_subPathStorage,
						null,
						_logger);
					var databaseItems = await syncMultiFile.MultiFile(subPathInFiles, updateDelegate, addParentFolder);
				
					await new SyncRemove(_appSettings, _setupDatabaseTypes,
							query, _memoryCache, _logger)
						.Remove(databaseItems, updateDelegate);

					foreach ( var item in databaseItems )
					{
						_console.Write(item.Status == FileIndexItem.ExifStatus
							.NotFoundSourceMissing
							? "≠"
							: "•");
					}
				
					return databaseItems;
				}, _appSettings.MaxDegreesOfParallelism);


			var results = new List<FileIndexItem>();
			foreach ( var resultChunk in resultChunkList )
			{
				results.AddRange(resultChunk);
			}
		
			return results;
		}

		internal static List<FileIndexItem> PathsToUpdateInDatabase(
			List<FileIndexItem> databaseItems, 
			IReadOnlyCollection<string> pathsOnDisk)
		{
			var resultDatabaseItems = new List<FileIndexItem>(databaseItems);
			foreach ( var path in pathsOnDisk )
			{
				var item = databaseItems.FirstOrDefault(p => string.Equals(p.FilePath, path, StringComparison.InvariantCultureIgnoreCase));
				if (item == null ) // when the file should be added to the index
				{
					// Status is used by MultiFile
					resultDatabaseItems.Add(new FileIndexItem(path){Status = FileIndexItem.ExifStatus.NotFoundNotInIndex});
					continue;
				}
				resultDatabaseItems.Add(item);
			}
			return resultDatabaseItems.DistinctBy(p => p.FilePath).ToList();
		}
	
		private async Task<List<FileIndexItem?>> CheckIfFolderExistOnDisk(List<FileIndexItem> fileIndexItems)
		{
			var fileIndexItemsOnlyFolders = fileIndexItems
				.Where(p => p.IsDirectory == true).ToList();
			
			if ( !fileIndexItemsOnlyFolders.Any() ) return new List<FileIndexItem?>();
		
			return (await fileIndexItemsOnlyFolders
				.ForEachAsync(async item =>
				{
					// assume only the input of directories
					if ( _subPathStorage.ExistFolder(item.FilePath!) )
					{
						return null;
					}
					var query = new QueryFactory(_setupDatabaseTypes, _query,_memoryCache, _appSettings, _logger).Query();
					return await RemoveChildItems(query!, item);
				}, _appSettings.MaxDegreesOfParallelism)).ToList();
		}
	
		/// <summary>
		/// Remove all items that are included
		/// </summary>
		/// <param name="query">To run async a query object</param>
		/// <param name="item">root item</param>
		/// <returns>root item</returns>
		internal async Task<FileIndexItem> RemoveChildItems(IQuery query, FileIndexItem item)
		{
			// Child items within
			var removeItems = await _query.GetAllRecursiveAsync(item.FilePath!);
			foreach ( var remove in removeItems )
			{
				_console.Write("✕");
				await query.RemoveItemAsync(remove);
			}
			
			// Item it self
			await query.RemoveItemAsync(item);
			_console.Write("✕");
			item.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;

			// only used in loop
			await query.DisposeAsync();

			return item;
		}
	}
	
}

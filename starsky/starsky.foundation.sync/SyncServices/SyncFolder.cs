using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncFolder
	{
		private readonly AppSettings _appSettings;
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private readonly IQuery _query;
		private readonly IStorage _subPathStorage;
		private readonly ISelectorStorage _selectorStorage;
		private readonly IConsole _console;

		public SyncFolder(AppSettings appSettings, IServiceScopeFactory serviceScopeFactory, IQuery query, 
			ISelectorStorage selectorStorage, IConsole console)
		{
			_selectorStorage = selectorStorage;
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_appSettings = appSettings;
			_setupDatabaseTypes = new SetupDatabaseTypes(appSettings,
				serviceScopeFactory.CreateScope().ServiceProvider.GetService<IServiceCollection>());
			_query = query;
			_console = console;
		}

		public async Task<List<FileIndexItem>> Folder(string inputSubPath)
		{
			var subPaths = new List<string> {inputSubPath};	
			subPaths.AddRange(_subPathStorage.GetDirectoryRecursive(inputSubPath));
			
			var allResults = new List<FileIndexItem>();
			foreach ( var subPath in subPaths )
			{
				var fileIndexItems = await _query.GetAllFilesAsync(subPath);
				var pathsOnDisk = _subPathStorage.GetAllFilesInDirectory(subPath)
					.Where(ExtensionRolesHelper.IsExtensionSyncSupported).ToList();

				var indexItems = await LoopOverFolder(fileIndexItems, pathsOnDisk);
				allResults.AddRange(indexItems);
			}
			allResults.Add(await AddParentFolder(inputSubPath));
			return allResults;
		}
		
		private async Task<List<FileIndexItem>> LoopOverFolder(IEnumerable<FileIndexItem> fileIndexItems, 
			IReadOnlyCollection<string> pathsOnDisk)
		{
			var pathsToUpdateInDatabase = PathsToUpdateInDatabase(fileIndexItems, pathsOnDisk);
			if ( !pathsToUpdateInDatabase.Any() ) return new List<FileIndexItem>();

			var result = await pathsToUpdateInDatabase
				.ForEachAsync(async subPathInFiles =>
				{
					var query = await new QueryFactory(_setupDatabaseTypes, _query).Query();
					var dbItem = await new SyncSingleFile(_appSettings, query, 
						_selectorStorage, null).SingleFile(subPathInFiles);
					if ( dbItem.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing )
					{
						await new SyncRemove(_appSettings, _setupDatabaseTypes, query)
							.Remove(subPathInFiles);
						_console.Write("≠");
						return dbItem;
					}
					_console.Write("•");
					return dbItem;
				}, _appSettings.MaxDegreesOfParallelism);
			return result == null ? new List<FileIndexItem>() : result.ToList();
		}
		
		internal async Task<FileIndexItem> AddParentFolder(string subPath)
		{
			var item = await _query.GetObjectByFilePathAsync(subPath);
			
			// Current item exist
			if ( item != null )
			{
				item.Status = FileIndexItem.ExifStatus.Ok;
				return item;
			}

			// Not on disk
			if ( !_subPathStorage.ExistFolder(subPath) )
			{
				return new FileIndexItem(subPath)
				{
					Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
				};
			}
			
			// not in db but should add this
			item = await _query.AddItemAsync(new FileIndexItem(subPath){
				IsDirectory = true
			});
			item.SetLastEdited();
			item.Status = FileIndexItem.ExifStatus.Ok;
			item.ImageFormat = ExtensionRolesHelper.ImageFormat.unknown;
			
			// also add the parent of this folder 
			await _query.AddParentItemsAsync(subPath);
			return item;
		}

		internal static List<string> PathsToUpdateInDatabase(IEnumerable<FileIndexItem> fileIndexItems, 
			IReadOnlyCollection<string> pathsOnDisk)
		{
			var pathFormFileIndexItems = fileIndexItems.Select(p => p.FilePath).ToList();

			// files that still lives in the db but not on disk
			var pathsToRemovedFromDb = pathFormFileIndexItems.Except(pathsOnDisk).ToList();

			// and combine all items
			var pathsToScan = new List<string>(pathsToRemovedFromDb);
			pathsToScan.AddRange(pathsOnDisk);
			// and order by alphabet and remove duplicates
			return new HashSet<string>(pathsToScan).OrderBy(p => p).ToList();
		}
	}
}

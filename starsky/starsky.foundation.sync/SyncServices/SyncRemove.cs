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

namespace starsky.foundation.sync.SyncServices
{
	public sealed class SyncRemove
	{
		private readonly AppSettings _appSettings;
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private readonly IQuery _query;
		private readonly IMemoryCache _memoryCache;
		private readonly IWebLogger _logger;

		public SyncRemove(AppSettings appSettings, IQuery query, IMemoryCache memoryCache, IWebLogger logger)
		{
			_appSettings = appSettings;
			_setupDatabaseTypes = new SetupDatabaseTypes(appSettings);
			_memoryCache = memoryCache;
			_logger = logger;
			_query = query;
		}

		public SyncRemove(AppSettings appSettings, SetupDatabaseTypes setupDatabaseTypes,
			IQuery query, IMemoryCache memoryCache, IWebLogger logger)
		{
			_appSettings = appSettings;
			_setupDatabaseTypes = setupDatabaseTypes;
			_memoryCache = memoryCache;
			_query = query;
			_logger = logger;
		}

		/// <summary>
		/// remove path from database
		/// </summary>
		/// <param name="subPath">subPath</param>
		/// <returns></returns>
		public async Task<List<FileIndexItem>> Remove(string subPath)
		{
			return await Remove(new List<string> {subPath});
		}

		/// <summary>
		/// Remove list from database, Does not check if the file exist on disk
		/// </summary>
		/// <param name="subPaths">list of sub paths</param>
		/// <returns>file with status</returns>
		public async Task<List<FileIndexItem>> Remove(List<string> subPaths)
		{
			// Get folders
			var toDeleteList = await _query.GetAllRecursiveAsync(subPaths);
			// and single objects 
			toDeleteList.AddRange(await _query.GetObjectsByFilePathQueryAsync(subPaths));
			
			await toDeleteList
				.ForEachAsync(async item =>
				{
					var query = new QueryFactory(_setupDatabaseTypes, _query, _memoryCache, _appSettings, _logger).Query();
					await query!.RemoveItemAsync(item);
					item.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
					// only dispose inside parallelism loop
					await query.DisposeAsync();
					return item;
				}, _appSettings.MaxDegreesOfParallelism);

			await LoopOverSidecarFiles(subPaths);

			// Add items that are not in the database
			foreach ( var subPath in subPaths.Where(subPath => 
				!toDeleteList.Exists(p => p.FilePath == subPath)) )
			{
				toDeleteList.Add(new FileIndexItem(subPath)
				{
					Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
				});
			}
			
			return toDeleteList.OrderBy(p => p.FilePath).ToList();
		}
		
		/// <summary>
		/// Remove from database and Gives only back the files that are deleted
		/// </summary>
		/// <param name="databaseItems">input of files with deleted status (NotFoundSourceMissing)</param>
		/// <returns>Gives only back the files that are deleted</returns>
		public async Task<List<FileIndexItem>> Remove(IEnumerable<FileIndexItem> databaseItems)
		{
			var deleted = databaseItems
				.Where(p =>
					p.Status is FileIndexItem.ExifStatus
						.NotFoundSourceMissing).Select(p => p.FilePath).ToList();
			
			return await Remove(deleted);
		}
		
		private async Task LoopOverSidecarFiles(List<string> subPaths)
		{
			var parentDirectories = new HashSet<string>();
			var xmpSubPaths = subPaths
				.Where(ExtensionRolesHelper.IsExtensionSidecar).ToList();
			foreach ( var xmpPath in xmpSubPaths )
			{
				parentDirectories.Add(FilenamesHelper.GetParentPath(xmpPath));
			}

			var itemsInDirectories = new HashSet<FileIndexItem>(
				await _query.GetAllFilesAsync(parentDirectories.ToList()));

			// that is an filepath without extension
			var collectionPath = xmpSubPaths.Select(singlePath
				=> $"{FilenamesHelper.GetParentPath(singlePath)}/" +
			FilenamesHelper.GetFileNameWithoutExtension(singlePath)).ToList();

			foreach ( var item in itemsInDirectories )
			{
				foreach ( var singleCollectionPath in collectionPath )
				{
					if ( item.FilePath.StartsWith(singleCollectionPath) 
					     && !ExtensionRolesHelper.IsExtensionSidecar(item.FilePath) )
					{
						item.RemoveSidecarExtension("xmp");
						await _query.UpdateItemAsync(item);
					}
				}
			}
		}
	}
}

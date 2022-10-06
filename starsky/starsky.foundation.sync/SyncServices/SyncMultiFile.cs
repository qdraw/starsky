using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices
{
	
	public class SyncMultiFile
	{
		private readonly IQuery _query;
		private readonly IWebLogger _logger;
		private readonly SyncSingleFile _syncSingleFile;
		private readonly IStorage _subPathStorage;
		private readonly AppSettings _appSettings;

		public SyncMultiFile(AppSettings appSettings, IQuery query, IStorage subPathStorage, IWebLogger logger)
		{
			_query = query;
			_syncSingleFile =
				new SyncSingleFile(appSettings, query, subPathStorage, logger);
			_subPathStorage = subPathStorage;
			_logger = logger;
			_appSettings = appSettings;
		}

		/// <summary>
		/// Sync List of Files
		/// </summary>
		/// <param name="subPathInFiles">subPaths style</param>
		/// <param name="updateDelegate">callback when done</param>
		/// <returns>items that are changed</returns>
		internal async Task<List<FileIndexItem>> MultiFile(List<string> subPathInFiles,
			ISynchronize.SocketUpdateDelegate updateDelegate = null)
		{
			var databaseItems = await _query.GetObjectsByFilePathQueryAsync(subPathInFiles);

			var resultDatabaseItems = new List<FileIndexItem>();
			foreach ( var path in subPathInFiles )
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

			return await MultiFile(resultDatabaseItems, updateDelegate);
		}
	
		/// <summary>
		/// For Checking single items without querying the database
		/// </summary>
		/// <param name="dbItems">current items</param>
		/// <param name="updateDelegate">push updates realtime to the user and avoid waiting</param>
		/// <returns>updated item with status</returns>
		internal async Task<List<FileIndexItem>> MultiFile(List<FileIndexItem> dbItems,
			ISynchronize.SocketUpdateDelegate updateDelegate = null)
		{
			if ( dbItems == null ) return new List<FileIndexItem>();
			
			// // TODO: BATCH add
			// if ( dbItem.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex )
			// {
			// 	updatedDbItems.Add(await _syncSingleFile.NewItem(statusItem, dbItem.FilePath));
			// 	return updatedDbItems;
			// }
			List<FileIndexItem> updatedDbItems = await _syncSingleFile.NewItem(statusItem, dbItem.FilePath));
			
			updatedDbItems.AddRange(await dbItems.Where( p => p.Status != FileIndexItem.ExifStatus.NotFoundNotInIndex).ForEachAsync(
				async dbItem =>
				{
					// await _syncSingleFile.UpdateSidecarFile(dbItem.FilePath);
			
					var statusItem =  _syncSingleFile.CheckForStatusNotOk(dbItem.FilePath);
					if ( statusItem.Status != FileIndexItem.ExifStatus.Ok )
					{
						_logger.LogDebug($"[MultiFile/db] status {statusItem.Status} for {dbItem.FilePath} {Synchronize.DateTimeDebug()}");
						return statusItem;
					}
			
					var (isSame, updatedDbItem) = await _syncSingleFile.SizeFileHashIsTheSame(dbItem);
					if ( !isSame )
					{
						return await _syncSingleFile.UpdateItem(dbItem, updatedDbItem.Size, dbItem.FilePath, false);
					}
			
					updatedDbItem.Status = FileIndexItem.ExifStatus.OkAndSame;
					SyncSingleFile.AddDeleteStatus(updatedDbItem, FileIndexItem.ExifStatus.DeletedAndSame);
			
					return updatedDbItem;
				}, _appSettings.MaxDegreesOfParallelism));

			updatedDbItems = await AddParentItems(updatedDbItems);
		
			if ( updateDelegate == null ) return updatedDbItems;
			return await PushToSocket(updatedDbItems, updateDelegate);
		}
	
		private static async Task<List<FileIndexItem>> PushToSocket(List<FileIndexItem> updatedDbItems,
			ISynchronize.SocketUpdateDelegate updateDelegate)
		{
			var notOkayAndSame = updatedDbItems.Where(p =>
				p.Status != FileIndexItem.ExifStatus.OkAndSame).ToList();
			if ( notOkayAndSame.Any() )
			{
				await updateDelegate(notOkayAndSame);
			}
			return updatedDbItems;
		}

		private async Task<List<FileIndexItem>> AddParentItems(List<FileIndexItem> updatedDbItems)
		{
			// give parent folders back
			var addedParentItems = new List<FileIndexItem>();
			foreach ( var subPath in updatedDbItems
				         .Select(p => p.ParentDirectory).Distinct()
				         .Where(p => _subPathStorage.ExistFolder(p)))
			{
				addedParentItems.AddRange(await _query.AddParentItemsAsync(subPath));
			}
			updatedDbItems.AddRange(addedParentItems);
			return updatedDbItems;
		}

	}

}

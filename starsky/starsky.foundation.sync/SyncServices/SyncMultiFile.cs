using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices
{
	
	public sealed class SyncMultiFile
	{
		private readonly IQuery _query;
		private readonly IWebLogger _logger;
		private readonly SyncSingleFile _syncSingleFile;
		private readonly IStorage _subPathStorage;
		private readonly AppSettings _appSettings;

		public SyncMultiFile(AppSettings appSettings, IQuery query, IStorage subPathStorage, IMemoryCache cache, IWebLogger logger)
		{
			_query = query;
			_syncSingleFile =
				new SyncSingleFile(appSettings, query, subPathStorage, cache, logger);
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
			_logger.LogInformation("MultiFileQuery: " + string.Join(",", subPathInFiles));
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
		/// <param name="addParentFolder"></param>
		/// <returns>updated item with status</returns>
		internal async Task<List<FileIndexItem>> MultiFile(
			List<FileIndexItem> dbItems,
			ISynchronize.SocketUpdateDelegate updateDelegate = null,
			bool addParentFolder = true)
		{
			if ( dbItems == null ) return new List<FileIndexItem>();
						
			SyncSingleFile.AddDeleteStatus(dbItems, FileIndexItem.ExifStatus.DeletedAndSame);

			// Update XMP files, does an extra query to the database. in the future this needs to be refactored
			foreach ( var xmpOrSidecarFiles in dbItems.Where(p => ExtensionRolesHelper.IsExtensionSidecar(p.FilePath)) )
			{
				// Query!
				await _syncSingleFile.UpdateSidecarFile(xmpOrSidecarFiles.FilePath);
			}
			
			var statusItems =  _syncSingleFile.CheckForStatusNotOk(dbItems.Select(p => p.FilePath)).ToList();
			foreach ( var statusItem in statusItems )
			{
				var dbItemSearchedIndex = dbItems.FindIndex(p =>
					p.FilePath == statusItem.FilePath);
				var dbItemSearched = dbItems[dbItemSearchedIndex];
				
				if ( dbItemSearched == null || (dbItemSearched.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex 
				                                // why statusItem.Status?
				                                && statusItem.Status == FileIndexItem.ExifStatus.Ok))
				{
					continue;
				}
				
				dbItems[dbItemSearchedIndex].Status = statusItem.Status;
				
				if ( dbItemSearched is { Status: FileIndexItem.ExifStatus.Ok } )
				{
					// there is still a check if the file is not changed see: SizeFileHashIsTheSame
					dbItems[dbItemSearchedIndex].Status = FileIndexItem.ExifStatus.OkAndSame;
				}
			}
		
			// Multi thread check for file hash
			var isSameUpdatedItemList = await dbItems.Where(p => p.Status == FileIndexItem.ExifStatus.OkAndSame)
				.ForEachAsync(
					async dbItem => await _syncSingleFile.SizeFileHashIsTheSame(dbItem),
					_appSettings.MaxDegreesOfParallelism);
			
			if ( isSameUpdatedItemList != null )
			{
				foreach ( var (_,_,isSameUpdatedItem) in isSameUpdatedItemList.Where(p=> !p.Item1) )
				{
					await _syncSingleFile.UpdateItem(isSameUpdatedItem,
						isSameUpdatedItem.Size,
						isSameUpdatedItem.FilePath, false);
				}
			}

			// add new items
			var newItemsList = await _syncSingleFile.NewItem(
				dbItems.Where(p =>
					p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex
				).ToList(), false);
			foreach ( var newItem in newItemsList )
			{
				var newItemIndex = dbItems.FindIndex(
					p => p.FilePath == newItem.FilePath);
				if ( newItemIndex < 0 ) continue;
				newItem.Status = FileIndexItem.ExifStatus.Ok;
				SyncSingleFile.AddDeleteStatus(newItem);
				dbItems[newItemIndex] = newItem;
			}

			if ( addParentFolder )
			{
				_logger.LogInformation("Add Parent Folder For: " + string.Join(",", dbItems.Select(p => p.FilePath)));
				dbItems = await new AddParentList(_subPathStorage, _query).AddParentItems(dbItems);
			}
		
			if ( updateDelegate == null ) return dbItems;
			return await PushToSocket(dbItems, updateDelegate);
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



	}

}

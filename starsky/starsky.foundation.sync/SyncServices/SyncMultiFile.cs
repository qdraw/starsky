using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
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
		private readonly IStorage _subPathStorage;
		private readonly AppSettings _appSettings;
		private readonly NewUpdateItemWrapper _newUpdateItemWrapper;
		private readonly CheckForStatusNotOkHelper _checkForStatusNotOkHelper;

		public SyncMultiFile(AppSettings appSettings, IQuery query, 
			IStorage subPathStorage, IMemoryCache cache, IWebLogger logger)
		{
			_query = query;
			_subPathStorage = subPathStorage;
			_logger = logger;
			_appSettings = appSettings;
			_newUpdateItemWrapper = new NewUpdateItemWrapper(query, subPathStorage, appSettings, cache, logger);
			_checkForStatusNotOkHelper = new CheckForStatusNotOkHelper(_subPathStorage);
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
				var item = databaseItems.FirstOrDefault(p => 
					string.Equals(p.FilePath, path, StringComparison.InvariantCultureIgnoreCase));
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
						
			dbItems = DeleteStatusHelper.AddDeleteStatus(dbItems, FileIndexItem.ExifStatus.DeletedAndSame);

			var statusItems =  _checkForStatusNotOkHelper
				.CheckForStatusNotOk(dbItems.Select(p => p.FilePath)).ToList();
			UpdateCheckStatus(dbItems, statusItems);

			AddSidecarExtensionData(dbItems, statusItems);

			// Multi thread check for file hash
			var list = dbItems
				.Where(p => p.Status == FileIndexItem.ExifStatus.OkAndSame);
			var isSameUpdatedItemList = await list
				.ForEachAsync(
					async dbItem => await new SizeFileHashIsTheSameHelper(_subPathStorage)
						.SizeFileHashIsTheSame(dbItems
						.Where(p => p.FileCollectionName == dbItem.FileCollectionName).ToList(), dbItem.FilePath),
					_appSettings.MaxDegreesOfParallelism);

			dbItems = await IsSameUpdatedItemList(isSameUpdatedItemList?.ToList(), dbItems);

			// add new items
			var newItemsList = await _newUpdateItemWrapper.NewItem(
				dbItems.Where(p =>
					p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex
				).ToList(), false);
			foreach ( var newItem in newItemsList )
			{
				// only for new items that needs to be added to the db
				var newItemIndex = dbItems.FindIndex(
					p => p.FilePath == newItem.FilePath);
				if ( newItemIndex < 0 ) continue;
				newItem.Status = FileIndexItem.ExifStatus.Ok;
				DeleteStatusHelper.AddDeleteStatus(newItem);
				dbItems[newItemIndex] = newItem;
			}

			if ( addParentFolder )
			{
				_logger.LogInformation("Add Parent Folder For: " + 
				                       string.Join(",", dbItems.Select(p => p.FilePath)));
				
				dbItems = await new AddParentList(_subPathStorage, _query).AddParentItems(dbItems);
			}
		
			if ( updateDelegate == null ) return dbItems;
			return await PushToSocket(dbItems, updateDelegate);
		}

		private static void UpdateCheckStatus(List<FileIndexItem> dbItems, List<FileIndexItem> statusItems)
		{
			foreach ( var statusItem in statusItems )
			{
				var dbItemSearchedIndex = dbItems.FindIndex(p =>
					p.FilePath == statusItem.FilePath);
				if ( dbItemSearchedIndex < 0 )
				{
					continue;
				}
				var dbItemSearched = dbItems[dbItemSearchedIndex];
				
				if ( dbItemSearched == null || (dbItemSearched.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex 
				                                // why statusItem.Status?
				                                && statusItem.Status == FileIndexItem.ExifStatus.Ok))
				{
					continue;
				}

				dbItems[dbItemSearchedIndex].Status = statusItem.Status;
				
				if ( dbItemSearched is { Status: FileIndexItem.ExifStatus.Ok } ) // 0 check
				{
					// there is still a check if the file is not changed see: SizeFileHashIsTheSame
					dbItems[dbItemSearchedIndex].Status = FileIndexItem.ExifStatus.OkAndSame;
				}
			}
		}

		private async Task<List<FileIndexItem>> IsSameUpdatedItemList(IReadOnlyCollection<Tuple<bool?, bool?, FileIndexItem>> 
			isSameUpdatedItemList, List<FileIndexItem> dbItems)
		{
			if ( isSameUpdatedItemList == null ) return dbItems;
			
			foreach ( var (isLastEditedSame,isFileHashSame,isSameUpdatedItem) in 
			         isSameUpdatedItemList.Where(p=> p.Item1 != true) )
			{
				var updateItemIndex = dbItems.FindIndex(
					p => p.FilePath == isSameUpdatedItem.FilePath);
					
				if ( isLastEditedSame == false && isFileHashSame == true )
				{
					dbItems[updateItemIndex] = await _newUpdateItemWrapper.HandleLastEditedIsSame(isSameUpdatedItem, true);
					continue;
				}
					
				dbItems[updateItemIndex] = await _newUpdateItemWrapper.UpdateItem(isSameUpdatedItem,
					isSameUpdatedItem.Size,
					isSameUpdatedItem.FilePath, false);
			}

			return dbItems;
		}

		private static void AddSidecarExtensionData(List<FileIndexItem> dbItems, List<FileIndexItem> statusItems)
		{
			foreach ( var statusItem in statusItems )
			{
				foreach ( var item in dbItems.Where(p =>
					         p.FileCollectionName == statusItem.FileCollectionName
					         && p.ParentDirectory == statusItem.ParentDirectory
					         && ExtensionRolesHelper.IsExtensionSidecar(p.FileName) && 
					         p.Status is FileIndexItem.ExifStatus.Ok or FileIndexItem.ExifStatus.OkAndSame 
						         or FileIndexItem.ExifStatus.NotFoundNotInIndex))
				{
					var dbMatchItemSearchedIndex = dbItems.FindIndex(p =>
						p.ParentDirectory == item.ParentDirectory && p.FileCollectionName == item.FileCollectionName);
						
					dbItems[dbMatchItemSearchedIndex].AddSidecarExtension("xmp");
					dbItems[dbMatchItemSearchedIndex].LastChanged.Add(nameof(FileIndexItem.SidecarExtensions));
				}
			}
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

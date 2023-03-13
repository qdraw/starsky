using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.sync.Helpers;

public class NewUpdateItemWrapper
{
	private readonly NewItem _newItem;
	private readonly IQuery _query;
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;
	private readonly IStorage _subPathStorage;

	public NewUpdateItemWrapper(IQuery query, IStorage subPathStorage, AppSettings appSettings, IMemoryCache memoryCache, IWebLogger logger)
	{
		_newItem = new NewItem(subPathStorage, new ReadMeta(subPathStorage, appSettings, memoryCache, logger));
		_appSettings = appSettings;
		_query = query;
		_logger = logger;
		_subPathStorage = subPathStorage;
	}
	
	/// <summary>
	/// Create an new item in the database
	/// </summary>
	/// <param name="statusItem">contains the status</param>
	/// <param name="subPath">relative path</param>
	/// <returns>database item</returns>
	internal async Task<FileIndexItem> NewItem(FileIndexItem statusItem, string subPath)
	{
		// Add a new Item
		var dbItem = await _newItem.NewFileItem(statusItem);

		// When not OK do not Add (fileHash issues)
		if ( dbItem.Status != FileIndexItem.ExifStatus.Ok ) return dbItem;
				
		await _query.AddItemAsync(dbItem);
		await _query.AddParentItemsAsync(subPath);
		DeleteStatusHelper.AddDeleteStatus(dbItem);
		return dbItem;
	}

	internal async Task<FileIndexItem> HandleLastEditedIsSame(FileIndexItem updatedDbItem, bool? fileHashSame)
	{
		if ( _appSettings.SyncAlwaysUpdateLastEditedTime != true && fileHashSame == true)
		{
			return updatedDbItem;
		}

		return await UpdateItemLastEdited(updatedDbItem);
	}
	
	/// <summary>
	/// Only update the last edited time
	/// </summary>
	/// <param name="updatedDbItem">item incl updated last edited time</param>
	/// <returns>object with ok status</returns>
	internal async Task<FileIndexItem> UpdateItemLastEdited(FileIndexItem updatedDbItem)
	{
		await _query.UpdateItemAsync(updatedDbItem);
		updatedDbItem.Status = FileIndexItem.ExifStatus.Ok;
		DeleteStatusHelper.AddDeleteStatus(updatedDbItem);
		updatedDbItem.LastChanged =
			new List<string> {nameof(FileIndexItem.LastEdited)};
		return updatedDbItem;
	}


	
	/// <summary>
	/// Update item to database
	/// </summary>
	/// <param name="dbItem">item to update</param>
	/// <param name="size">byte size</param>
	/// <param name="subPath">relative path</param>
	/// <param name="addParentItems">auto add parent items</param>
	/// <returns>same item</returns>
	internal async Task<FileIndexItem> UpdateItem(FileIndexItem dbItem, long size, string subPath, bool addParentItems)
	{
		if ( _appSettings.ApplicationType == AppSettings.StarskyAppType.WebController )
		{
			_logger.LogDebug($"[SyncSingleFile] Trigger Update Item {subPath}");
		}
			
		var updateItem = await _newItem.PrepareUpdateFileItem(dbItem, size);
		await _query.UpdateItemAsync(updateItem);
		if ( addParentItems )
		{
			await _query.AddParentItemsAsync(subPath);
		}
		DeleteStatusHelper.AddDeleteStatus(dbItem);
		return updateItem;
	}

	/// <summary>
	/// Create an new item in the database
	/// </summary>
	/// <param name="statusItems">contains the status</param>
	/// <param name="addParentItem"></param>
	/// <returns>database item</returns>
	internal async Task<List<FileIndexItem>> NewItem(List<FileIndexItem> statusItems, bool addParentItem)
	{
		// Add a new Item
		var dbItems = await _newItem.NewFileItem(statusItems);

		// When not OK do not Add (fileHash issues)
		var okDbItems =
			dbItems.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList();
		await _query.AddRangeAsync(okDbItems);

		if ( addParentItem )
		{
			await new AddParentList(_subPathStorage, _query)
				.AddParentItems(okDbItems);
		}
		return dbItems;
	}

}

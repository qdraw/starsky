using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices;

public class SyncMultiFile
{
	private readonly IStorage _subPathStorage;
	private readonly IQuery _query;
	private readonly NewItem _newItem;
	private readonly IWebLogger _logger;
	private readonly AppSettings _appSettings;
	private readonly SyncSingleFile _syncSingleFile;

	public SyncMultiFile(AppSettings appSettings, IQuery query, IStorage subPathStorage, IWebLogger logger)
	{
		_appSettings = appSettings;
		_subPathStorage = subPathStorage;
		_query = query;
		_newItem = new NewItem(_subPathStorage, new ReadMeta(_subPathStorage, appSettings));
		_syncSingleFile =
			new SyncSingleFile(appSettings, query, subPathStorage, logger);
		_logger = logger;
	}

	/// <summary>
	/// For Checking single items without querying the database
	/// </summary>
	/// <param name="subPaths">paths</param>
	/// <param name="dbItems">current items</param>
	/// <param name="updateDelegate">push updates realtime to the user and avoid waiting</param>
	/// <returns>updated item with status</returns>
	internal async Task<FileIndexItem> MultiFile(List<FileIndexItem> dbItems,
		ISynchronize.SocketUpdateDelegate updateDelegate = null)
	{
		var updatedDbItems = new List<FileIndexItem>();
		foreach ( var dbItem in dbItems )
		{
			// todo sidecar files update
			
			var statusItem =  _syncSingleFile.CheckForStatusNotOk(dbItem.FilePath);
			if ( statusItem.Status != FileIndexItem.ExifStatus.Ok )
			{
				_logger.LogDebug($"[MultiFile/db] status {statusItem.Status} for {dbItem.FilePath} {Synchronize.DateTimeDebug()}");
				updatedDbItems.Add(statusItem);
				continue;
			}
			
			var (isSame, updatedDbItem) = await _syncSingleFile.SizeFileHashIsTheSame(dbItem);
			if ( !isSame )
			{
				updatedDbItems.Add(await _syncSingleFile.UpdateItem(dbItem, updatedDbItem.Size, dbItem.FilePath));
			}
			updatedDbItems.Add(dbItem);
		}

		// if ( updateDelegate != null )
		// {
		// 	await updateDelegate(new List<FileIndexItem> {dbItem});
		// }
	}


}

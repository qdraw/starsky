using System;
using System.Collections.Generic;
using System.Linq;
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
	private readonly IQuery _query;
	private readonly IWebLogger _logger;
	private readonly SyncSingleFile _syncSingleFile;

	public SyncMultiFile(AppSettings appSettings, IQuery query, IStorage subPathStorage, IWebLogger logger)
	{
		_query = query;
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
	internal async Task<List<FileIndexItem>> MultiFile(List<FileIndexItem> dbItems,
		ISynchronize.SocketUpdateDelegate updateDelegate = null)
	{
		var updatedDbItems = new List<FileIndexItem>();
		if ( dbItems == null ) return updatedDbItems;
		foreach ( var dbItem in dbItems )
		{
			await _syncSingleFile.UpdateSidecarFile(dbItem.FilePath);
			
			var statusItem =  _syncSingleFile.CheckForStatusNotOk(dbItem.FilePath);
			if ( statusItem.Status != FileIndexItem.ExifStatus.Ok )
			{
				_logger.LogDebug($"[MultiFile/db] status {statusItem.Status} for {dbItem.FilePath} {Synchronize.DateTimeDebug()}");
				updatedDbItems.Add(statusItem);
				continue;
			}

			if ( dbItem.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex )
			{
				updatedDbItems.Add(await _syncSingleFile.NewItem(statusItem, dbItem.FilePath));
				continue;
			}
			
			var (isSame, updatedDbItem) = await _syncSingleFile.SizeFileHashIsTheSame(dbItem);
			if ( !isSame )
			{
				updatedDbItems.Add(await _syncSingleFile.UpdateItem(dbItem, updatedDbItem.Size, dbItem.FilePath));
				continue;
			}
			
			updatedDbItem.Status = FileIndexItem.ExifStatus.OkAndSame;
			_syncSingleFile.AddDeleteStatus(updatedDbItem, FileIndexItem.ExifStatus.DeletedAndSame);
			
			updatedDbItems.Add(updatedDbItem);
		}

		if ( updateDelegate == null ) return updatedDbItems;
		
		var notOkayAndSame = updatedDbItems.Where(p =>
			p.Status != FileIndexItem.ExifStatus.OkAndSame).ToList();
		if ( notOkayAndSame.Any() )
		{
			await updateDelegate(notOkayAndSame);
		}

		return updatedDbItems;

	}

	internal async Task<List<FileIndexItem>> MultiFile(List<string> subPathInFiles,
		ISynchronize.SocketUpdateDelegate updateDelegate = null)
	{
		var databaseItems = await _query.GetObjectsByFilePathQueryAsync(subPathInFiles);

		var resultDatabaseItems = new List<FileIndexItem>();
		foreach ( var path in subPathInFiles )
		{
			var item = databaseItems.FirstOrDefault(p => p.FilePath == path);
			if (item == null )
			{
				resultDatabaseItems.Add(new FileIndexItem(path){Status = FileIndexItem.ExifStatus.NotFoundNotInIndex});
				continue;
			}
			resultDatabaseItems.Add(item);
		}

		return await MultiFile(resultDatabaseItems, updateDelegate);
	}
}

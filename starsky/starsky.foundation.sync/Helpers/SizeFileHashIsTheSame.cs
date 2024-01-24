using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;

namespace starsky.foundation.sync.Helpers;

public class SizeFileHashIsTheSameHelper
{
	private readonly IStorage _subPathStorage;

	public SizeFileHashIsTheSameHelper(IStorage subPathStorage)
	{
		_subPathStorage = subPathStorage;
	}

	/// <summary>
	/// When the same stop checking and return value
	/// </summary>
	/// <param name="dbItems">item that contain size and fileHash</param>
	/// <param name="subPath">which item</param>
	/// <returns>Last Edited is the bool (null is should check further in process), FileHash Same bool (null is not checked) , database item</returns>
	internal async Task<Tuple<bool?,bool?,FileIndexItem>> SizeFileHashIsTheSame(List<FileIndexItem> dbItems, string subPath)
	{
		var dbItem = dbItems.Find(p => p.FilePath == subPath);
		if ( dbItem == null )
		{
			return new Tuple<bool?, bool?, FileIndexItem>(false, false, null!);
		}
		
		// when last edited is the same
		var (isRequestFileLastEditTheSame, lastEdit,_) = CompareLastEditIsTheSame(dbItem);
		dbItem.LastEdited = lastEdit;
		dbItem.Size = _subPathStorage.Info(dbItem.FilePath!).Size;
		
		// compare raw files
		var otherRawItems = dbItems.Where(p =>
			ExtensionRolesHelper.IsExtensionForceXmp(p.FilePath) && !ExtensionRolesHelper.IsExtensionSidecar(p.FilePath))
			.Where(p => p.FilePath == subPath)
			.Select(CompareLastEditIsTheSame)
			.Where(p => p.Item1).ToList();
	
		if ( isRequestFileLastEditTheSame && otherRawItems.Count == 0 )
		{
			return new Tuple<bool?, bool?, FileIndexItem>(true, null, dbItem);
		}
		
		// when byte hash is different update
		var (requestFileHashTheSame,_ ) = await CompareFileHashIsTheSame(dbItem);
		
		// when there are xmp files in the list and the fileHash of the current raw is the same
		if ( isRequestFileLastEditTheSame && requestFileHashTheSame )
		{
			return new Tuple<bool?, bool?, FileIndexItem>(null, null, dbItem);
		}
		
		return new Tuple<bool?, bool?, FileIndexItem>(false, requestFileHashTheSame, dbItem);
	}
	
	/// <summary>
	/// Compare the file hash en return 
	/// </summary>
	/// <param name="dbItem">database item</param>
	/// <returns>tuple that has value: is the same; and the fileHash</returns>
	private async Task<Tuple<bool,string>> CompareFileHashIsTheSame(FileIndexItem dbItem)
	{
		var (localHash,_) = await new 
			FileHash(_subPathStorage).GetHashCodeAsync(dbItem.FilePath!);
		var isTheSame = dbItem.FileHash == localHash;
		dbItem.FileHash = localHash;
		return new Tuple<bool, string>(isTheSame, localHash);
	}
	
	/// <summary>
	/// True when result is the same
	/// </summary>
	/// <param name="dbItem"></param>
	/// <returns>lastWriteTime is the same, lastWriteTime, filePath</returns>
	private Tuple<bool,DateTime,string> CompareLastEditIsTheSame(FileIndexItem dbItem)
	{
		var lastWriteTime = _subPathStorage.Info(dbItem.FilePath!).LastWriteTime;
		if (lastWriteTime.Year == 1 )
		{
			return new Tuple<bool, DateTime,string>(false, lastWriteTime, dbItem.FilePath!);
		}

		var isTheSame = DateTime.Compare(dbItem.LastEdited, lastWriteTime) == 0;

		dbItem.LastEdited = lastWriteTime;
		return new Tuple<bool, DateTime,string>(isTheSame, lastWriteTime, dbItem.FilePath!);
	}
}

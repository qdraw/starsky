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
	/// <returns>Last Edited is the bool, FileHash Same bool , database item</returns>
	internal async Task<Tuple<bool,bool?,FileIndexItem>> SizeFileHashIsTheSame(List<FileIndexItem> dbItems, string subPath)
	{
		var dbItem = dbItems.FirstOrDefault(p => p.FilePath == subPath);
		if ( dbItem == null )
		{
			return new Tuple<bool, bool?, FileIndexItem>(false, false, null);
		}
		
		// when last edited is the same
		var (isLastEditTheSame, lastEdit) = CompareLastEditIsTheSame(dbItem);
		dbItem.LastEdited = lastEdit;
		dbItem.Size = _subPathStorage.Info(dbItem.FilePath!).Size;

		// compare xmp sidecar
		var isXmpLastEditTheSame = true;
		var xmpDbItem = dbItems.FirstOrDefault(p =>
			ExtensionRolesHelper.IsExtensionSidecar(p.FilePath) );
		if ( xmpDbItem != null )
		{
			(isXmpLastEditTheSame, _) = CompareLastEditIsTheSame(xmpDbItem);
		}
		
		if ( isLastEditTheSame && isXmpLastEditTheSame)
		{
			return new Tuple<bool, bool?, FileIndexItem>(true, null, dbItem);
		}
			
		// when byte hash is different update
		var (fileHashTheSame,_ ) = await CompareFileHashIsTheSame(dbItem);

		return new Tuple<bool, bool?, FileIndexItem>(false, fileHashTheSame && isXmpLastEditTheSame, dbItem);
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
	/// <returns></returns>
	private Tuple<bool,DateTime> CompareLastEditIsTheSame(FileIndexItem dbItem)
	{
		var lastWriteTime = _subPathStorage.Info(dbItem.FilePath!).LastWriteTime;
		if (lastWriteTime.Year == 1 )
		{
			return new Tuple<bool, DateTime>(false, lastWriteTime);
		}
			
		var isTheSame = dbItem.LastEdited == lastWriteTime;

		dbItem.LastEdited = lastWriteTime;
		return new Tuple<bool, DateTime>(isTheSame, lastWriteTime);
	}
}

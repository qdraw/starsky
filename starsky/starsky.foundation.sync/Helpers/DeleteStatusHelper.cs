using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Models;

namespace starsky.foundation.sync.Helpers;

public static class DeleteStatusHelper
{
	internal static FileIndexItem? AddDeleteStatus(FileIndexItem dbItem, 
		FileIndexItem.ExifStatus exifStatus = FileIndexItem.ExifStatus.Deleted)
	{
		if ( dbItem?.Tags == null )
		{
			return null;
		}
		
		if ( dbItem.Tags.Contains(TrashKeyword.TrashKeywordString) )
		{
			dbItem.Status = exifStatus;
		}
		return dbItem;
	}
	
	internal static List<FileIndexItem> AddDeleteStatus(IEnumerable<FileIndexItem> dbItems,
		FileIndexItem.ExifStatus exifStatus =
			FileIndexItem.ExifStatus.Deleted)
	{
		return dbItems.Select(item => AddDeleteStatus(item, exifStatus)).Cast<FileIndexItem>().ToList();
	}
}

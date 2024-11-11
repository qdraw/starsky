using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Helpers;

public static class SortHelper
{
	/// <summary>
	///     Help order the list
	/// </summary>
	/// <param name="fileIndexItems">items</param>
	/// <param name="sort">way of sorting</param>
	/// <returns>sorted list</returns>
	public static IEnumerable<FileIndexItem> Helper(IEnumerable<FileIndexItem> fileIndexItems,
		SortType sort = SortType.FileName)
	{
		switch ( sort )
		{
			case SortType.ImageFormat:
				return [.. fileIndexItems.OrderBy(p => p.ImageFormat).ThenBy(p => p.FileName)];
			default:
				return fileIndexItems;
		}
	}
}

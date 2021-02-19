using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Helpers
{
	public static class SortHelper
	{
		public static IEnumerable<FileIndexItem> Helper(IEnumerable<FileIndexItem> fileIndexItems, SortType sort = SortType.FileName)
		{
			switch ( sort )
			{
				case SortType.ImageFormat:
					return fileIndexItems.OrderBy(p => p.ImageFormat).ToList();
				default:
					return fileIndexItems;
			}
		}
	}
}

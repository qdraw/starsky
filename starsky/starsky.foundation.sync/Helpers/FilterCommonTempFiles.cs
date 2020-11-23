using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.foundation.sync.Helpers
{
	public static class FilterCommonTempFiles
	{
		
		public static bool Filter(string subPath)
		{
			return subPath.ToLowerInvariant().EndsWith(".ds_store") || subPath.ToLowerInvariant().EndsWith(".tmp") ||
			       subPath.ToLowerInvariant().EndsWith("desktop.ini");
		}

		public static List<FileIndexItem> DefaultOperationNotSupported(string subPath)
		{
			return new List<FileIndexItem>
			{
				new FileIndexItem(subPath)
				{
					Status = FileIndexItem.ExifStatus.OperationNotSupported
				}
			};
		}
	}
}

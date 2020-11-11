using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncRemove
	{
		private readonly IQuery _query;

		public SyncRemove(IQuery query)
		{
			_query = query;
		}

		public async Task<List<FileIndexItem>> Remove(IEnumerable<string> subPaths)
		{
			var items = new List<FileIndexItem>();
			foreach ( var subPath in subPaths )
			{
				var item = await _query.GetObjectByFilePathAsync(subPath);
				item.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
				items.Add(item);
			}
			return items;
		}
	}
}

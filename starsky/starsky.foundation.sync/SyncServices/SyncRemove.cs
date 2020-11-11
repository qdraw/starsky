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
				if ( item != null )
				{
					await _query.RemoveItemAsync(item);
					item.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
					items.Add(item);
					continue;
				}
				items.Add(new FileIndexItem(subPath){Status = FileIndexItem.ExifStatus.NotFoundNotInIndex});
			}
			return items;
		}
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncRemove
	{
		private readonly IQuery _query;
		private readonly IConsole _console;

		public SyncRemove(IQuery query, IConsole console)
		{
			_query = query;
			_console = console;
		}

		public async Task<List<FileIndexItem>> Remove(IEnumerable<string> subPaths)
		{
			var items = new List<FileIndexItem>();
			foreach ( var subPath in subPaths )
			{
				var item = await _query.GetObjectByFilePathAsync(subPath);
				if ( item != null )
				{
					_console.WriteLine($"{subPath} is removed");
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

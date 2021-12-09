using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeIManualBackgroundSyncService : IManualBackgroundSyncService
	{
		private readonly Dictionary<string, FileIndexItem.ExifStatus> _items;

		public FakeIManualBackgroundSyncService(Dictionary<string, FileIndexItem.ExifStatus> items)
		{
			_items = items;
		}
		public Task<FileIndexItem.ExifStatus> ManualSync(string subPath,
			string operationId)
		{
			var value = _items.FirstOrDefault(p => Equals(p.Key, subPath)).Value;
			return Task.FromResult(value);
		}
	}
}

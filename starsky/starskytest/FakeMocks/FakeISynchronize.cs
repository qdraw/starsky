using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeISynchronize : ISynchronize
	{
		public Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true)
		{
			return Task.FromResult(new List<FileIndexItem>());
		}
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starskytest.FakeMocks;

public class FakeISyncAddThumbnailTable : ISyncAddThumbnailTable
{
	public Task<List<FileIndexItem>> SyncThumbnailTableAsync(List<FileIndexItem> fileIndexItems)
	{
		return Task.FromResult(fileIndexItems);
	}
}

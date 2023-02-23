using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeITrashConnectionService : ITrashConnectionService
{
	public Task<List<FileIndexItem>> ConnectionServiceAsync(List<FileIndexItem> moveToTrash, FileIndexItem.ExifStatus status)
	{
		return Task.FromResult(moveToTrash);
	}
}

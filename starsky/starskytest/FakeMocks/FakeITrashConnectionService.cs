using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeITrashConnectionService : ITrashConnectionService
{
	public Task<List<FileIndexItem>> ConnectionServiceAsync(List<FileIndexItem> moveToTrash, 
		bool isSystemTrash)
	{
		var status = isSystemTrash
			? FileIndexItem.ExifStatus.NotFoundSourceMissing
			: FileIndexItem.ExifStatus.Deleted;
		
		foreach ( var item in moveToTrash )
		{
			item.Status = status;
		}
		return Task.FromResult(moveToTrash);
	}
}

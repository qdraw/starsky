using System.Collections.Generic;
using starsky.foundation.sync.WatcherInterfaces;

namespace starskytest.FakeMocks;

public class FakeDiskWatcher : IDiskWatcher
{
	public List<string> AddedItems { get; set; } = new();

	public List<(string TenantId, string Path)> AddedByTenant { get; } = new();

	public void Watcher(string fullFilePath)
	{
		AddedItems.Add(fullFilePath);
	}

	public void Watcher(string fullFilePath, string tenantId)
	{
		AddedItems.Add(fullFilePath);
		AddedByTenant.Add(( tenantId, fullFilePath ));
	}
}

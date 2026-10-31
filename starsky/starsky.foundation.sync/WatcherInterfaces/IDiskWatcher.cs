namespace starsky.foundation.sync.WatcherInterfaces;

public interface IDiskWatcher
{
	/// <summary>
	///     Watch a folder of the default tenant
	/// </summary>
	void Watcher(string fullFilePath);

	/// <summary>
	///     Watch a folder of a tenant: the changes are synced as that tenant
	/// </summary>
	void Watcher(string fullFilePath, string tenantId);
}

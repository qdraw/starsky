using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.sync.Helpers;

public sealed class SyncIgnoreCheck(AppSettings appSettings, IConsole console)
{
	public bool Filter(string subPath)
	{
		var isSynced = appSettings.SyncIgnore.Exists(subPath.StartsWith);
		if ( isSynced && appSettings.IsVerbose() )
		{
			console.WriteLine($"sync ignored for: {subPath}");
		}

		return isSynced;
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.sync.SyncInterfaces
{
	/// <summary>
	/// New Style sync
	/// </summary>
	public interface ISynchronize
	{
		delegate Task SocketUpdateDelegate(List<FileIndexItem> changedFiles);

		Task<List<FileIndexItem>> Sync(string subPath,
			SocketUpdateDelegate? updateDelegate = null,
			DateTime? childDirectoriesAfter = null);
		Task<List<FileIndexItem>> Sync(List<string> subPaths,
			SocketUpdateDelegate? updateDelegate = null);
	}
}

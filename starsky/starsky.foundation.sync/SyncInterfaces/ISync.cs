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
		Task<List<FileIndexItem>> Sync(string subPath, bool recursive = true);
		Task<List<FileIndexItem>> Sync(List<string> subPaths, bool recursive = true);
	}
}

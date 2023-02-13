using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.sync.SyncInterfaces;

public interface ISyncAddThumbnailTable
{
	Task<List<FileIndexItem>> SyncThumbnailTableAsync(
		List<FileIndexItem> fileIndexItems);
}

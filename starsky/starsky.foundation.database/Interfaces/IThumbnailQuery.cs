using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;

namespace starsky.foundation.database.Interfaces;

public interface IThumbnailQuery
{
	Task<List<ThumbnailItem>?> AddThumbnailRangeAsync(ThumbnailSize size,
		IEnumerable<string> fileHashes, bool? setStatus = null);

	Task<List<ThumbnailItem>?> Get(string fileHash);
}

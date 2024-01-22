using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.geolookup.Interfaces
{
	public interface IGeoIndexGpx
	{
		Task<List<FileIndexItem>> LoopFolderAsync(
			List<FileIndexItem> metaFilesInDirectory);
	}
}

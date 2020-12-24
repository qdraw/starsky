using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.geolookup.Interfaces
{
	public interface IGeoIndexGpx
	{
		List<FileIndexItem>
			LoopFolder(List<FileIndexItem> metaFilesInDirectory);
	}
}

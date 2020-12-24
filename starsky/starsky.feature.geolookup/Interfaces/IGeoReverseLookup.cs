using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.geolookup.Interfaces
{
	public interface IGeoReverseLookup
	{
		List<FileIndexItem> LoopFolderLookup(
			List<FileIndexItem> metaFilesInDirectory,
			bool overwriteLocationNames);
	}
}

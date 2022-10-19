using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.geolookup.Interfaces
{
	public interface IGeoReverseLookup
	{
		Task<List<FileIndexItem>> LoopFolderLookup(
			List<FileIndexItem> metaFilesInDirectory,
			bool overwriteLocationNames);
	}
}

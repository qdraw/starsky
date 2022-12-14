using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.geolookup.Models;
using starsky.foundation.database.Models;

namespace starsky.feature.geolookup.Interfaces
{
	public interface IGeoReverseLookup
	{
		Task<List<FileIndexItem>> LoopFolderLookup(
			List<FileIndexItem> metaFilesInDirectory,
			bool overwriteLocationNames);

		Task<GeoLocationModel> GetLocation(double latitude, double longitude);
	}
}

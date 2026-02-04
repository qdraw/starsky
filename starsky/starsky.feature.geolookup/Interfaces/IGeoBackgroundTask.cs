using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.geolookup.Interfaces
{
	public interface IGeoBackgroundTask
	{
		Task<List<FileIndexItem>> GeoBackgroundTaskAsync(
			string f = "/",
			bool index = true,
			bool overwriteLocationNames = false);
	}
}

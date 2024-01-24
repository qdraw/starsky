using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.database.Models;

namespace starskytest.FakeMocks;

public class FakeIGeoBackgroundTask : IGeoBackgroundTask
{
	public int Count { get; set; } = 0;
	
	public Task<List<FileIndexItem>> GeoBackgroundTaskAsync(string f = "/", bool index = true,
		bool overwriteLocationNames = false)
	{
		Count++;
		return Task.FromResult(new List<FileIndexItem>{new FileIndexItem(f)
		{
			Longitude = 52,
			Latitude = 5
		}});
	}
}

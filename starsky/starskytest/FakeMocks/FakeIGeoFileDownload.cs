using System.IO;
using System.Threading.Tasks;
using starsky.foundation.geo.GeoDownload.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIGeoFileDownload : IGeoFileDownload
{
	public int Count { get; set; }

	public Task DownloadAsync()
	{
		if ( Count == int.MaxValue )
		{
			throw new FileNotFoundException("Not allowed to write to disk");
		}

		Count++;
		return Task.CompletedTask;
	}
}

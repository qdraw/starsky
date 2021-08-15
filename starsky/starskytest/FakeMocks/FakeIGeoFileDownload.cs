using System.IO;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIGeoFileDownload : IGeoFileDownload
	{
		public int Count { get; set; } = 0;
		public Task Download()
		{
			if ( Count == int.MaxValue  )
			{
				throw new FileNotFoundException("Not allowed to write to disk");
			}
			Count++;
			return Task.CompletedTask;
		}
	}
}

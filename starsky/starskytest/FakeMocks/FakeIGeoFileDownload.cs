using starsky.feature.geolookup.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIGeoFileDownload : IGeoFileDownload
	{
		public int Count { get; set; } = 0;
		public void Download()
		{
			Count++;
		}
	}
}

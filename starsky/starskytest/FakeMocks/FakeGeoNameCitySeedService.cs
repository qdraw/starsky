using System.Threading.Tasks;
using starsky.foundation.database.GeoNamesCities.Interfaces;

namespace starskytest.FakeMocks;

public sealed class FakeGeoNameCitySeedService : IGeoNameCitySeedService
{
	public bool SeedResult = true;

	public Task<bool> Seed()
	{
		return Task.FromResult(SeedResult);
	}
}

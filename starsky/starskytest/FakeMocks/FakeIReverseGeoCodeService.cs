using System.Threading.Tasks;
using starsky.foundation.geo.ReverseGeoCode.Interface;
using starsky.foundation.geo.ReverseGeoCode.Model;

namespace starskytest.FakeMocks;

public class FakeIReverseGeoCodeService : IReverseGeoCodeService
{
	public Task<GeoLocationModel> GetLocation(double latitude, double longitude)
	{
		return Task.FromResult(new GeoLocationModel());
	}
}

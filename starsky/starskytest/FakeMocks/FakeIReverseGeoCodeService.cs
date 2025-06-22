using System.Threading.Tasks;
using starsky.foundation.geo.ReverseGeoCode.Interface;
using starsky.foundation.geo.ReverseGeoCode.Model;

namespace starskytest.FakeMocks;

public class FakeIReverseGeoCodeService(GeoLocationModel? geoLocationModel = null) : IReverseGeoCodeService
{
	public Task<GeoLocationModel> GetLocation(double latitude, double longitude)
	{
		var status = new GeoLocationModel();
		if ( geoLocationModel != null )
		{
			status = geoLocationModel;
		}

		if ( !string.IsNullOrEmpty(status.LocationState) &&
		     !string.IsNullOrEmpty(status.LocationCountry) &&
		     !string.IsNullOrEmpty(status.LocationCountryCode) &&
		     !string.IsNullOrEmpty(status.LocationCity) )
		{
			status.ErrorReason = "Success";
			status.IsSuccess = true;
			return Task.FromResult(status);
		}

		status.ErrorReason = "No location found";
		status.IsSuccess = false;
		return Task.FromResult(status);
	}
}

using starsky.foundation.geo.ReverseGeoCode.Model;

namespace starsky.foundation.geo.ReverseGeoCode.Interface;

public interface IReverseGeoCodeService
{
	Task<GeoLocationModel> GetLocation(double latitude, double longitude);
}

using System.Threading.Tasks;
using starsky.feature.geolookup.Models;

namespace starsky.feature.geolookup.Interfaces;

public interface INominatimProxyService
{
	/// <summary>
	/// This API does not allow BATCH requests, so only one request at a time.
	/// If you want to do multiple requests, please wait at least 1 second between requests.
	/// See https://nominatim.org/release-docs/develop/api/Reverse/ for more details.
	/// </summary>
	/// <param name="latitude">Decimal Degree latitude</param>
	/// <param name="longitude">Decimal Degree longitude</param>
	/// <returns>object with information</returns>
	Task<NominatimReverseResult> ReverseAsync(double latitude, double longitude);
}

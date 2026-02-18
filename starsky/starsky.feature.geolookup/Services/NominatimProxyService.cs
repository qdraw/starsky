using System.Text.Json;
using System.Threading.Tasks;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;

namespace starsky.feature.geolookup.Services;

[Service(typeof(INominatimProxyService), InjectionLifetime = InjectionLifetime.Scoped)]
public class NominatimProxyService(IHttpClientHelper httpClientHelper) : INominatimProxyService
{
	private const string HttpsPrefix = "https://";
	private const string NominatimBaseUrl = "nominatim.openstreetmap.org/reverse";

	/// <summary>
	/// This API does not allow BATCH requests, so only one request at a time.
	/// If you want to do multiple requests, please wait at least 1 second between requests.
	/// See https://nominatim.org/release-docs/develop/api/Reverse/ for more details.
	/// </summary>
	/// <param name="latitude">Decimal Degree latitude</param>
	/// <param name="longitude">Decimal Degree longitude</param>
	/// <returns>object with information</returns>
	public async Task<NominatimReverseResult> ReverseAsync(double latitude, double longitude)
	{
		if ( !ValidateLocation.ValidateLatitudeLongitude(latitude, longitude) )
		{
			return new NominatimReverseResult { Error = "Non-valid location" };
		}

		const string errorMessage = "Failed to parse Nominatim response";
		var url = $"{HttpsPrefix}{NominatimBaseUrl}?format=json&lat={latitude}" +
		          $"&lon={longitude}&addressdetails=1";
		var response = await httpClientHelper.ReadString(url);
		if ( !response.Key || string.IsNullOrWhiteSpace(response.Value) )
		{
			return new NominatimReverseResult { Error = errorMessage };
		}

		try
		{
			return JsonSerializer.Deserialize<NominatimReverseResult>(response.Value) ??
			       new NominatimReverseResult { Error = errorMessage };
		}
		catch
		{
			return new NominatimReverseResult { Error = errorMessage };
		}
	}
}

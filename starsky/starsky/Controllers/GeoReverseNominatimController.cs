using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.geolookup.Interfaces;

namespace starsky.Controllers;

[Authorize]
public class GeoReverseNominatimController(INominatimProxyService nominatimProxyService)
	: Controller
{
	/// <summary>
	///     Reverse geo lookup with OpenStreetMap Nominatim
	///		This API does not allow BATCH requests, so only one request at a time.
	///		If you want to do multiple requests, please wait at least 1 second between requests.
	///		See https://nominatim.org/release-docs/develop/api/Reverse/ for more details.
	/// </summary>
	/// <param name="lat">Latitude coordinate in Decimal Degree (DD)</param>
	/// <param name="lon">Longitude coordinate in DD</param>
	/// <returns>reverse geo code data</returns>
	/// <response code="200">Data with object</response>
	[HttpGet("/api/geo-reverse-nominatim")]
	[Produces("application/json")]
	public async Task<IActionResult> GeoReverseLookup(double lat, double lon)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model is not valid");
		}

		var result = await nominatimProxyService.ReverseAsync(lat, lon);
		return Ok(result);
	}
}

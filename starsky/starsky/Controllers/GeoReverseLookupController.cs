using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.geolookup.Interfaces;

namespace starsky.Controllers;

[AllowAnonymous]
public sealed class GeoReverseLookupController : Controller
{
	private readonly IGeoReverseLookup _geoReverseLookup;

	public GeoReverseLookupController(IGeoReverseLookup geoReverseLookup)
	{
		_geoReverseLookup = geoReverseLookup;
	}
	
	/// <summary>
	/// Reverse geo lookup
	/// </summary>
	/// <param name="latitude">Latitude coordinate in Decimal Degree (DD)</param>
	/// <param name="longitude">Longitude coordinate in DD</param>
	/// <returns>reverse geo code data</returns>
	/// <response code="200">Data with object</response>
	[HttpGet("/api/geo-reverse-lookup")]
	[AllowAnonymous]
	[ResponseCache(Duration = 7257600, Location = ResponseCacheLocation.Client)]
	[Produces("application/json")]
	public async Task<IActionResult> GeoReverseLookup(double latitude, double longitude)
	{
		var result = await _geoReverseLookup.GetLocation(latitude, longitude);
		return Ok(result);
	}
}

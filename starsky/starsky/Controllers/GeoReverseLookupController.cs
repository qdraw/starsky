using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.geolookup.Interfaces;
using starsky.foundation.geo.ReverseGeoCode.Interface;

namespace starsky.Controllers;

[AllowAnonymous]
public sealed class GeoReverseLookupController : Controller
{
	private readonly IReverseGeoCodeService _reverseGeoCodeService;

	public GeoReverseLookupController(IReverseGeoCodeService reverseGeoCodeService)
	{
		_reverseGeoCodeService = reverseGeoCodeService;
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
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model is not valid");
		}
		
		var result = await _reverseGeoCodeService.GetLocation(latitude, longitude);
		return Ok(result);
	}
}

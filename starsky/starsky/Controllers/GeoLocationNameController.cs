using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.geo.LocationNameSearch.Interfaces;

namespace starsky.Controllers;

[Authorize]
public class GeoLocationNameController(ILocationNameService locationNameService) : Controller
{
	/// <summary>
	///     Reverse geo lookup
	/// </summary>
	/// <param name="latitude">Latitude coordinate in Decimal Degree (DD)</param>
	/// <param name="longitude">Longitude coordinate in DD</param>
	/// <returns>reverse geo code data</returns>
	/// <response code="200">Data with object</response>
	[HttpGet("/api/geo-location-name/city")]
	[Produces("application/json")]
	public async Task<IActionResult> GeoReverseLookup(DateOnly date, string city)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model is not valid");
		}

		var result = await locationNameService.SearchCity(city);
		return Ok(result);
	}

	/// <summary>
	///     Reverse geo lookup
	/// </summary>
	/// <param name="latitude">Latitude coordinate in Decimal Degree (DD)</param>
	/// <param name="longitude">Longitude coordinate in DD</param>
	/// <returns>reverse geo code data</returns>
	/// <response code="200">Data with object</response>
	[HttpGet("/api/geo-location-name/city-timezone")]
	[Produces("application/json")]
	public async Task<IActionResult> SearchCityTimezone(string dateTime, string city)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model is not valid");
		}

		var result = await locationNameService.SearchCityTimezone(dateTime, city);
		return Ok(result);
	}
}

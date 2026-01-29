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
	///     Search for a city by name
	/// </summary>
	/// <param name="city">City name to search for</param>
	/// <returns>City search results</returns>
	/// <response code="200">Data with object</response>
	[HttpGet("/api/geo-location-name/city")]
	[Produces("application/json")]
	public async Task<IActionResult> GeoReverseLookup(string city)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model is not valid");
		}

		var result = await locationNameService.SearchCity(city);
		return Ok(result);
	}

	/// <summary>
	///     Search for a city's timezone for a given date/time
	/// </summary>
	/// <param name="dateTime">Date/time string (ISO 8601 or similar)</param>
	/// <param name="city">City name to search for</param>
	/// <returns>Timezone information for the city at the given date/time</returns>
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

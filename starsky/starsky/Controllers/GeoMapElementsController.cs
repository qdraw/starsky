using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.geolookup.Interfaces;

namespace starsky.Controllers;

[Authorize]
public class GeoMapElementsController(IOsmMapElementsService osmMapElementsService) : Controller
{
	[HttpGet("/api/geo-map-elements")]
	[Produces("application/json")]
	public async Task<IActionResult> GeoMapElements(double lat, double lon)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model is not valid");
		}

		var result = await osmMapElementsService.LookupAsync(lat, lon);
		return Ok(result);
	}
}
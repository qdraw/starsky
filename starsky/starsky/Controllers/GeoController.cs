using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace starsky.Controllers
{
	[Authorize]
	public class GeoController : Controller
	{
		/// <summary>
		/// Reverse lookup for Geo Information and/or add Geo location based on a GPX file within the same directory
		/// </summary>
		/// <param name="f">subPath</param>
		/// <param name="colorClass">filter on colorClass (use int)</param>
		/// <param name="json">to not show as webpage</param>
		/// <param name="collections">to combine files with the same name before the extension</param>
		/// <param name="hidedelete">ignore deleted files</param>
		/// <returns></returns>
		/// <response code="200">returns a list of items from the database</response>
		/// <response code="404">subpath not found in the database</response>
		/// <response code="401">User unauthorized</response>
		[HttpGet("/api/geo/sync")]
		[Produces("application/json")]
		[ProducesResponseType(404)]
		public IActionResult Index(
			string f = "/"
		)
		{
			return Json("");
		}
	}
}

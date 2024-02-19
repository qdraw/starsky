using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.desktop.Interfaces;
using starsky.feature.desktop.Models;

namespace starsky.Controllers;

[Authorize]
public class DesktopEditorController : Controller
{
	private readonly IOpenEditorDesktopService _openEditorDesktopService;

	public DesktopEditorController(IOpenEditorDesktopService openEditorDesktopService)
	{
		_openEditorDesktopService = openEditorDesktopService;
	}

	/// <summary>
	/// Open a file in the default editor or a specific editor on the desktop
	/// </summary>
	/// <param name="f">single or multiple subPaths</param>
	/// <param name="collections">to combine files with the same name before the extension</param>
	/// <returns></returns>
	/// <response code="200">returns a list of items from the database</response>
	/// <response code="404">subPath not found in the database</response>
	/// <response code="401">User unauthorized</response>
	[HttpGet("/api/desktop-editor/open")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(List<PathImageFormatExistsAppPathModel>), 200)]
	[ProducesResponseType(typeof(List<PathImageFormatExistsAppPathModel>), 204)]
	[ProducesResponseType(typeof(string), 400)]
	[ProducesResponseType(401)]
	public async Task<IActionResult> OpenAsync(
		string f = "",
		bool collections = true)
	{
		var (success, status, list) =
			await _openEditorDesktopService.OpenAsync(f, collections);

		switch ( success )
		{
			case null:
				return BadRequest(status);
			case false:
				HttpContext.Response.StatusCode = 204;
				break;
		}

		return Json(list);
	}
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.platform.Helpers;
using starsky.project.web.Helpers;

namespace starsky.Controllers;

[Authorize]
public sealed class AllowedTypesController : Controller
{
	/// <summary>
	///     A (string) list of allowed MIME-types ExtensionSyncSupportedList
	/// </summary>
	/// <returns>Json list</returns>
	/// <response code="200">list</response>
	/// <response code="401">please login first</response>
	[HttpGet("/api/allowed-types/mimetype/sync")]
	[ProducesResponseType(typeof(HashSet<string>), 200)]
	[Produces("application/json")]
	public IActionResult AllowedTypesMimetypeSync()
	{
		return MimeTypesJson(ExtensionRolesHelper.ExtensionSyncSupportedList);
	}


	/// <summary>
	///     A (string) list of allowed ExtensionThumbSupportedList MimeTypes
	/// </summary>
	/// <returns>Json list</returns>
	/// <response code="200">list</response>
	/// <response code="401">please login first</response>
	[HttpGet("/api/allowed-types/mimetype/thumb")]
	[ProducesResponseType(typeof(HashSet<string>), 200)]
	[Produces("application/json")]
	public IActionResult AllowedTypesMimetypeSyncThumb()
	{
		return MimeTypesJson(ExtensionRolesHelper.ExtensionSkiaSharpThumbnailSupportedList);
	}

	/// <summary>
	///     Check if IsExtensionThumbnailSupported
	/// </summary>
	/// <returns>Json list</returns>
	/// <param name="f">the name with extension and no parent path</param>
	/// <response code="200">is supported</response>
	/// <response code="415">the extenstion from the filename is not supported to generate thumbnails</response>
	/// <response code="401">please log in first</response>
	[HttpGet("/api/allowed-types/thumb")]
	[ProducesResponseType(typeof(bool), 200)]
	[ProducesResponseType(typeof(bool), 415)]
	[Produces("application/json")]
	public IActionResult AllowedTypesThumb(string f)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("ModelState is not valid");
		}

		var result = ExtensionRolesHelper.IsExtensionSkiaSharpThumbnailSupported(f);
		if ( !result )
		{
			Response.StatusCode = 415;
		}

		return Json(result);
	}

	/// <summary>
	///     Convert a list of extensions to a distinct list of mime types
	/// </summary>
	private IActionResult MimeTypesJson(List<string> extensions)
	{
		var mimeTypes = new HashSet<string>();
		foreach ( var extension in extensions )
		{
			mimeTypes.Add(MimeHelper.GetMimeType(extension));
		}

		return Json(mimeTypes);
	}
}

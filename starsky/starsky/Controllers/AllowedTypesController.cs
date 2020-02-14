using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starskycore.Helpers;

namespace starsky.Controllers
{
	[Authorize]
	public class AllowedTypesController : Controller
	{
		
		/// <summary>
		/// A (string) list of allowed MIME-types ExtensionSyncSupportedList
		/// </summary>
		/// <returns>Json list</returns>
		/// <response code="200">list</response>
		/// <response code="401">please login first</response>
		[HttpGet("/api/allowed-types/mimetype/sync")]
		[ProducesResponseType(typeof(HashSet<string>),200)]
		public IActionResult AllowedTypesMimetypeSync()
		{
			var mimeTypes = ExtensionRolesHelper.ExtensionSyncSupportedList.Select(MimeHelper.GetMimeType).ToHashSet();
			return Json(mimeTypes);
		} 
		
		
		/// <summary>
		/// A (string) list of allowed ExtensionThumbSupportedList MimeTypes
		/// </summary>
		/// <returns>Json list</returns>
		/// <response code="200">list</response>
		/// <response code="401">please login first</response>
		[HttpGet("/api/allowed-types/mimetype/thumb")]
		[ProducesResponseType(typeof(HashSet<string>),200)]
		public IActionResult AllowedTypesMimetypeSyncThumb()
		{
			var mimeTypes = ExtensionRolesHelper.ExtensionThumbSupportedList.Select(MimeHelper.GetMimeType).ToHashSet();
			return Json(mimeTypes);
		} 
		
		/// <summary>
		/// Check if IsExtensionThumbnailSupported
		/// </summary>
		/// <returns>Json list</returns>
		/// <param name="f">the name with extension and no parent path</param>
		/// <response code="200">is supported</response>
		/// <response code="415">the extenstion from the filename is not supported to generate thumbnails</response>
		/// <response code="401">please login first</response>
		[HttpGet("/api/allowed-types/thumb")]
		[ProducesResponseType(typeof(bool),200)]
		[ProducesResponseType(typeof(bool),415)]
		public IActionResult AllowedTypesThumb(string f)
		{
			var result = ExtensionRolesHelper.IsExtensionThumbnailSupported(f);
			if ( !result ) Response.StatusCode = 415;
			return Json(result);
		} 
	}
}

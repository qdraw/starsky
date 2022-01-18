using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.Controllers
{
	public class TimeZoneController : Controller
	{
		private readonly AppSettings _appSettings;

		public TimeZoneController(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		/// <summary>
		/// Upload to specific folder (does not check if already has been imported)
		/// Use the header 'to' to determine the location to where to upload
		/// Add header 'filename' when uploading direct without form
		/// (ActionResult UploadToFolder)
		/// </summary>
		/// <response code="200">done</response>
		/// <returns>the ImportIndexItem of the imported files </returns>
		[HttpGet("/api/timezone/diff")]
		[ProducesResponseType(typeof(string), 200)]
		[Produces("application/json")]
		public IActionResult TimezoneDiff()
		{
			if ( _appSettings?.CameraTimeZoneInfo == null )
			{
				return Json(string.Empty);
			}


			var timeZoneInfo = _appSettings.CameraTimeZoneInfo;
			TimeSpan offset = timeZoneInfo.GetUtcOffset(DateTime.UtcNow);
			
			return Json(offset);
		}

		
	}
}

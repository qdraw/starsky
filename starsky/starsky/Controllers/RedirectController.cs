using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starskycore.Models;

namespace starsky.Controllers
{
	[Authorize]
	public class RedirectController : Controller
	{
		private readonly AppSettings _appsettings;

		public RedirectController(AppSettings appsettings = null)
		{
			_appsettings = appsettings;
		}
			    
		/// <summary>
		/// Redirect or view path to relative paths using the structure-config (see /api/env)
		/// </summary>
		/// <param name="value">how many days ago</param>
		/// <param name="json">get results</param>
		/// <returns>redirect or path to relative folder</returns>
		/// <response code="200">(if json is true) the subpath of the folder</response>
		/// <response code="301">(if json is false) redirect to folder</response>
		[HttpGet("/redirect/SubpathRelative")]
		[ProducesResponseType(200)] // value
		[ProducesResponseType(301)] // redirect
		public IActionResult SubpathRelative(int value, bool json = true)
		{
			if(value >= 1) value = value * -1; // always in the past
			// Fallback for dates older than 24-11-1854 to avoid a exception.
			if ( value < -60000 ) value = 0;
			
			var importmodel = new ImportIndexItem(_appsettings)
			{
				DateTime = DateTime.Today.AddDays(value), 
				SourceFullFilePath = "notimplemented.jpg"
			};
			// expect something like this: /2018/09/2018_09_02/
			var subpath = importmodel.ParseSubfolders(false);
			if(json) return Json(subpath);
			return RedirectToAction("Index", "Home", new { f = subpath });
		}

	}
}

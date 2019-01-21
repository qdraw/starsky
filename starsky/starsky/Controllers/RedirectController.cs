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
		/// <returns></returns>
		[HttpGet("/redirect/SubpathRelative")]
		public IActionResult SubpathRelative(int value, bool json = false)
		{
			if(value >= 1) value = value * -1; //always in the past

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

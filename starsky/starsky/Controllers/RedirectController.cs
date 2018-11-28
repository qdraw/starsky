using System;
using Microsoft.AspNetCore.Mvc;
using starsky.Models;

namespace starsky.Controllers
{
	public class RedirectController : Controller
	{
		private readonly AppSettings _appsettings;

		public RedirectController(AppSettings appsettings = null)
		{
			_appsettings = appsettings;
		}
			    
		public IActionResult SubpathRelative(int value, bool json = false)
		{
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

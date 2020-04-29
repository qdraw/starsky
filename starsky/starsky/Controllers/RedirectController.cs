using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers
{
	[Authorize]
	public class RedirectController : Controller
	{
		private readonly StructureService _structureService;

		public RedirectController(ISelectorStorage selectorStorage, AppSettings appSettings)
		{
			var storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_structureService = new StructureService(storage, appSettings.Structure);
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
		[Produces("application/json")]
		public IActionResult SubPathRelative(int value, bool json = true)
		{
			if(value >= 1) value = value * -1; // always in the past
			// Fallback for dates older than 24-11-1854 to avoid a exception.
			if ( value < -60000 ) value = 0;
			
			// expect something like this: /2018/09/2018_09_02/
			var subPath = _structureService.ParseSubfolders(DateTime.Today.AddDays(value));
			if(json) return Json(subPath);
			return RedirectToAction("Index", "Home", new { f = subPath });
		}

	}
}

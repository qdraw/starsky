using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskycore.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class AppSettingsController : Controller
	{
		private readonly AppSettings _appSettings;

		public AppSettingsController(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
		
		/// <summary>
		/// Show the runtime settings (dont allow AllowAnonymous)
		/// </summary>
		/// <returns>config data, except connection strings</returns>
		/// <response code="200">returns the runtime settings of Starsky</response>
		[HttpHead("/api/env")]
		[HttpGet("/api/env")]
		[IgnoreAntiforgeryToken]
		[Produces("application/json")]
		[ProducesResponseType(typeof(AppSettings),200)]
		[ProducesResponseType(typeof(AppSettings),401)]
		public IActionResult Env()
		{
			var appSettings = _appSettings.CloneToDisplay();
			return Json(appSettings);
		}
		
		/// <summary>
		/// Show the runtime settings (dont allow AllowAnonymous)
		/// </summary>
		/// <returns>config data, except connection strings</returns>
		/// <response code="200">returns the runtime settings of Starsky</response>
		[HttpPost("/api/env")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(AppSettings),200)]
		[ProducesResponseType(typeof(AppSettings),401)]
		[Permission(UserManager.AppPermissions.AppSettingsWrite)]
		public IActionResult UpdateAppSettings(AppSettings toAppSettings)
		{
			new AppSettingsEditor(_appSettings).Update(toAppSettings);
			return Env();
		}
	}
}

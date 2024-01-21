using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.feature.settings.Interfaces;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers
{
	[Authorize]
	public sealed class AppSettingsController : Controller
	{
		private readonly AppSettings _appSettings;
		private readonly IUpdateAppSettingsByPath _updateAppSettingsByPath;

		public AppSettingsController(AppSettings appSettings, IUpdateAppSettingsByPath updateAppSettingsByPath)
		{
			_appSettings = appSettings;
			_updateAppSettingsByPath = updateAppSettingsByPath;
		}
		
		/// <summary>
		/// Show the runtime settings (dont allow AllowAnonymous)
		/// </summary>
		/// <returns>config data, except connection strings</returns>
		/// <response code="200">returns the runtime settings of Starsky</response>
		[HttpHead("/api/env")]
		[HttpGet("/api/env")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(AppSettings),200)]
		[ProducesResponseType(typeof(AppSettings),401)]
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse",Justification = "Request in tests")]
		public IActionResult Env()
		{
			var appSettings = _appSettings.CloneToDisplay();
			if ( Request != null && Request.Headers.Any(p => p.Key == "x-force-html") )
			{
				Response.Headers.ContentType = "text/html; charset=utf-8";
			}
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
		public async Task<IActionResult> UpdateAppSettings(AppSettingsTransferObject appSettingTransferObject  )
		{
			var result = await _updateAppSettingsByPath.UpdateAppSettingsAsync(
				appSettingTransferObject);
			
			if ( !result.IsError )
			{
				return Env();
			}
			
			Response.StatusCode = result.StatusCode;
			return Content(result.Message);
		}
	}
}

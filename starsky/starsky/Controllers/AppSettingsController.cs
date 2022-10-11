using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
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
	public class AppSettingsController : Controller
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _hostStorage;

		public AppSettingsController(AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_appSettings = appSettings;
			_hostStorage = selectorStorage.Get( SelectorStorage.StorageServices.HostFilesystem);
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
			if ( !string.IsNullOrEmpty(appSettingTransferObject.StorageFolder))
			{
				if ( !_appSettings.StorageFolderAllowEdit )
				{
					Response.StatusCode = 403;
					return Content("There is an Environment variable set so you can't update it here");
				}
				if (!_hostStorage.ExistFolder(appSettingTransferObject.StorageFolder) )
				{
					return NotFound("Location on disk not found");
				}
			}
			
			// To update current session
			AppSettingsCompareHelper.Compare(_appSettings, appSettingTransferObject);
			
			// should not forget app: prefix
			var jsonOutput = JsonSerializer.Serialize(new { app = appSettingTransferObject }, new JsonSerializerOptions
			{
				WriteIndented = true, 
				Converters =
				{
					new JsonBoolQuotedConverter(),
				},
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			});

			await _hostStorage.WriteStreamAsync(
				PlainTextFileHelper.StringToStream(jsonOutput),
				_appSettings.AppSettingsPath);
			
			return Env();
		}
	}
}

using System.Text.Json;
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
		public async Task<IActionResult> UpdateAppSettings(AppSettingsTransferObject appSettingTransferObject  )
		{
			AppSettingsCompareHelper.Compare(_appSettings, appSettingTransferObject);
			
			// should not forget app: prefix
			var json = JsonSerializer.Serialize(new { app = _appSettings }, new JsonSerializerOptions
			{
				WriteIndented = true, 
				Converters =
				{
					new JsonBoolQuotedConverter(),
				}
			});
			
			var jsonOutput = json.Replace(new AppSettings().BaseDirectoryProject, "{AssemblyDirectory}");

			await _hostStorage.WriteStreamAsync(
				new PlainTextFileHelper().StringToStream(jsonOutput),
				_appSettings.AppSettingsPath);
			
			return Env();
		}
	}
}

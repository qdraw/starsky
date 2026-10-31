using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using starsky.Attributes;
using starsky.feature.settings.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;

namespace starsky.Controllers;

[Authorize]
public sealed class AppSettingsController : Controller
{
	private readonly AppSettings _appSettings;
	private readonly IUpdateAppSettingsByPath _updateAppSettingsByPath;

	public AppSettingsController(AppSettings appSettings,
		IUpdateAppSettingsByPath updateAppSettingsByPath)
	{
		_appSettings = appSettings;
		_updateAppSettingsByPath = updateAppSettingsByPath;
	}

	/// <summary>
	///     Show the runtime settings (dont allow AllowAnonymous)
	/// </summary>
	/// <returns>config data, except connection strings</returns>
	/// <response code="200">returns the runtime settings of Starsky</response>
	[HttpHead("/api/env")]
	[HttpGet("/api/env")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(AppSettings), 200)]
	[ProducesResponseType(typeof(AppSettings), 401)]
	[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse",
		Justification = "Request in tests")]
	public IActionResult Env()
	{
		var appSettings = _appSettings.CloneToDisplay();

		// the host folders are those of the platform (and of the default tenant): a member of one
		// tenant does not need them to know where the files of another tenant are
		if ( IsMoreThanOneTenantAndNotPlatformAdmin() )
		{
			const string hidden = "hidden";
			appSettings.StorageFolder = hidden;
			appSettings.ThumbnailTempFolder = hidden;
			appSettings.TempFolder = hidden;
			appSettings.DependenciesFolder = hidden;
			appSettings.TenantsRoot = hidden;
			appSettings.AppSettingsPath = hidden;
			appSettings.AppSettingsLocalPath = hidden;
			appSettings.StorageFolderMappings = new Dictionary<string, string>();
		}

		// For end-to-end testing
		if ( Request != null! && Request.Headers.Any(p => p.Key == "x-force-html") )
		{
			Response.Headers.ContentType = "text/html; charset=utf-8";
		}

		return Json(appSettings);
	}

	/// <summary>
	///     Show the runtime settings (dont allow AllowAnonymous)
	/// </summary>
	/// <returns>config data, except connection strings</returns>
	/// <response code="200">returns the runtime settings of Starsky</response>
	[HttpPost("/api/env")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(AppSettings), 200)]
	[ProducesResponseType(typeof(AppSettings), 401)]
	[Permission(UserManager.AppPermissions.AppSettingsWrite)]
	public async Task<IActionResult> UpdateAppSettings(
		AppSettingsTransferObject appSettingTransferObject)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("ModelState is not valid");
		}

		// these settings (and the file they are written to) are shared by all tenants
		if ( IsMoreThanOneTenantAndNotPlatformAdmin() )
		{
			return StatusCode(403, "Process wide settings: platform administrator only");
		}

		var result = await _updateAppSettingsByPath.UpdateAppSettingsAsync(
			appSettingTransferObject);

		if ( !result.IsError )
		{
			return Env();
		}

		Response.StatusCode = result.StatusCode;
		return Content(result.Message);
	}

	/// <summary>
	///     With one tenant everything is as before. With more, the process wide settings are for
	///     the platform administrator
	/// </summary>
	private bool IsMoreThanOneTenantAndNotPlatformAdmin()
	{
		var registry = HttpContext?.RequestServices?.GetService<ITenantRegistry>();
		return registry?.ActiveTenantIds.Count > 1 &&
		       !User.Claims.Any(c => c.Type == TenantMembershipMiddleware.PlatformAdminClaim);
	}
}

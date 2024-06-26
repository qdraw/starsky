using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.desktop.Interfaces;
using starsky.feature.trash.Interfaces;
using starsky.foundation.platform.Models;
using starsky.project.web.ViewModels;

namespace starsky.Controllers;

public class AppSettingsFeaturesController : Controller
{
	private readonly IMoveToTrashService _moveToTrashService;
	private readonly AppSettings _appSettings;
	private readonly IOpenEditorDesktopService _openEditorDesktopService;

	public AppSettingsFeaturesController(IMoveToTrashService moveToTrashService,
		IOpenEditorDesktopService openEditorDesktopService,
		AppSettings appSettings)
	{
		_moveToTrashService = moveToTrashService;
		_openEditorDesktopService = openEditorDesktopService;
		_appSettings = appSettings;
	}

	/// <summary>
	/// Show features that used in the frontend app / menu
	/// </summary>
	/// <returns>EnvFeatures that are used</returns>
	/// <response code="200">returns the runtime settings of Starsky</response>
	[HttpGet("/api/env/features")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(AppSettings), 200)]
	[ProducesResponseType(typeof(AppSettings), 401)]
	[AllowAnonymous]
#if !DEBUG
	// 86400 is 1 week
	[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client)]
#endif
	public IActionResult FeaturesView()
	{
		var shortAppSettings = new EnvFeaturesViewModel
		{
			SystemTrashEnabled = _moveToTrashService.IsEnabled(),
			UseLocalDesktop = _appSettings.UseLocalDesktop == true,
			OpenEditorEnabled = _openEditorDesktopService.IsEnabled()
		};

		return Json(shortAppSettings);
	}
}

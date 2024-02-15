using starsky.feature.desktop.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.native.OpenApplicationNative.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.feature.desktop.Service;

[Service(typeof(IOpenEditorDesktopService), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenEditorDesktopService : IOpenEditorDesktopService
{
	private readonly AppSettings _appSettings;
	private readonly IOpenApplicationNativeService _openApplicationNativeService;

	public OpenEditorDesktopService(AppSettings appSettings,
		IOpenApplicationNativeService openApplicationNativeService)
	{
		_appSettings = appSettings;
		_openApplicationNativeService = openApplicationNativeService;
	}

	public void Test()
	{
		_appSettings.UseLocalDesktop
	}
}

using starsky.foundation.injection;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.native.OpenApplicationNative.Interfaces;

namespace starsky.foundation.native.OpenApplicationNative;

[Service(typeof(IOpenApplicationNativeService), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenApplicationNativeService : IOpenApplicationNativeService
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="fullPaths">full path style</param>
	/// <param name="applicationUrl"> applicationUrl</param>
	/// <returns>operation succeed (NOT if file is gone)</returns>
	public bool? OpenApplicationAtUrl(List<string> fullPaths, string applicationUrl)
	{
		var currentPlatform = OperatingSystemHelper.GetPlatform();
		var macOsOpenResult = MacOsOpenUrl.OpenApplicationAtUrl(fullPaths,
			applicationUrl, currentPlatform);

		var windowsOpenResult = WindowsOpenDesktopApp.OpenApplicationAtUrl(fullPaths,
			applicationUrl, currentPlatform);

		return macOsOpenResult ?? windowsOpenResult;
	}
}

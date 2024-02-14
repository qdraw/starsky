using starsky.foundation.injection;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.native.OpenApplicationNative.Interfaces;

namespace starsky.foundation.native.OpenApplicationNative;

[Service(typeof(IOpenApplicationNativeService), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenApplicationNativeService : IOpenApplicationNativeService
{
	/// <summary>
	/// Open file with specified application
	/// </summary>
	/// <param name="fullPaths">full path style</param>
	/// <param name="applicationUrl"> applicationUrl</param>
	/// <returns>true is operation succeed, false failed | null is platform unsupported</returns>
	public bool? OpenApplicationAtUrl(List<string> fullPaths, string applicationUrl)
	{
		var currentPlatform = OperatingSystemHelper.GetPlatform();
		var macOsOpenResult = MacOsOpenUrl.OpenApplicationAtUrl(fullPaths,
			applicationUrl, currentPlatform);

		var windowsOpenResult = WindowsOpenDesktopApp.OpenApplicationAtUrl(fullPaths,
			applicationUrl, currentPlatform);

		return macOsOpenResult ?? windowsOpenResult;
	}

	/// <summary>
	/// Open file with default application
	/// </summary>
	/// <param name="fullPaths">full path style</param>
	/// <returns>true is operation succeed, false failed | null is platform unsupported</returns>
	public bool? OpenDefault(List<string> fullPaths)
	{
		var currentPlatform = OperatingSystemHelper.GetPlatform();
		var macOsOpenResult = MacOsOpenUrl.OpenDefault(fullPaths, currentPlatform);
		var windowsOpenResult = WindowsOpenDesktopApp.OpenDefault(fullPaths,
			currentPlatform);

		return macOsOpenResult ?? windowsOpenResult;
	}
}

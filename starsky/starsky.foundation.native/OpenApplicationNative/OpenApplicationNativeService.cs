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
	/// <param name="fullPath">system path</param>
	/// <returns>operation succeed (NOT if file is gone)</returns>
	public bool? OpenApplicationAtUrl(string fullPath, string applicationUrl)
	{
		var currentPlatform = OperatingSystemHelper.GetPlatform();
		var macOsOpenResult =
			MacOsOpenUrl.OpenApplicationAtUrl(fullPath, applicationUrl, currentPlatform);
		var windowsOpenResult = false;
		return macOsOpenResult ?? windowsOpenResult;
	}
}

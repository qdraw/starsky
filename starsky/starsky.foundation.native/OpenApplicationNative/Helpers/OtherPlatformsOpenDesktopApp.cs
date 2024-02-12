using System.Runtime.InteropServices;

namespace starsky.foundation.native.OpenApplicationNative.Helpers;

public static class OtherPlatformsOpenDesktopApp
{
	/// <summary>
	/// Skip if is MacOS
	/// </summary>
	/// <param name="fileUrl"></param>
	/// <param name="applicationUrl"></param>
	/// <param name="platform"></param>
	/// <returns></returns>
	internal static bool? OpenApplicationAtUrl(
		string fileUrl,
		string applicationUrl, OSPlatform platform)
	{
		return platform == OSPlatform.OSX ? null : OpenApplicationAtUrl(fileUrl, applicationUrl);
	}

	/// <summary>
	/// Internal
	/// </summary>
	/// <param name="fileUrl"></param>
	/// <param name="applicationUrl"></param>
	/// <returns></returns>
	internal static bool? OpenApplicationAtUrl(
		string fileUrl,
		string applicationUrl)
	{
		// do nothing
		return null;
	}
	
	
}

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace starsky.foundation.native.OpenApplicationNative.Helpers;

public static class WindowsOpenDesktopApp
{
	/// <summary>
	///     Add check if is Windows
	/// </summary>
	/// <param name="fileUrls">full file paths</param>
	/// <param name="platform">running platform</param>
	/// <returns></returns>
	internal static bool? OpenDefault(
		List<string> fileUrls, OSPlatform platform)
	{
		return platform != OSPlatform.Windows ? null : OpenDefault(fileUrls);
	}

	public static bool? OpenDefault(List<string> fileUrls)
	{
		if ( fileUrls.Count == 0 )
		{
			return false;
		}

		var result = new List<bool?>();
		foreach ( var fileUrl in fileUrls )
		{
			result.Add(OpenDefault(fileUrl));
		}

		return result.TrueForAll(p => p == true);
	}

	/// <summary>
	///     Does NOT check if file exists
	/// </summary>
	/// <param name="fileUrl">Absolute Path of file</param>
	/// <returns></returns>
	public static bool? OpenDefault(
		string fileUrl)
	{
		try
		{
			var projectStartInfo = new ProcessStartInfo
			{
				FileName = fileUrl,
				UseShellExecute = true,
				WindowStyle = ProcessWindowStyle.Normal
			};
			var projectProcess = Process.Start(projectStartInfo);
			return projectProcess != null;
		}
		catch ( Win32Exception )
		{
			return false;
		}
	}

	/// <summary>
	///     Skip if is Mac OS
	/// </summary>
	/// <param name="fileUrls"></param>
	/// <param name="applicationUrl"></param>
	/// <param name="platform"></param>
	/// <returns></returns>
	internal static bool? OpenApplicationAtUrl(
		List<string> fileUrls,
		string applicationUrl, OSPlatform platform)
	{
		return platform != OSPlatform.Windows
			? null
			: OpenApplicationAtUrl(fileUrls, applicationUrl);
	}

	/// <summary>
	///     Internal
	/// </summary>
	/// <param name="fileUrls"></param>
	/// <param name="applicationUrl"></param>
	/// <returns></returns>
	internal static bool OpenApplicationAtUrl(
		List<string> fileUrls,
		string applicationUrl)
	{
		if ( fileUrls.Count == 0 )
		{
			return false;
		}

		var results = new List<bool>();
		foreach ( var url in fileUrls )
		{
			var projectStartInfo = new ProcessStartInfo
			{
				FileName = applicationUrl,
				WindowStyle = ProcessWindowStyle.Normal,
				Arguments = url
			};

			var process = new Process { StartInfo = projectStartInfo };
			var projectProcess = process.Start();
			results.Add(projectProcess);

			process.Dispose();
		}

		return results.TrueForAll(p => p);
	}
}

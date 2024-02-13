using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace starsky.foundation.native.OpenApplicationNative.Helpers;

public static class WindowsOpenDesktopApp
{
	
	/// <summary>
	/// Add check if is Windows
	/// </summary>
	/// <param name="fileUrl"></param>
	/// <param name="platform"></param>
	/// <returns></returns>
	internal static bool? OpenDefault(
		string fileUrl, OSPlatform platform)
	{
		return platform != OSPlatform.OSX ? null : OpenDefault(fileUrl);
	}

	/// <summary>
	/// Does NOT check if file exists
	/// </summary>
	/// <param name="fileUrl">Absolute Path of file</param>
	/// <returns></returns>
	public static bool? OpenDefault(
		string fileUrl)
	{
		try
		{
			var projectStartInfo = new ProcessStartInfo();
			projectStartInfo.FileName = fileUrl;
			projectStartInfo.UseShellExecute = true;
			projectStartInfo.WindowStyle = ProcessWindowStyle.Normal;
			var projectProcess = Process.Start(projectStartInfo);
			return projectProcess != null;
		}
		catch ( Win32Exception )
		{
			return false;
		}
	}

	/// <summary>
	/// Skip if is MacOS
	/// </summary>
	/// <param name="fileUrls"></param>
	/// <param name="applicationUrl"></param>
	/// <param name="platform"></param>
	/// <returns></returns>
	internal static bool? OpenApplicationAtUrl(
		List<string> fileUrls,
		string applicationUrl, OSPlatform platform)
	{
		return platform == OSPlatform.OSX
			? null
			: OpenApplicationAtUrl(fileUrls, applicationUrl);
	}

	/// <summary>
	/// Internal
	/// </summary>
	/// <param name="fileUrls"></param>
	/// <param name="applicationUrl"></param>
	/// <returns></returns>
	internal static bool OpenApplicationAtUrl(
		List<string> fileUrls,
		string applicationUrl)
	{
		var projectStartInfo = new ProcessStartInfo();
		projectStartInfo.FileName = applicationUrl;
		projectStartInfo.WindowStyle = ProcessWindowStyle.Normal;

		projectStartInfo.Arguments = GetArguments(fileUrls);
		// not sure if needed
		projectStartInfo.LoadUserProfile = true;

		var process = new Process
		{
			StartInfo = projectStartInfo
		};
		process.Start();

		process.Dispose();
		return true;
	}
	
	
	internal static string GetArguments(List<string> fileUrls)
	{
		// %windir%\system32\mspaint.exe C:\Users\mini\Desktop\travel.png
		var arguments = new StringBuilder();
		foreach ( var url in fileUrls )
		{
			arguments.Append($"\"{url}\" ");
		}

		return arguments.ToString();
	}

}

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static Medallion.Shell.Shell;

namespace starsky.foundation.native.OpenApplicationNative.Helpers;

public static class WindowsOpenDesktopApp
{
	
	/// <summary>
	/// Add check if not Mac OS X
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
	//	var command = Default.Run(applicationUrl,
	//options:
	//opts =>
	//{
	//	opts.StartInfo(si =>
	//		si.Arguments = GetArguments(fileUrls));
	//});

		return null;
	}

	/// <summary>
	/// Skip if is MacOS
	/// </summary>
	/// <param name="fileUrls"></param>
	/// <param name="applicationUrl"></param>
	/// <param name="platform"></param>
	/// <returns></returns>
	internal static async Task<bool?> OpenApplicationAtUrl(
		List<string> fileUrls,
		string applicationUrl, OSPlatform platform)
	{
		return platform == OSPlatform.OSX
			? null
			: await OpenApplicationAtUrl(fileUrls, applicationUrl);
	}

	/// <summary>
	/// Internal
	/// </summary>
	/// <param name="fileUrls"></param>
	/// <param name="applicationUrl"></param>
	/// <returns></returns>
	internal static async Task<bool?> OpenApplicationAtUrl(
		List<string> fileUrls,
		string applicationUrl)
	{
		var command = Default.Run(applicationUrl,
			options:
			opts =>
			{
				opts.StartInfo(si =>
					si.Arguments = GetArguments(fileUrls));
			});

		var commandResult = await command.Task;

		return commandResult.Success;
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

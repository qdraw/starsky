using System.Runtime.InteropServices;
using starsky.foundation.injection;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.native.OpenApplicationNative.Interfaces;
using starsky.foundation.platform.Architecture;

namespace starsky.foundation.native.OpenApplicationNative;

[Service(typeof(IOpenApplicationNativeService), InjectionLifetime = InjectionLifetime.Scoped)]
public class OpenApplicationNativeService : IOpenApplicationNativeService
{
	/// <summary>
	///     Is Open File supported on this configuration
	/// </summary>
	/// <returns>true if supported, false if not supported</returns>
	public bool DetectToUseOpenApplication()
	{
		return DetectToUseOpenApplicationInternal(RuntimeInformation.IsOSPlatform,
			Environment.UserInteractive);
	}


	/// <summary>
	///     Open file with specified application
	/// </summary>
	/// <param name="fullPathAndApplicationUrl">List first item is fullFilePath, second is ApplicationUrl</param>
	/// <returns>true is operation succeed, false failed | null is platform unsupported</returns>
	public bool? OpenApplicationAtUrl(
		List<(string fullFilePath, string applicationUrl)> fullPathAndApplicationUrl)
	{
		if ( fullPathAndApplicationUrl.Count == 0 )
		{
			return false;
		}

		var filesByApplicationPath = SortToOpenFilesByApplicationPath(fullPathAndApplicationUrl);

		var results = new List<bool?>();
		foreach ( var (fullFilePaths, applicationPath) in filesByApplicationPath )
		{
			results.Add(OpenApplicationAtUrl(fullFilePaths, applicationPath));
		}

		if ( results.Contains(null) )
		{
			return null;
		}

		return results.TrueForAll(p => p == true);
	}

	/// <summary>
	///     Open file with default application
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

	/// <summary>
	///     Is Open File supported on this configuration
	/// </summary>
	/// <param name="runtimeInformationIsOsPlatform">RuntimeInformation.IsOSPlatform</param>
	/// <param name="environmentUserInteractive">Environment.UserInteractive</param>
	/// <returns>true if supported, false if not supported</returns>
	internal static bool DetectToUseOpenApplicationInternal(
		IsOsPlatformDelegate runtimeInformationIsOsPlatform,
		bool environmentUserInteractive)
	{
		// Linux is not supported yet
		if ( runtimeInformationIsOsPlatform(OSPlatform.Linux) ||
		     runtimeInformationIsOsPlatform(OSPlatform.FreeBSD) )
		{
			return false;
		}

		// When running in Windows as Service it does not open the application
		// On Mac OS it does open the application
		if ( !environmentUserInteractive && runtimeInformationIsOsPlatform(OSPlatform.Windows) )
		{
			return false;
		}

		return true;
	}

	/// <summary>
	///     Open file with specified application
	/// </summary>
	/// <param name="fullPaths">full path style</param>
	/// <param name="applicationUrl"> applicationUrl</param>
	/// <returns>true is operation succeed, false failed | null is platform unsupported</returns>
	internal static bool? OpenApplicationAtUrl(List<string> fullPaths, string applicationUrl)
	{
		var currentPlatform = OperatingSystemHelper.GetPlatform();
		var macOsOpenResult = MacOsOpenUrl.OpenApplicationAtUrl(fullPaths,
			applicationUrl, currentPlatform);

		var windowsOpenResult = WindowsOpenDesktopApp.OpenApplicationAtUrl(fullPaths,
			applicationUrl, currentPlatform);

		return macOsOpenResult ?? windowsOpenResult;
	}

	internal static List<(List<string>, string)> SortToOpenFilesByApplicationPath(
		List<(string fullFilePath, string applicationUrl)> fullPathAndApplicationUrl)
	{
		// Group applications by their names
		var groupedApplications = fullPathAndApplicationUrl.GroupBy(x => x.Item2).ToList();

		// Extract full paths for each application and call the implemented function
		var results = new List<(List<string>, string)>();
		foreach ( var group in groupedApplications )
		{
			var fullPaths = group.Select(item => item.Item1).ToList();
			var applicationUrl = group.Key;
			results.Add(( fullPaths, applicationUrl ));
		}

		return results;
	}

	/// <summary>
	///     Use to overwrite the RuntimeInformation.IsOSPlatform
	/// </summary>
	internal delegate bool IsOsPlatformDelegate(OSPlatform osPlatform);
}

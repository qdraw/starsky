using starsky.foundation.platform.Models;

namespace starsky.foundation.optimisation.Helpers;

public class ImageOptimisationExePath(AppSettings appSettings)
{
	internal string GetExeParentFolder(string toolName, string currentArchitecture)
	{
		return Path.Combine(appSettings.DependenciesFolder,
			string.IsNullOrEmpty(currentArchitecture)
				? toolName
				: $"{toolName}-{currentArchitecture}");
	}

	/// <summary>
	///     Get the path to the image Optimisation executable
	/// </summary>
	/// <returns>Full path of executable</returns>
	internal string GetExePath(string toolName, string currentArchitecture)
	{
		var exeFile = Path.Combine(GetExeParentFolder(toolName, currentArchitecture),
			toolName);
		if ( currentArchitecture is "win-x64" or "win-arm64" )
		{
			exeFile += ".exe";
		}

		return exeFile;
	}
}

using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Models;

namespace starsky.foundation.video.GetDependencies;

public class FfmpegExePath(AppSettings appSettings)
{
	private const string FfmpegDependenciesFolder = "ffmpeg";
	private const string FfmpegExecutableBaseName = "ffmpeg";

	internal string GetExeParentFolder(string currentArchitecture)
	{
		return Path.Combine(appSettings.DependenciesFolder,
			string.IsNullOrEmpty(currentArchitecture)
				? FfmpegDependenciesFolder
				: $"{FfmpegDependenciesFolder}-{currentArchitecture}");
	}

	/// <summary>
	///     Get the path to the ffmpeg executable (assume current architecture)
	/// </summary>
	/// <returns>Full path of executable</returns>
	internal string GetExePath()
	{
		return GetExePath(CurrentArchitecture
			.GetCurrentRuntimeIdentifier());
	}

	/// <summary>
	///     Get the path to the ffmpeg executable
	/// </summary>
	/// <returns>Full path of executable</returns>
	internal string GetExePath(string currentArchitecture)
	{
		var exeFile = Path.Combine(GetExeParentFolder(currentArchitecture),
			FfmpegExecutableBaseName);
		if ( currentArchitecture is "win-x64" or "win-arm64" )
		{
			exeFile += ".exe";
		}

		return exeFile;
	}
}

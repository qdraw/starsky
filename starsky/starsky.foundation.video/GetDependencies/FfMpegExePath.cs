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

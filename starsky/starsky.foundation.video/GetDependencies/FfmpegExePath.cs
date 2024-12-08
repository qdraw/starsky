using starsky.foundation.platform.Models;

namespace starsky.foundation.video.GetDependencies;

public class FfmpegExePath(AppSettings appSettings)
{
	private const string FfmpegDependenciesFolder = "ffmpeg";

	internal string GetExeParentFolder()
	{
		return Path.Combine(appSettings.DependenciesFolder, FfmpegDependenciesFolder);
	}

	internal string GetExePath(string currentArchitecture)
	{
		var exeFile = Path.Combine(appSettings.DependenciesFolder, FfmpegDependenciesFolder,
			"ffmpeg");
		if ( currentArchitecture is "win-x64" or "win-arm64" )
		{
			exeFile += ".exe";
		}

		return exeFile;
	}
}

using Medallion.Shell;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IFfmpegChmod), InjectionLifetime = InjectionLifetime.Scoped)]
public class FfMpegChmod(IStorage hostFileSystemStorage, IWebLogger logger) : IFfmpegChmod
{
	internal string CmdPath { get; set; } = "/bin/chmod";

	public async Task<bool> Chmod(string exeFile)
	{
		if ( !hostFileSystemStorage.ExistFile(CmdPath) )
		{
			logger.LogError("[RunChmodOnFfmpegExe] WARNING: /bin/chmod does not exist");
			return false;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run(CmdPath, "0755", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		logger.LogError(
			$"command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}
}

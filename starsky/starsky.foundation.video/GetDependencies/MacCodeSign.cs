using Medallion.Shell;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.video.GetDependencies;

public class MacCodeSign(IStorage hostFileSystemStorage, IWebLogger logger)
{
	public string CodeSignPath { get; set; } = "/usr/bin/codesign";
	public string XattrPath { get; set; } = "/usr/bin/xattr";

	public async Task<bool> MacCodeSignAndXattrExecutable(string exeFile)
	{
		var result = await MacCodeSignExecutable(exeFile);
		if ( !result )
		{
			return false;
		}

		return await MacXattrExecutable(exeFile);
	}

	private async Task<bool> MacCodeSignExecutable(string exeFile)
	{
		if ( !hostFileSystemStorage.ExistFile(CodeSignPath) )
		{
			logger.LogError("[RunChmodOnFfmpegExe] WARNING: /usr/bin/codesign does not exist");
			return true;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run(CodeSignPath, "--force", "--deep", "-s", "-", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		logger.LogError(
			$"codesign Command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}

	private async Task<bool> MacXattrExecutable(string exeFile)
	{
		if ( !hostFileSystemStorage.ExistFile(XattrPath) )
		{
			logger.LogError("[RunChmodOnFfmpegExe] WARNING: /usr/bin/xattr does not exist");
			return true;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run(XattrPath, "-rd", "com.apple.quarantine", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		logger.LogError(
			$"xattr Command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}
}

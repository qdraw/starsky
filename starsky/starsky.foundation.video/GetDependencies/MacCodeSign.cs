using Medallion.Shell;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IMacCodeSign), InjectionLifetime = InjectionLifetime.Scoped)]
public class MacCodeSign : IMacCodeSign
{
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;

	public MacCodeSign(ISelectorStorage selectorStorage, IWebLogger logger)
	{
		_hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
	}
	
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

	internal async Task<bool> MacCodeSignExecutable(string exeFile)
	{
		if ( !_hostFileSystemStorage.ExistFile(CodeSignPath) )
		{
			_logger.LogError("[RunChmodOnFfmpegExe] WARNING: /usr/bin/codesign does not exist");
			return true;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run(CodeSignPath, "--force", "--deep", "-s", "-", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		_logger.LogError(
			$"codesign Command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}

	internal async Task<bool> MacXattrExecutable(string exeFile)
	{
		if ( !_hostFileSystemStorage.ExistFile(XattrPath) )
		{
			_logger.LogError("[RunChmodOnFfmpegExe] WARNING: /usr/bin/xattr does not exist");
			return true;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run(XattrPath, "-rd", "com.apple.quarantine", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		_logger.LogError(
			$"xattr Command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}
}


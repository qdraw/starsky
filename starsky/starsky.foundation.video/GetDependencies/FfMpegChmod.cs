using Medallion.Shell;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IFfmpegChmod), InjectionLifetime = InjectionLifetime.Scoped)]
public class FfMpegChmod : IFfmpegChmod
{
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;

	public FfMpegChmod(ISelectorStorage selectorStorage, IWebLogger logger)
	{
		_hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
	}
	internal string CmdPath { get; set; } = "/bin/chmod";

	public async Task<bool> Chmod(string exeFile)
	{
		if ( !_hostFileSystemStorage.ExistFile(CmdPath) )
		{
			_logger.LogError("[RunChmodOnFfmpegExe] WARNING: /bin/chmod does not exist");
			return false;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run(CmdPath, "0755", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		_logger.LogError(
			$"command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}
}

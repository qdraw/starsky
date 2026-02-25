using System.Runtime.CompilerServices;
using Medallion.Shell;
using starsky.foundation.injection;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.optimisation.Services;

[Service(typeof(IImageOptimisationChmod), InjectionLifetime = InjectionLifetime.Scoped)]
public class ImageOptimisationChmod(ISelectorStorage selectorStorage, IWebLogger logger)
	: IImageOptimisationChmod
{
	private readonly IStorage _hostFileSystemStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	internal string CmdPath { get; set; } = "/bin/chmod";

	public async Task<bool> Chmod(string exeFile)
	{
		if ( !_hostFileSystemStorage.ExistFile(CmdPath) )
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

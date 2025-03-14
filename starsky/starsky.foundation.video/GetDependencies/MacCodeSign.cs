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
	private const string CodeSignDefaultPath = "/usr/bin/codesign";
	private const string XattrDefaultPath = "/usr/bin/xattr";

	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;

	public MacCodeSign(ISelectorStorage selectorStorage, IWebLogger logger)
	{
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
	}

	internal string CodeSignPath { get; set; } = CodeSignDefaultPath;
	internal string XattrPath { get; set; } = XattrDefaultPath;

	public async Task<bool?> MacCodeSignAndXattrExecutable(string exeFile)
	{
		if ( !_hostFileSystemStorage.ExistFile(exeFile) )
		{
			return null;
		}

		var result = await MacCodeSignExecutable(exeFile);
		if ( result is null or false )
		{
			return result;
		}

		return await MacXattrExecutable(exeFile);
	}

	internal async Task<bool?> MacCodeSignExecutable(string exeFile)
	{
		if ( !_hostFileSystemStorage.ExistFile(exeFile) )
		{
			_logger.LogError($"[MacCodeSignExecutable] WARNING: {exeFile} does not exists");
			return null;
		}

		if ( !_hostFileSystemStorage.ExistFile(CodeSignPath) )
		{
			_logger.LogError("[MacCodeSignExecutable] WARNING: /usr/bin/codesign does not exist");
			return null;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run(CodeSignPath, "--force", "--deep", "-s", "-", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		_logger.LogError(
			$"[MacCodeSignExecutable] codesign Command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}

	internal async Task<bool?> MacXattrExecutable(string exeFile)
	{
		if ( !_hostFileSystemStorage.ExistFile(exeFile) )
		{
			_logger.LogError($"[MacXattrExecutable] WARNING: {exeFile} does not exists");
			return null;
		}

		if ( !_hostFileSystemStorage.ExistFile(XattrPath) )
		{
			_logger.LogError("[MacXattrExecutable] WARNING: /usr/bin/xattr does not exist");
			return null;
		}

		// command.run does not care about the $PATH
		var result = await Command.Run(XattrPath, "-rd", "com.apple.quarantine", exeFile).Task;
		if ( result.Success )
		{
			return true;
		}

		_logger.LogError(
			$"[MacXattrExecutable] xattr Command failed with exit code {result.ExitCode}: {result.StandardError}");
		return false;
	}
}

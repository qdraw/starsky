using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Interfaces;

namespace starsky.foundation.video.GetDependencies;

[Service(typeof(IFfMpegPrepareBeforeRunning), InjectionLifetime = InjectionLifetime.Scoped)]
public class FfMpegPrepareBeforeRunning : IFfMpegPrepareBeforeRunning
{
	private readonly IFfmpegChmod _ffmpegChmod;
	private readonly FfmpegExePath _ffmpegExePath;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly IMacCodeSign _macCodeSign;

	public FfMpegPrepareBeforeRunning(ISelectorStorage selectorStorage, IMacCodeSign macCodeSign,
		IFfmpegChmod ffmpegChmod,
		AppSettings appSettings, IWebLogger logger)
	{
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_ffmpegExePath = new FfmpegExePath(appSettings);
		_macCodeSign = macCodeSign;
		_ffmpegChmod = ffmpegChmod;
		_logger = logger;
	}

	public async Task<bool> PrepareBeforeRunning(string currentArchitecture)
	{
		var exeFile = _ffmpegExePath.GetExePath(currentArchitecture);

		if ( !_hostFileSystemStorage.ExistFile(exeFile) )
		{
			_logger.LogError($"[FfMpegDownload] exeFile {exeFile} does not exist");
			return false;
		}

		if ( currentArchitecture is "win-arm64" or "win-x64" )
		{
			return true;
		}

		if ( !await _ffmpegChmod.Chmod(exeFile) )
		{
			_logger.LogError("[FfMpegDownload] Chmod failed");
			return false;
		}

		if ( currentArchitecture is "osx-x64" or "osx-arm64" )
		{
			return await _macCodeSign.MacCodeSignAndXattrExecutable(exeFile) == true;
		}

		return true;
	}
}

using Medallion.Shell;
using starsky.foundation.injection;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.optimisation.Services;

[Service(typeof(IMozJpegService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MozJpegService : IMozJpegService
{
	private readonly IMozJpegDownload _mozJpegDownload;
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;

	public MozJpegService(AppSettings appSettings,
		ISelectorStorage selectorStorage, IWebLogger logger, IMozJpegDownload mozJpegDownload)
	{
		_appSettings = appSettings;
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
		_mozJpegDownload = mozJpegDownload;
	}

	public async Task RunMozJpeg(Optimizer optimizer,
		IEnumerable<ImageOptimisationItem> targets)
	{
		var downloadStatus = await _mozJpegDownload.Download();
		if ( downloadStatus != ImageOptimisationDownloadStatus.Ok )
		{
			_logger.LogError(
				$"[ImageOptimisationService] MozJPEG download failed: {downloadStatus}");
			return;
		}

		var cjpegPath = GetMozJpegPath();
		if ( !_hostFileSystemStorage.ExistFile(cjpegPath) )
		{
			_logger.LogError($"[ImageOptimisationService] cjpeg not found at {cjpegPath}");
			return;
		}

		foreach ( var outputPath in targets.Select(item => item.OutputPath) )
		{
			if ( !IsJpegOutput(outputPath) )
			{
				continue;
			}

			var tempFilePath = outputPath + ".optimizing";
			var result = await Command.Run(cjpegPath,
				"-quality", optimizer.Options.Quality.ToString(),
				"-optimize",
				"-outfile", tempFilePath,
				outputPath).Task;

			if ( !result.Success || !_hostFileSystemStorage.ExistFile(tempFilePath) )
			{
				_logger.LogError(
					$"[ImageOptimisationService] cjpeg failed for {outputPath}: {result.StandardError}");
				if ( _hostFileSystemStorage.ExistFile(tempFilePath) )
				{
					_hostFileSystemStorage.FileDelete(tempFilePath);
				}

				continue;
			}

			_hostFileSystemStorage.FileDelete(outputPath);
			_hostFileSystemStorage.FileMove(tempFilePath, outputPath);
		}
	}

	private string GetMozJpegPath()
	{
		var architecture = CurrentArchitecture.GetCurrentRuntimeIdentifier();
		var exeName = _appSettings.IsWindows ? "cjpeg.exe" : "cjpeg";
		return Path.Combine(_appSettings.DependenciesFolder, "mozjpeg", architecture, exeName);
	}

	private bool IsJpegOutput(string outputPath)
	{
		var stream = _hostFileSystemStorage.ReadStream(outputPath, 68);
		var format = new ExtensionRolesHelper(_logger).GetImageFormat(stream);
		return format == ExtensionRolesHelper.ImageFormat.jpg;
	}
}

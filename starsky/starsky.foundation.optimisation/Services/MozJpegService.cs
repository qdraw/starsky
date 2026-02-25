using starsky.foundation.injection;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using static Medallion.Shell.Shell;

namespace starsky.foundation.optimisation.Services;

[Service(typeof(IMozJpegService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MozJpegService : IMozJpegService
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly IMozJpegDownload _mozJpegDownload;

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
		if ( downloadStatus != ImageOptimisationDownloadStatus.Ok &&
		     downloadStatus != ImageOptimisationDownloadStatus.OkAlreadyDownloaded )
		{
			_logger.LogError(
				$"[ImageOptimisationService] MozJPEG download failed: {downloadStatus}");
			return;
		}

		var cJpegPath = GetMozJpegPath();
		if ( !_hostFileSystemStorage.ExistFile(cJpegPath) )
		{
			_logger.LogError($"[ImageOptimisationService] cjpeg not found at {cJpegPath}");
			return;
		}

		foreach ( var outputInputPath in targets.Select(item => item.OutputPath) )
		{
			if ( !IsJpegOutput(outputInputPath) )
			{
				continue;
			}

			var tempFilePath = outputInputPath + ".optimizing";
			await using var outputStream = File.OpenWrite(tempFilePath);

			var parent = Directory.GetParent(cJpegPath);
			List<string> arguments =
				["-quality", optimizer.Options.Quality.ToString(), "-optimize", outputInputPath];

			var command = Default.Run(
				cJpegPath,
				options: opts =>
				{
					opts.StartInfo(i => i.Arguments = string.Join(" ", arguments));
					opts.WorkingDirectory(parent!.FullName);
				}
			) > outputStream;
			await command.Task;

			if ( !command.Result.Success )
			{
				_logger.LogError(
					$"[ImageOptimisationService] cjpeg failed for {outputInputPath}: " +
					$"{command.Result.StandardError}");
				if ( _hostFileSystemStorage.ExistFile(tempFilePath) )
				{
					_hostFileSystemStorage.FileDelete(tempFilePath);
				}

				continue;
			}

			_hostFileSystemStorage.FileDelete(outputInputPath);
			_hostFileSystemStorage.FileMove(tempFilePath, outputInputPath);
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
		using var stream =  _hostFileSystemStorage.ReadStream(outputPath, 68);
		var format = new ExtensionRolesHelper(_logger).GetImageFormat(stream);
		return format == ExtensionRolesHelper.ImageFormat.jpg;
	}
}

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

[Service(typeof(IImageOptimisationService), InjectionLifetime = InjectionLifetime.Scoped)]
public class ImageOptimisationService : IImageOptimisationService
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly IMozJpegDownload _mozJpegDownload;

	public ImageOptimisationService(AppSettings appSettings,
		ISelectorStorage selectorStorage, IWebLogger logger,
		IMozJpegDownload mozJpegDownload)
	{
		_appSettings = appSettings;
		_logger = logger;
		_mozJpegDownload = mozJpegDownload;
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
	}

	public async Task Optimize(IReadOnlyCollection<ImageOptimisationItem> images,
		List<Optimizer>? optimizers = null)
	{
		if ( images.Count == 0 )
		{
			return;
		}

		optimizers ??= _appSettings.PublishProfilesDefaults?.Optimizers ?? [];
		if ( optimizers.Count == 0 )
		{
			return;
		}

		foreach ( var optimizer in optimizers.Where(p => p.Enabled) )
		{
			var targets = images
				.Where(image => optimizer.ImageFormats.Count == 0 ||
				                optimizer.ImageFormats.Contains(image.ImageFormat))
				.Where(p => _hostFileSystemStorage.ExistFile(p.OutputPath))
				.ToList();
			if ( targets.Count == 0 )
			{
				continue;
			}

			switch ( optimizer.Id.ToLowerInvariant() )
			{
				case "mozjpeg":
					await RunMozJpeg(optimizer, targets);
					break;
				default:
					_logger.LogInformation($"[ImageOptimisationService] Unknown optimizer id: {optimizer.Id}");
					break;
			}
		}
	}

	private async Task RunMozJpeg(Optimizer optimizer,
		IEnumerable<ImageOptimisationItem> targets)
	{
		var downloadStatus = await _mozJpegDownload.Download();
		if ( downloadStatus != ImageOptimisationDownloadStatus.Ok )
		{
			_logger.LogError($"[ImageOptimisationService] MozJPEG download failed: {downloadStatus}");
			return;
		}

		var cjpegPath = GetMozJpegPath();
		if ( !_hostFileSystemStorage.ExistFile(cjpegPath) )
		{
			_logger.LogError($"[ImageOptimisationService] cjpeg not found at {cjpegPath}");
			return;
		}

		foreach ( var item in targets )
		{
			if ( !IsJpegOutput(item.OutputPath) )
			{
				continue;
			}

			var tempFilePath = item.OutputPath + ".optimizing";
			var result = await Command.Run(cjpegPath,
				"-quality", optimizer.Options.Quality.ToString(),
				"-optimize",
				"-outfile", tempFilePath,
				item.OutputPath).Task;

			if ( !result.Success || !_hostFileSystemStorage.ExistFile(tempFilePath) )
			{
				_logger.LogError($"[ImageOptimisationService] cjpeg failed for {item.OutputPath}: {result.StandardError}");
				if ( _hostFileSystemStorage.ExistFile(tempFilePath) )
				{
					_hostFileSystemStorage.FileDelete(tempFilePath);
				}
				continue;
			}

			_hostFileSystemStorage.FileDelete(item.OutputPath);
			_hostFileSystemStorage.FileMove(tempFilePath, item.OutputPath);
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

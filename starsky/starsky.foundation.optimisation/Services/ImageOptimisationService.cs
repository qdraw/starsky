using starsky.foundation.injection;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
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
	private readonly IMozJpegService _mozJpegService;

	public ImageOptimisationService(AppSettings appSettings,
		ISelectorStorage selectorStorage, IWebLogger logger,
		IMozJpegService mozJpegService)
	{
		_appSettings = appSettings;
		_logger = logger;
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_mozJpegService = mozJpegService;
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
					await _mozJpegService.RunMozJpeg(optimizer, targets);
					break;
				default:
					_logger.LogInformation(
						$"[ImageOptimisationService] Unknown optimizer id: {optimizer.Id}");
					break;
			}
		}
	}
}

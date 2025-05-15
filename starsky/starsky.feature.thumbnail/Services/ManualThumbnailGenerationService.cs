using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IManualThumbnailGenerationService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class ManualThumbnailGenerationService : IManualThumbnailGenerationService
{
	private readonly IThumbnailQueuedHostedService _bgTaskQueue;
	private readonly IWebLogger _logger;
	private readonly IThumbnailSocketService _socketService;
	private readonly IThumbnailService _thumbnailService;

	public ManualThumbnailGenerationService(IWebLogger logger,
		IThumbnailSocketService socketService,
		IThumbnailService thumbnailService,
		IThumbnailQueuedHostedService bgTaskQueue)
	{
		_logger = logger;
		_socketService = socketService;
		_thumbnailService = thumbnailService;
		_bgTaskQueue = bgTaskQueue;
	}

	public async Task ManualBackgroundQueue(string subPath)
	{
		// When the CPU is too high its gives an Error 500
		await _bgTaskQueue.QueueBackgroundWorkItemAsync(
			async _ => { await WorkThumbnailGeneration(subPath); }, subPath);
	}

	internal async Task WorkThumbnailGeneration(string subPath)
	{
		try
		{
			_logger.LogInformation($"[ThumbnailGenerationController] start {subPath}");
			var generateThumbnailResults = await _thumbnailService.GenerateThumbnail(subPath);
			await _socketService.NotificationSocketUpdate(subPath, generateThumbnailResults);
			_logger.LogInformation($"[ThumbnailGenerationController] done {subPath}");
		}
		catch ( UnauthorizedAccessException e )
		{
			_logger.LogError($"[ThumbnailGenerationController] catch-ed exception {e.Message}", e);
		}
	}
}

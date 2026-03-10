using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Models;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IManualThumbnailGenerationService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class ManualThumbnailGenerationService : IManualThumbnailGenerationService
{
	public const string JobType = "Thumbnail.ManualGeneration.v1";
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
		await _bgTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
		{
			MetaData = subPath,
			TraceParentId = null,
			PriorityLane = ProcessTaskQueue.PriorityLaneThumbnail,
			JobType = JobType,
			PayloadJson = JsonSerializer.Serialize(new ManualThumbnailGenerationPayload
			{
				SubPath = subPath
			})
		});
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

public sealed class ManualThumbnailGenerationPayload
{
	public string SubPath { get; set; } = string.Empty;
}

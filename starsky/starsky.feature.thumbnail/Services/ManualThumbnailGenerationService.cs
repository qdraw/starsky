using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.thumbnail.Services;

[Service(typeof(IManualThumbnailGenerationService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class ManualThumbnailGenerationService : IManualThumbnailGenerationService
{
	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly IWebSocketConnectionsService _connectionsService;
	private readonly IThumbnailService _thumbnailService;
	private readonly IThumbnailQueuedHostedService _bgTaskQueue;
	
	public ManualThumbnailGenerationService(IQuery query, IWebLogger logger, IWebSocketConnectionsService connectionsService, 
		IThumbnailService thumbnailService, 
		IThumbnailQueuedHostedService bgTaskQueue)
	{
		_query = query;
		_logger = logger;
		_connectionsService = connectionsService;
		_thumbnailService = thumbnailService;
		_bgTaskQueue = bgTaskQueue;	
	}

	public async Task ManualBackgroundQueue(string subPath)
	{
		// When the CPU is to high its gives a Error 500
		await _bgTaskQueue.QueueBackgroundWorkItemAsync(async _ =>
		{
			await WorkThumbnailGeneration(subPath);
		}, subPath);
	}
	
	internal async Task WorkThumbnailGeneration(string subPath)
	{
		try
		{
			_logger.LogInformation($"[ThumbnailGenerationController] start {subPath}");
			var thumbs = await _thumbnailService.CreateThumbnailAsync(subPath);
			var getAllFilesAsync = await _query.GetAllFilesAsync(subPath);

			var result =
				WhichFilesNeedToBePushedForUpdates(thumbs, getAllFilesAsync);

			if ( result.Count == 0 )
			{
				_logger.LogInformation($"[ThumbnailGenerationController] done - no results {subPath}");
				return;
			}

			var webSocketResponse =
				new ApiNotificationResponseModel<List<FileIndexItem>>(result, ApiNotificationType.ThumbnailGeneration);
			await _connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
				
			_logger.LogInformation($"[ThumbnailGenerationController] done {subPath}");
		}
		catch ( UnauthorizedAccessException e )
		{
			_logger.LogError($"[ThumbnailGenerationController] catch-ed exception {e.Message}", e);
		}
	}

	internal static List<FileIndexItem> WhichFilesNeedToBePushedForUpdates(List<GenerationResultModel> thumbs, IEnumerable<FileIndexItem> getAllFilesAsync)
	{
		var result = new List<FileIndexItem>();
		var searchFor = getAllFilesAsync.Where(item =>
			thumbs.Find(p => p.SubPath == item.FilePath && item.Tags != null)
				?.Success == true).DistinctBy(p => p.FilePath);
		foreach ( var item in searchFor )
		{
			if ( item.Tags!.Contains(TrashKeyword.TrashKeywordString) ) continue;

			item.LastChanged = new List<string> {"LastEdited", "FileHash"};
			result.Add(item);
		}

		return result;
	}
}



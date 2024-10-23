using System;
using System.Collections.Generic;
using System.Linq;
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
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IDatabaseThumbnailGenerationService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class DatabaseThumbnailGenerationService : IDatabaseThumbnailGenerationService
{
	private readonly IThumbnailQueuedHostedService _bgTaskQueue;
	private readonly IWebSocketConnectionsService _connectionsService;
	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly IThumbnailQuery _thumbnailQuery;
	private readonly IThumbnailService _thumbnailService;
	private readonly IUpdateStatusGeneratedThumbnailService _updateStatusGeneratedThumbnailService;

	public DatabaseThumbnailGenerationService(IQuery query, IWebLogger logger,
		IWebSocketConnectionsService connectionsService,
		IThumbnailService thumbnailService, IThumbnailQuery thumbnailQuery,
		IThumbnailQueuedHostedService bgTaskQueue,
		IUpdateStatusGeneratedThumbnailService updateStatusGeneratedThumbnailService)
	{
		_query = query;
		_logger = logger;
		_connectionsService = connectionsService;
		_thumbnailService = thumbnailService;
		_thumbnailQuery = thumbnailQuery;
		_bgTaskQueue = bgTaskQueue;
		_updateStatusGeneratedThumbnailService = updateStatusGeneratedThumbnailService;
	}

	public async Task StartBackgroundQueue(DateTime endTime)
	{
		var totalProcessed = 0;
		var currentPage = 0;
		const int batchSize = 100;

		if ( _thumbnailQuery.IsRunningJobAsync() )
		{
			return;
		}

		//todo: add a check to see if the service is already running

		List<ThumbnailItem> missingThumbnails;

		do
		{
			// Query missing thumbnails in batches
			missingThumbnails =
				await _thumbnailQuery.GetMissingThumbnailsBatchAsync(currentPage, batchSize);

			// Process each batch
			var fileHashesList = missingThumbnails.Select(p => p.FileHash).ToList();
			var queryItems = await _query.GetObjectsByFileHashAsync(fileHashesList);
			if ( queryItems.Count == 0 )
			{
				break;
			}

			await _bgTaskQueue.QueueBackgroundWorkItemAsync(
				async _ => { await WorkThumbnailGeneration(missingThumbnails, queryItems); },
				"DatabaseThumbnailGenerationService");

			totalProcessed += missingThumbnails.Count;
			currentPage++;

			_logger.LogInformation(
				$"[DatabaseThumbnailGenerationService] Processed {totalProcessed} thumbnails so far...");
		} while ( missingThumbnails.Count == batchSize );
	}

	internal async Task<IEnumerable<ThumbnailItem>> WorkThumbnailGeneration(
		List<ThumbnailItem> chuckedItems,
		List<FileIndexItem> fileIndexItems)
	{
		var resultData = new List<FileIndexItem>();

		foreach ( var item in chuckedItems )
		{
			var fileIndexItem = fileIndexItems.Find(p => p.FileHash == item.FileHash);
			if ( fileIndexItem?.FilePath == null ||
			     fileIndexItem.Status != FileIndexItem.ExifStatus.Ok )
			{
				// when null set to false
				item.Small ??= false;
				item.Large ??= false;
				item.ExtraLarge ??= false;
				await _thumbnailQuery.UpdateAsync(item);
				continue;
			}

			var generationResultModels = ( await _thumbnailService.CreateThumbAsync(fileIndexItem
				.FilePath!, fileIndexItem.FileHash!) ).ToList();

			await _updateStatusGeneratedThumbnailService.AddOrUpdateStatusAsync(
				generationResultModels);
			var removedItems = await _updateStatusGeneratedThumbnailService
				.RemoveNotfoundStatusAsync(generationResultModels);
			if ( removedItems.Count != 0 )
			{
				_logger.LogInformation(
					$"[DatabaseThumbnailGenerationService] removed items ({DateTime.UtcNow:HH:mm:ss})" +
					$" items: {string.Join(",", removedItems)}");
				continue;
			}

			resultData.Add(fileIndexItem);
		}

		var filteredData = resultData
			.Where(p => p.Status == FileIndexItem.ExifStatus.Ok).ToList();

		if ( filteredData.Count == 0 )
		{
			_logger.LogInformation(
				$"[DatabaseThumbnailGenerationService] no items ({DateTime.UtcNow:HH:mm:ss})");
			return chuckedItems;
		}

		_logger.LogInformation(
			$"[DatabaseThumbnailGenerationService] done ({DateTime.UtcNow:HH:mm:ss})" +
			$" {filteredData.Count} items: " +
			$"{string.Join(",", filteredData.Select(p => p.FilePath).ToList())}");

		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(filteredData,
				ApiNotificationType.ThumbnailGeneration);
		await _connectionsService.SendToAllAsync(webSocketResponse,
			new CancellationToken());

		return chuckedItems;
	}
}

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
using starsky.foundation.platform.Extensions;
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
	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly IWebSocketConnectionsService _connectionsService;
	private readonly IThumbnailService _thumbnailService;
	private readonly IThumbnailQueuedHostedService _bgTaskQueue;
	private readonly IUpdateStatusGeneratedThumbnailService _updateStatusGeneratedThumbnailService;
	private readonly IThumbnailQuery _thumbnailQuery;

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
		var thumbnailItems = await _thumbnailQuery.UnprocessedGeneratedThumbnails();
		var fileHashesList = thumbnailItems.Select(p => p.FileHash).ToList();
		var queryItems = await _query.GetObjectsByFileHashAsync(fileHashesList);

		foreach ( var chuckedItems in thumbnailItems.ChunkyEnumerable(50) )
		{
			// When the CPU is to high its gives a Error 500
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(async _ =>
			{
				await FilterAndWorkThumbnailGeneration(endTime,
					chuckedItems.ToList(), queryItems);
			}, "DatabaseThumbnailGenerationService");
		}
	}

	internal async Task<IEnumerable<ThumbnailItem>> FilterAndWorkThumbnailGeneration(
		DateTime endTime,
		List<ThumbnailItem> chuckedItems,
		List<FileIndexItem> queryItems)
	{
		// todo: refactor 
		// message:Cancel job due timeout

		if ( endTime > DateTime.UtcNow )
		{
			return await WorkThumbnailGeneration(chuckedItems, queryItems);
		}

		return new List<ThumbnailItem>();
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

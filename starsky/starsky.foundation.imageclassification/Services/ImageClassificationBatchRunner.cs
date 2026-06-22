using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.imageclassification.Interfaces;
using starsky.foundation.imageclassification.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.imageclassification.Services;

[Service(typeof(IImageClassificationBatchRunner), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ImageClassificationBatchRunner(
	IQuery query,
	IImageClassificationBackgroundTaskQueue queue,
	AppSettings appSettings,
	IWebLogger logger) : IImageClassificationBatchRunner
{
	public async Task<int> EnqueueBatchAsync(CancellationToken cancellationToken = default)
	{
		var allItems = await query.GetAllRecursiveAsync("/");
		var batchSize = appSettings.ImageClassificationBatchSize > 0
			? appSettings.ImageClassificationBatchSize
			: 25;

		var candidates = allItems
			.Where(IsClassificationCandidate)
			.OrderByDescending(GetOrderingDate)
			.Take(batchSize)
			.ToList();

		foreach ( var fileIndexItem in candidates )
		{
			var payload = new ImageClassificationQueuePayload
			{
				FilePath = fileIndexItem.FilePath ?? string.Empty
			};

			await queue.QueueJobAsync(new BackgroundTaskQueueJob
			{
				MetaData = fileIndexItem.FilePath,
				TraceParentId = Activity.Current?.Id,
				PriorityLane = ProcessTaskQueue.PriorityLaneImageClassification,
				JobType = ImageClassificationBackgroundJobHandler.ImageClassificationJobType,
				PayloadJson = JsonSerializer.Serialize(payload)
			});
		}

		logger.LogInformation(
			$"[ImageClassificationBatchRunner] Enqueued {candidates.Count} jobs (batchSize {batchSize})");
		return candidates.Count;
	}

	internal static DateTime GetOrderingDate(FileIndexItem fileIndexItem)
	{
		return fileIndexItem.DateTime > DateTime.MinValue
			? fileIndexItem.DateTime
			: fileIndexItem.LastEdited;
	}

	private static bool IsClassificationCandidate(FileIndexItem fileIndexItem)
	{
		if ( fileIndexItem.IsDirectory == true || string.IsNullOrWhiteSpace(fileIndexItem.FilePath) )
		{
			return false;
		}

		return string.IsNullOrWhiteSpace(fileIndexItem.SuggestedTags);
	}
}


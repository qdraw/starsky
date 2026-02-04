using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starsky.feature.thumbnail.Services;

[Service(typeof(ISmallThumbnailBackgroundJobService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class SmallThumbnailBackgroundJobService(
	IThumbnailQueuedHostedService bgTaskQueue,
	IThumbnailService thumbnailService,
	ISelectorStorage selectorStorage,
	IThumbnailSocketService socketService,
	IWebLogger logger) : ISmallThumbnailBackgroundJobService
{
	private readonly IStorage _storage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	public async Task<bool> CreateJob(bool? isAuthenticated, string? filePath)
	{
		var path = filePath ?? string.Empty;
		var exists = _storage.ExistFile(path);
		if ( !exists || isAuthenticated != true )
		{
			return false;
		}

		if ( bgTaskQueue.Count() >= 5000 )
		{
			logger.LogError("[SmallThumbnailBackgroundJobService] Too many items in queue");
			return false;
		}

		await bgTaskQueue.QueueBackgroundWorkItemAsync(
			async _ => { await WorkThumbnailGenerationLoop(path); },
			"SmallThumbnailBackgroundJobService");
		return true;
	}

	private async Task WorkThumbnailGenerationLoop(string path)
	{
		await Task.Yield();
		var result =
			await thumbnailService.GenerateThumbnail(path, ThumbnailGenerationType.SmallOnly);
		await socketService.NotificationSocketUpdate(path, result);
	}
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.sync.SyncServices
{
	[Service(typeof(IManualBackgroundSyncService), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class ManualBackgroundSyncService : IManualBackgroundSyncService
	{
		private readonly ISynchronize _synchronize;
		private readonly IQuery _query;
		private readonly ISocketSyncUpdateService _socketUpdateService;
		private readonly IMemoryCache _cache;
		private readonly IWebLogger _logger;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;

		public ManualBackgroundSyncService(ISynchronize synchronize, IQuery query,
			ISocketSyncUpdateService socketUpdateService,
			IMemoryCache cache, IWebLogger logger, IUpdateBackgroundTaskQueue bgTaskQueue)
		{
			_synchronize = synchronize;
			_socketUpdateService = socketUpdateService;
			_query = query;
			_cache = cache;
			_logger = logger;
			_bgTaskQueue = bgTaskQueue;
		}

		internal const string ManualSyncCacheName = "ManualSync_";

		public async Task<FileIndexItem.ExifStatus> ManualSync(string subPath)
		{
			var fileIndexItem = await _query.GetObjectByFilePathAsync(subPath);
			// on a new database ->
			if ( subPath == "/" && fileIndexItem == null ) fileIndexItem = new FileIndexItem();
			if ( fileIndexItem == null )
			{
				_logger.LogInformation($"[ManualSync] NotFoundNotInIndex skip for: {subPath}");
				return FileIndexItem.ExifStatus.NotFoundNotInIndex;
			}

			if ( _cache.TryGetValue(ManualSyncCacheName + subPath, out _) )
			{
				// also used in removeCache
				_query.RemoveCacheParentItem(subPath);
				_logger.LogInformation($"[ManualSync] Cache hit skip for: {subPath}");
				return FileIndexItem.ExifStatus.OperationNotSupported;
			}

			CreateSyncLock(subPath);

			// Runs within IUpdateBackgroundTaskQueue
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(async _ =>
			{
				await BackgroundTaskExceptionWrapper(fileIndexItem.FilePath!);
			}, fileIndexItem.FilePath!);

			return FileIndexItem.ExifStatus.Ok;
		}

		internal void CreateSyncLock(string subPath)
		{
			_cache.Set(ManualSyncCacheName + subPath, true,
				new TimeSpan(0, 2, 0));
		}

		private void RemoveSyncLock(string? subPath)
		{
			subPath ??= string.Empty;
			_cache.Remove(ManualSyncCacheName + subPath);
		}

		internal async Task BackgroundTaskExceptionWrapper(string? subPath)
		{
			try
			{
				await BackgroundTask(subPath);
			}
			catch ( Exception exception )
			{
				_logger.LogError(exception,
					"ManualBackgroundSyncService [ManualSync] catch-ed exception");
				RemoveSyncLock(subPath);
				throw;
			}
		}

		internal async Task BackgroundTask(string? subPath)
		{
			subPath ??= string.Empty;

			_logger.LogInformation($"[ManualBackgroundSyncService] start {subPath} " +
			                       $"{DateTime.Now.ToShortTimeString()}");

			var updatedList = await _synchronize.Sync(subPath, _socketUpdateService.PushToSockets);

			_query.CacheUpdateItem(updatedList.Where(p => p.ParentDirectory == subPath).ToList());

			// so you can click on the button again
			RemoveSyncLock(subPath);
			_logger.LogInformation($"[ManualBackgroundSyncService] done {subPath} " +
			                       $"{DateTime.Now.ToShortTimeString()}");
			_logger.LogInformation(
				$"[ManualBackgroundSyncService] Ok: {updatedList.Count(p => p.Status == FileIndexItem.ExifStatus.Ok)}" +
				$" ~ OkAndSame: {updatedList.Count(p => p.Status == FileIndexItem.ExifStatus.OkAndSame)}");
		}
	}
}

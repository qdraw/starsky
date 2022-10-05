using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.sync.SyncServices
{
	[Service(typeof(IManualBackgroundSyncService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ManualBackgroundSyncService : IManualBackgroundSyncService
	{
		private readonly ISynchronize _synchronize;
		private readonly IQuery _query;
		private readonly IWebSocketConnectionsService _connectionsService;
		private readonly IMemoryCache _cache;
		private readonly IWebLogger _logger;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly IServiceScopeFactory _scopeFactory;

		public ManualBackgroundSyncService(ISynchronize synchronize, IQuery query,
			IWebSocketConnectionsService connectionsService, 
			IMemoryCache cache , IWebLogger logger, IUpdateBackgroundTaskQueue bgTaskQueue, 
			IServiceScopeFactory scopeFactory)
		{
			_synchronize = synchronize;
			_connectionsService = connectionsService;
			_query = query;
			_cache = cache;
			_logger = logger;
			_bgTaskQueue = bgTaskQueue;
			_scopeFactory = scopeFactory;
		}

		internal const string ManualSyncCacheName = "ManualSync_";
		
		public async Task<FileIndexItem.ExifStatus> ManualSync(string subPath,
			string operationId = null)
		{
			var fileIndexItem = await _query.GetObjectByFilePathAsync(subPath);
			// on a new database ->
			if ( subPath == "/" && fileIndexItem == null) fileIndexItem = new FileIndexItem();
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

			_bgTaskQueue.QueueBackgroundWorkItem(async _ =>
			{
				await BackgroundTaskExceptionWrapper(fileIndexItem.FilePath,
					operationId);
			});

			return FileIndexItem.ExifStatus.Ok;
		}

		internal async Task PushToSockets(List<FileIndexItem> updatedList)
		{
			var webSocketResponse =
				new ApiNotificationResponseModel<List<FileIndexItem>>(updatedList, ApiNotificationType.ManualBackgroundSync);
			await _connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
		}

		internal void CreateSyncLock(string subPath)
		{
			_cache.Set(ManualSyncCacheName + subPath, true, 
				new TimeSpan(0,2,0));
		}

		private void RemoveSyncLock(string subPath)
		{
			_cache.Remove(ManualSyncCacheName + subPath);
		}

		internal async Task BackgroundTaskExceptionWrapper(string subPath, string operationId)
		{
			try
			{
				await BackgroundTask(subPath, operationId);
			}
			catch ( Exception exception)
			{
				_logger.LogError(exception,"ManualBackgroundSyncService [ManualSync] catch-ed exception");
				RemoveSyncLock(subPath);
				throw;
			}
		}

		internal async Task BackgroundTask(string subPath, string operationId)
		{
			var operationHolder = RequestTelemetryHelper.GetOperationHolder(_scopeFactory,
				nameof(ManualSync), operationId);
			
			_logger.LogInformation($"[ManualBackgroundSyncService] start {subPath} " +
			                       $"{DateTime.Now.ToShortTimeString()}");
			
			var updatedList = await _synchronize.Sync(subPath, false, PushToSockets);
			
			_query.CacheUpdateItem(updatedList.Where(p => p.ParentDirectory == subPath).ToList());
			
			// so you can click on the button again
			RemoveSyncLock(subPath);
			_logger.LogInformation($"[ManualBackgroundSyncService] done {subPath} " +
			                       $"{DateTime.Now.ToShortTimeString()}");
			_logger.LogInformation($"[ManualBackgroundSyncService] Ok: {updatedList.Count(p => p.Status == FileIndexItem.ExifStatus.Ok)}" +
			                       $" ~ OkAndSame: {updatedList.Count(p => p.Status == FileIndexItem.ExifStatus.OkAndSame)}");
			operationHolder.SetData(_scopeFactory, updatedList);
		}
		
		[SuppressMessage("Performance", "CA1822:Mark members as static")]
		internal List<FileIndexItem> FilterBefore(IReadOnlyCollection<FileIndexItem> syncData)
		{
			return syncData.Where(p => (
				p.Status == FileIndexItem.ExifStatus.Ok ||
				p.Status == FileIndexItem.ExifStatus.OkAndSame ||
				p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex || 
				p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing ||
				p.Status == FileIndexItem.ExifStatus.Deleted) && 
			    p.FilePath != "/" ).ToList();
		}
	}
}

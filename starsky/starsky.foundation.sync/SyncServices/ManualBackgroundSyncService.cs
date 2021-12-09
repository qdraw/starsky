using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
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

			_cache.Set(ManualSyncCacheName + subPath, true, 
				new TimeSpan(0,1,0));
			
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				await BackgroundTask(fileIndexItem.FilePath, operationId);
			});

			return FileIndexItem.ExifStatus.Ok;
		}

		internal async Task PushToSockets(List<FileIndexItem> updatedList)
		{
			await _connectionsService.SendToAllAsync(JsonSerializer.Serialize(
				updatedList,
				DefaultJsonSerializer.CamelCase), CancellationToken.None);
		}

		internal async Task BackgroundTask(string subPath, string operationId)
		{
			var operationHolder = RequestTelemetryHelper.GetOperationHolder(_scopeFactory,
				nameof(ManualSync), operationId);
			
			_logger.LogInformation($"[ManualBackgroundSyncService] start {subPath} " +
			                       $"{DateTime.Now.ToShortTimeString()}");
			
			var updatedList = await _synchronize.Sync(subPath, false, PushToSockets);
			
			_query.CacheUpdateItem(FilterBefore(updatedList));
			
			// so you can click on the button again
			_cache.Remove(ManualSyncCacheName + subPath);
			_logger.LogInformation($"[ManualBackgroundSyncService] done {subPath} " +
			                       $"{DateTime.Now.ToShortTimeString()}");
			operationHolder.SetData(updatedList);
		}
		
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

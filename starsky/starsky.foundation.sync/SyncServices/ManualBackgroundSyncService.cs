using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.worker.Services;

namespace starsky.foundation.sync.SyncServices
{
	[Service(typeof(IManualBackgroundSyncService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ManualBackgroundSyncService : IManualBackgroundSyncService
	{
		private readonly ISynchronize _synchronize;
		private readonly IQuery _query;
		private readonly IWebSocketConnectionsService _connectionsService;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IMemoryCache _cache;

		public ManualBackgroundSyncService(ISynchronize synchronize, IQuery query,
			IWebSocketConnectionsService connectionsService, 
			IBackgroundTaskQueue bgTaskQueue,
			IMemoryCache cache)
		{
			_synchronize = synchronize;
			_connectionsService = connectionsService;
			_bgTaskQueue = bgTaskQueue;
			_query = query;
			_cache = cache;
		}

		private const string QueryCacheName = "ManualSync_";
		
		public async Task<FileIndexItem.ExifStatus> ManualSync(string subPath)
		{
			var fileIndexItem = await _query.GetObjectByFilePathAsync(subPath);
			if ( fileIndexItem == null )
			{
				return FileIndexItem.ExifStatus.NotFoundNotInIndex;
			}

			if (_cache.TryGetValue(QueryCacheName + subPath, out _))
				return FileIndexItem.ExifStatus.OperationNotSupported;

			_cache.Set(QueryCacheName + subPath, true, 
				new TimeSpan(0,2,0));

			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				var updatedList = await _synchronize.Sync(fileIndexItem.FilePath, false);
				await _connectionsService.SendToAllAsync(JsonSerializer.Serialize(updatedList, 
					DefaultJsonSerializer.CamelCase), token);
				_cache.Remove(QueryCacheName + subPath);
			});

			return FileIndexItem.ExifStatus.Ok;
		}
	}
}

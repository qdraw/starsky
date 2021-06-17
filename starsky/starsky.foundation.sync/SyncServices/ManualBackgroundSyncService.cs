using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.SyncServices
{
	[Service(typeof(IManualBackgroundSyncService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ManualBackgroundSyncService : IManualBackgroundSyncService
	{
		private readonly ISynchronize _synchronize;
		private readonly IQuery _query;
		private readonly IWebSocketConnectionsService _connectionsService;
		private readonly IMemoryCache _cache;

		public ManualBackgroundSyncService(ISynchronize synchronize, IQuery query,
			IWebSocketConnectionsService connectionsService, 
			IMemoryCache cache)
		{
			_synchronize = synchronize;
			_connectionsService = connectionsService;
			_query = query;
			_cache = cache;
		}

		internal const string ManualSyncCacheName = "ManualSync_";
		
		public async Task<FileIndexItem.ExifStatus> ManualSync(string subPath)
		{
			var fileIndexItem = await _query.GetObjectByFilePathAsync(subPath);
			// on a new database ->
			if ( subPath == "/" && fileIndexItem == null) fileIndexItem = new FileIndexItem();
			if ( fileIndexItem == null )
			{
				return FileIndexItem.ExifStatus.NotFoundNotInIndex;
			}

			if (_cache.TryGetValue(ManualSyncCacheName + subPath, out _))
				return FileIndexItem.ExifStatus.OperationNotSupported;

			_cache.Set(ManualSyncCacheName + subPath, true, 
				new TimeSpan(0,2,0));
			
			await Task.Factory.StartNew(() => BackgroundTask(fileIndexItem.FilePath));

			return FileIndexItem.ExifStatus.Ok;
		}

		internal async Task PushToSockets(List<FileIndexItem> updatedList)
		{
			await _connectionsService.SendToAllAsync(JsonSerializer.Serialize(
				updatedList,
				DefaultJsonSerializer.CamelCase), CancellationToken.None);
		}

		internal async Task BackgroundTask(string subPath)
		{
			var updatedList = await _synchronize.Sync(subPath, false, PushToSockets);
			_query.CacheUpdateItem(FilterBefore(updatedList));
			
			// so you can click on the button again
			_cache.Remove(ManualSyncCacheName + subPath);
		}
		
		internal List<FileIndexItem> FilterBefore(IReadOnlyCollection<FileIndexItem> syncData)
		{
			return syncData.Where(p =>
				p.Status == FileIndexItem.ExifStatus.Ok ||
				p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex || 
				p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing ||
				p.Status == FileIndexItem.ExifStatus.Deleted).ToList();
		}
	}
}

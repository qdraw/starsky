using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.WatcherHelpers
{
	public class SyncWatcherConnector
	{
		private readonly ISynchronize _synchronize;
		private readonly AppSettings _appSettings;
		private readonly IWebSocketConnectionsService _websockets;
		private readonly IQuery _query;

		public SyncWatcherConnector(AppSettings appSettings, ISynchronize synchronize, 
			IWebSocketConnectionsService websockets, IQuery query)
		{
			_appSettings = appSettings;
			_synchronize = synchronize;
			_websockets = websockets;
			_query = query;
		}

		public SyncWatcherConnector(IServiceScopeFactory scopeFactory)
		{
			using var scope = scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			_synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>();
			_appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
			_websockets = scope.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
			_query = scope.ServiceProvider.GetRequiredService<IQuery>();
		}

		public async Task<List<FileIndexItem>> Sync(Tuple<string, string, WatcherChangeTypes> watcherOutput)
		{
			var (fullFilePath, toPath, type ) = watcherOutput;
			var syncData = new List<FileIndexItem>();
			if ( type == WatcherChangeTypes.Renamed && !string.IsNullOrEmpty(toPath))
			{
				await _synchronize.Sync(_appSettings.FullPathToDatabaseStyle(fullFilePath));
				syncData.Add(new FileIndexItem(_appSettings.FullPathToDatabaseStyle(fullFilePath))
				{
					IsDirectory = true, 
					Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
				});
				syncData.AddRange(await _synchronize.Sync(_appSettings.FullPathToDatabaseStyle(toPath)));
			}
			else
			{
				syncData = await _synchronize.Sync(_appSettings.FullPathToDatabaseStyle(fullFilePath));
			}

			var filtered = FilterBefore(syncData);
			if ( !filtered.Any() )
			{
				return syncData;
			}

			// update users who are active right now
			await _websockets.SendToAllAsync("[system] SyncWatcherConnector",CancellationToken.None);
			await _websockets.SendToAllAsync(JsonSerializer.Serialize(filtered,
				DefaultJsonSerializer.CamelCase), CancellationToken.None);
			
			// And update the query Cache
			_query.CacheUpdateItem(filtered.Where(p => p.Status == FileIndexItem.ExifStatus.Ok ||
				p.Status == FileIndexItem.ExifStatus.Deleted).ToList());
			
			// remove files that are not in the index from cache
			_query.RemoveCacheItem(filtered.Where(p => p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex || 
				p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList());
			
			return syncData;
		}

		internal List<FileIndexItem> FilterBefore(IReadOnlyCollection<FileIndexItem> syncData)
		{
			return syncData.Where(p =>
				p.Status == FileIndexItem.ExifStatus.Ok ||
				p.Status == FileIndexItem.ExifStatus.Deleted ||
				p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex || 
				p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList();
		}
	}
}

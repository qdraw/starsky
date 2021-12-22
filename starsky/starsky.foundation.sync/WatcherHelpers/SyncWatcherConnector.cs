using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.WatcherHelpers
{
	public class SyncWatcherConnector
	{
		private ISynchronize _synchronize;
		private AppSettings _appSettings;
		private IWebSocketConnectionsService _websockets;
		private IQuery _query;
		private IWebLogger _logger;
		private readonly IServiceScope _serviceScope;

		internal SyncWatcherConnector(AppSettings appSettings, ISynchronize synchronize, 
			IWebSocketConnectionsService websockets, IQuery query, IWebLogger logger)
		{
			_appSettings = appSettings;
			_synchronize = synchronize;
			_websockets = websockets;
			_query = query;
			_logger = logger;
		}

		public SyncWatcherConnector(IServiceScopeFactory scopeFactory)
		{
			_serviceScope = scopeFactory.CreateScope();
		}

		private void InjectScopes()
		{
			// ISynchronize is a scoped service
			_synchronize = _serviceScope.ServiceProvider.GetRequiredService<ISynchronize>();
			_appSettings = _serviceScope.ServiceProvider.GetRequiredService<AppSettings>();
			_websockets = _serviceScope.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
			_query = _serviceScope.ServiceProvider.GetRequiredService<IQuery>();
			_logger = _serviceScope.ServiceProvider.GetRequiredService<IWebLogger>();
		}

		public async Task<List<FileIndexItem>> Sync(Tuple<string, string, WatcherChangeTypes> watcherOutput)
		{
			// Avoid Disposed Query objects
			if ( _serviceScope != null ) InjectScopes();

			var (fullFilePath, toPath, type ) = watcherOutput;

			var syncData = new List<FileIndexItem>();
			
			_logger.LogInformation($"[SyncWatcherConnector] [{fullFilePath}] - [{toPath}] - [{type}]");
			
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

			await _query?.DisposeAsync();

			return syncData;
		}

		internal List<FileIndexItem> FilterBefore(IReadOnlyCollection<FileIndexItem> syncData)
		{
			// also remove duplicates from output list
			return syncData.GroupBy(x => x.FilePath).
				Select(x => x.First())
				.Where(p =>
				p.Status == FileIndexItem.ExifStatus.Ok ||
				p.Status == FileIndexItem.ExifStatus.Deleted ||
				p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex || 
				p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList();
		}
	}
}

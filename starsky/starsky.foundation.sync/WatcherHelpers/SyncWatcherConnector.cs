using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

		public SyncWatcherConnector(AppSettings appSettings, ISynchronize synchronize, 
			IWebSocketConnectionsService websockets)
		{
			_appSettings = appSettings;
			_synchronize = synchronize;
			_websockets = websockets;
		}

		public SyncWatcherConnector(IServiceScopeFactory scopeFactory)
		{
			using var scope = scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			_synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>();
			_appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
			_websockets = scope.ServiceProvider.GetRequiredService<IWebSocketConnectionsService>();
		}

		public async Task<List<FileIndexItem>> Sync(Tuple<string, WatcherChangeTypes> watcherOutput)
		{
			var (fullFilePath,_ ) = watcherOutput;
			var syncData = await _synchronize.Sync(_appSettings.FullPathToDatabaseStyle(fullFilePath));
			await FilterBeforeSocket(syncData);
			return syncData;
		}

		private async Task FilterBeforeSocket(IReadOnlyCollection<FileIndexItem> syncData)
		{
			var fileIndexItems = syncData.Where(p =>
				p.Status == FileIndexItem.ExifStatus.Ok ||
				p.Status == FileIndexItem.ExifStatus.NotFoundNotInIndex || 
				p.Status == FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList();
			if ( !fileIndexItems.Any() ) return;

			await _websockets.SendToAllAsync(JsonSerializer.Serialize(fileIndexItems,
				DefaultJsonSerializer.CamelCase), CancellationToken.None);
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.SyncInterfaces;

namespace starsky.foundation.sync.WatcherServices
{
	public class SyncWatcherPreflight
	{
		private readonly ISynchronize _synchronize;
		private readonly AppSettings _appSettings;

		public SyncWatcherPreflight(AppSettings appSettings, ISynchronize synchronize)
		{
			_appSettings = appSettings;
			_synchronize = synchronize;
		}

		public SyncWatcherPreflight(IServiceScopeFactory scopeFactory)
		{
			using var scope = scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			_synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>();
			_appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
		}

		public Task<List<FileIndexItem>> Sync(Tuple<string, WatcherChangeTypes> watcherOutput)
		{
			var (fullFilePath,_ ) = watcherOutput;
			return _synchronize.Sync(_appSettings.FullPathToDatabaseStyle(fullFilePath));
		}
	}
}

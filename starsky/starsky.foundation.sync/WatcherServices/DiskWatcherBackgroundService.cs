using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherInterfaces;

namespace starsky.foundation.sync.Services
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class DiskWatcherBackgroundService : BackgroundService
	{
		private readonly IDiskWatcher _diskWatcher;
		private readonly AppSettings _appSettings;

		public DiskWatcherBackgroundService(IDiskWatcher diskWatcher, AppSettings appSettings)
		{
			_diskWatcher = diskWatcher;
			_appSettings = appSettings;
		}
		
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_diskWatcher.Watcher(_appSettings.StorageFolder);
			return Task.CompletedTask;
		}
	}
}

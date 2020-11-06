using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.sync.Interfaces;

namespace starsky.foundation.sync.Services
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class DiskWatcherBackgroundService : BackgroundService
	{
		private readonly IDiskWatcher _diskWatcher;

		public DiskWatcherBackgroundService(IDiskWatcher diskWatcher)
		{
			_diskWatcher = diskWatcher;
		}
		
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Console.WriteLine("--> DiskWatcherBackgroundService");
			_diskWatcher.Watcher("/data/scripts");
			return Task.CompletedTask;
		}
	}
}

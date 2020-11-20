using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherInterfaces;

namespace starsky.foundation.sync.WatcherServices
{
	/// <summary>
	/// Run DiskWatcher as singleton (once) in the background of the app
	/// </summary>
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
		
		/// <summary>
		/// This method is triggered by BackgroundService
		/// </summary>
		/// <param name="stoppingToken">ignored but required</param>
		/// <returns>Task/nothing</returns>
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Console.WriteLine(_appSettings.UseDiskWatcher ? "UseDiskWatcher is enabled" : "UseDiskWatcher is disabled");
			if ( !_appSettings.UseDiskWatcher ) return Task.CompletedTask;

			_diskWatcher.Watcher(_appSettings.StorageFolder);
			return Task.CompletedTask;
		}
	}
}

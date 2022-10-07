using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Helpers;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.sync.WatcherBackgroundService
{
	[Service(typeof(IHostedService),
		InjectionLifetime = InjectionLifetime.Singleton)]
	public class DiskWatcherQueuedHostedService : BackgroundService
	{
		private readonly IDiskWatcherBackgroundTaskQueue _taskQueue;
		private readonly IWebLogger _logger;
		private readonly AppSettings _appSettings;


		
		public DiskWatcherQueuedHostedService(
			IDiskWatcherBackgroundTaskQueue taskQueue,
			IWebLogger logger, AppSettings appSettings) =>
			(_taskQueue, _logger, _appSettings) = (taskQueue, logger, appSettings);

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Queued Hosted Service for DiskWatcher");
			await ProcessTaskQueue.ProcessBatchedLoopAsync(_taskQueue, _logger,
				_appSettings, stoppingToken);
		}

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				$"QueuedHostedService {_taskQueue.GetType().Name} is stopping.");
			await base.StopAsync(stoppingToken);
		}
	}
}

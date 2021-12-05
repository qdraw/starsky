using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.sync.WatcherInterfaces;
using starsky.foundation.webtelemetry.Interfaces;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;

namespace starsky.foundation.sync.WatcherBackgroundService
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class DiskWatcherQueuedHostedService : BackgroundService
	{
		private readonly BackgroundQueuedHostedService _service;
		private readonly IWebLogger _logger;

		public DiskWatcherQueuedHostedService(DiskWatcherBackgroundTaskQueue taskQueue,
			IWebLogger logger, ITelemetryService telemetryService)
		{
			TaskQueue = taskQueue;
			_logger = logger;
			_service = new BackgroundQueuedHostedService(TaskQueue, logger,
				telemetryService);
		}

		private IBackgroundTaskQueue TaskQueue { get; set; }

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation($"Queued Hosted Service {GetType().Name} is " +
			                       $"starting on {Environment.MachineName}");
			await _service.ProcessTaskQueueAsync(stoppingToken);
		}
	}
}

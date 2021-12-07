using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.webtelemetry.Interfaces;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;

namespace starsky.foundation.sync.WatcherBackgroundService
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class DiskWatcherQueuedHostedService : BackgroundQueuedHostedService
	{
		public DiskWatcherQueuedHostedService(IBackgroundTaskQueue taskQueue, IWebLogger logger, 
			ITelemetryService telemetryService) : base(taskQueue, logger, telemetryService)
		{
		}

		// private readonly IWebLogger _logger;
		// private readonly ITelemetryService _telemetryService;
		// public DiskWatcherQueuedHostedService(IBackgroundTaskQueue taskQueue,
		// 	IWebLogger logger, ITelemetryService telemetryService)
		// {
		// 	TaskQueue = taskQueue;
		// 	_logger = logger;
		// 	_telemetryService = telemetryService;
		// }
		//
		// private IBackgroundTaskQueue TaskQueue { get; set; }
		//
		// protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		// {
		// 	_logger.LogInformation($"Queued Hosted Service {GetType().Name} is " +
		// 	                       $"starting on {Environment.MachineName}");
		// 	
		// 	await new BackgroundQueuedHostedService(TaskQueue, _logger,
		// 		_telemetryService).ProcessTaskQueueAsync(stoppingToken);
		// }

	}
}

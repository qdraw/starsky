using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.webtelemetry.Interfaces;
using starsky.foundation.worker.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.worker.Services
{
	[Service(typeof(IHostedService),
		InjectionLifetime = InjectionLifetime.Singleton)]
	public class BackgroundQueuedHostedService : BackgroundService
	{
		private readonly ITelemetryService _telemetryService;
		private readonly IWebLogger _logger;

		public BackgroundQueuedHostedService(IBackgroundTaskQueue taskQueue,
			IWebLogger logger, ITelemetryService telemetryService)
		{
			TaskQueue = taskQueue;
			_telemetryService = telemetryService;
			_logger = logger;
		}

		private IBackgroundTaskQueue TaskQueue { get; }
		
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
	        _logger.LogInformation($"Queued Hosted Service {GetType().Name} is " +
	                               $"starting on {Environment.MachineName}");
	        return ProcessTaskQueueAsync(stoppingToken);
        }
		
		private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var workItem = await TaskQueue.DequeueAsync(stoppingToken);
				try
				{
					await workItem(stoppingToken);
				}
				catch (Exception exception)
				{
					_logger.LogError(exception,  
						$"Error occurred executing work item " +
						$"{nameof(workItem)}.", nameof(workItem));
					_telemetryService.TrackException(exception);
				}
			}
			_logger.LogInformation("Queued Hosted Service is stopping.");
		}
		
		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation($"{nameof(BackgroundQueuedHostedService)} is stopping.");
			await base.StopAsync(stoppingToken);
		}
    }
}

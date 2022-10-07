#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.worker.Services
{
	[Service(typeof(IHostedService),
		InjectionLifetime = InjectionLifetime.Singleton)]
	public class UpdateBackgroundQueuedHostedService : BackgroundService
	{
		private readonly IUpdateBackgroundTaskQueue _taskQueue;
		private readonly IWebLogger _logger;

		public UpdateBackgroundQueuedHostedService(
			IUpdateBackgroundTaskQueue taskQueue,
			IWebLogger logger) =>
			(_taskQueue, _logger) = (taskQueue, logger);

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return ProcessTaskQueueAsync(stoppingToken);
		}

		private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var workItem =
						await _taskQueue.DequeueAsync(stoppingToken);

					await workItem(stoppingToken);
				}
				catch (OperationCanceledException)
				{
					// Prevent throwing if stoppingToken was signaled
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred executing task work item.");
				}
			}
		}

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				$"QueuedHostedService is stopping.");
			await base.StopAsync(stoppingToken);
		}
	}
}

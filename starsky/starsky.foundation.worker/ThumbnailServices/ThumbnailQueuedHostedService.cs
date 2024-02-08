using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.worker.ThumbnailServices
{
	[Service(typeof(IHostedService),
		InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class ThumbnailQueuedHostedService : BackgroundService
	{
		private readonly IThumbnailQueuedHostedService _taskQueue;
		private readonly IWebLogger _logger;

		public ThumbnailQueuedHostedService(
			IThumbnailQueuedHostedService taskQueue,
			IWebLogger logger, AppSettings appSettings) =>
			(_taskQueue, _logger, _) = (taskQueue, logger, appSettings);

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Queued Hosted Service for Thumbnails");
			await ProcessTaskQueue.ProcessTaskQueueAsync(_taskQueue, _logger,
				stoppingToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				$"QueuedHostedService {_taskQueue.GetType().Name} is stopping.");
			await base.StopAsync(cancellationToken);
		}
	}
}


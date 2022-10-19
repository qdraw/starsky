#nullable enable
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.worker.Services
{
	[Service(typeof(IHostedService),
		InjectionLifetime = InjectionLifetime.Singleton)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "S927: Rename parameter 'stoppingToken' " +
		"to 'cancellationToken' to match the base class declaration", Justification = "Is checked")]
	public sealed class UpdateBackgroundQueuedHostedService : BackgroundService
	{
		private readonly IUpdateBackgroundTaskQueue _taskQueue;
		private readonly IWebLogger _logger;

		public UpdateBackgroundQueuedHostedService(
			IUpdateBackgroundTaskQueue taskQueue,
			IWebLogger logger) =>
			(_taskQueue, _logger) = (taskQueue, logger);

		protected override Task ExecuteAsync(CancellationToken cancellationToken)
		{
			return ProcessTaskQueue.ProcessTaskQueueAsync(_taskQueue, _logger, cancellationToken);
		}

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				$"QueuedHostedService {_taskQueue.GetType().Name} is stopping.");
			await base.StopAsync(stoppingToken);
		}
	}
}

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Helpers;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.sync.WatcherBackgroundService;

[Service(typeof(IHostedService),
	InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class DiskWatcherQueuedHostedService : BackgroundService
{
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;
	private readonly IDiskWatcherBackgroundTaskQueue _taskQueue;

	public DiskWatcherQueuedHostedService(
		IDiskWatcherBackgroundTaskQueue taskQueue,
		IWebLogger logger, AppSettings appSettings)
	{
		( _taskQueue, _logger, _appSettings ) = ( taskQueue, logger, appSettings );
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Queued Hosted Service for DiskWatcher");
		await ProcessTaskQueue.ProcessBatchedLoopAsync(_taskQueue, _logger,
			_appSettings, stoppingToken);
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			$"QueuedHostedService {_taskQueue.GetType().Name} is stopping. Counts: {_taskQueue.Count()}");
		await base.StopAsync(cancellationToken);
	}
}

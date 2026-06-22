using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.imageclassification.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Helpers;

namespace starsky.foundation.imageclassification.Services;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class ImageClassificationQueuedHostedService : BackgroundService
{
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IImageClassificationBackgroundTaskQueue _taskQueue;

	public ImageClassificationQueuedHostedService(
		IImageClassificationBackgroundTaskQueue taskQueue,
		IWebLogger logger,
		IServiceScopeFactory scopeFactory)
	{
		_taskQueue = taskQueue;
		_logger = logger;
		_scopeFactory = scopeFactory;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		return ProcessTaskQueue.ProcessTaskQueueAsync(_taskQueue, _logger,
			stoppingToken, _scopeFactory);
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			$"QueuedHostedService {_taskQueue.GetType().Name} is stopping. Counts: {_taskQueue.Count()}");
		await base.StopAsync(cancellationToken);
	}
}


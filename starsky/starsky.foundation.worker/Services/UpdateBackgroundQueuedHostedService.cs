using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.worker.Services;

[Service(typeof(IHostedService),
	InjectionLifetime = InjectionLifetime.Singleton)]
[SuppressMessage("Usage", "S927: Rename parameter 'stoppingToken' " +
                          "to 'cancellationToken' to match the base class declaration",
	Justification = "Is checked")]
public sealed class UpdateBackgroundQueuedHostedService(
	IUpdateBackgroundTaskQueue taskQueue,
	IWebLogger logger,
	IServiceScopeFactory scopeFactory)
	: BackgroundService
{
	protected override Task ExecuteAsync(CancellationToken cancellationToken)
	{
		return ProcessTaskQueue.ProcessTaskQueueAsync(taskQueue, logger,
			cancellationToken, scopeFactory);
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation(
			$"QueuedHostedService {taskQueue.GetType().Name} is stopping. Counts: {taskQueue.Count()}");
		await base.StopAsync(stoppingToken);
	}
}

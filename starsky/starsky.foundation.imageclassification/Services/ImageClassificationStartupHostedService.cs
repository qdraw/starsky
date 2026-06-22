using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.imageclassification.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.imageclassification.Services;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class ImageClassificationStartupHostedService(
	IServiceScopeFactory scopeFactory,
	AppSettings appSettings,
	IWebLogger logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if ( appSettings.UseImageClassificationOnStartup != true )
		{
			return;
		}

		if ( appSettings.ApplicationType != AppSettings.StarskyAppType.WebController )
		{
			return;
		}

		using var scope = scopeFactory.CreateScope();
		var batchRunner = scope.ServiceProvider.GetRequiredService<IImageClassificationBatchRunner>();
		var amount = await batchRunner.EnqueueBatchAsync(stoppingToken);
		logger.LogInformation(
			$"[ImageClassificationStartupHostedService] queued {amount} image-classification jobs on startup");
	}
}


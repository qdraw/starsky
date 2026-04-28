using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.imageclassification.Interfaces;
using starsky.foundation.imageclassification.Services;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.worker.Models;

namespace starskyimageclassificationcli;

public static class Program
{
	public static async Task Main(string[] args)
	{
		new ArgsHelper().SetEnvironmentByArgs(args);

		var hostBuilder = Host.CreateDefaultBuilder(args)
			.ConfigureServices((_, services) =>
			{
				var configurationRoot = SetupAppSettings.AppSettingsToBuilder(args).Result;
				services.AddSingleton<IConfiguration>(configurationRoot);
				services.ConfigurePoCo<AppSettings>(configurationRoot.GetSection("App"));

				RegisterDependencies.Configure(services);

				var tempServiceProvider = services.BuildServiceProvider();
				var appSettings = tempServiceProvider.GetRequiredService<AppSettings>();
				appSettings.ApplicationType = AppSettings.StarskyAppType.ImageClassification;
				appSettings.Queue.Queues[QueueNames.ImageClassification] = QueueBackendType.Database;

				services.AddOpenTelemetryMonitoring(appSettings);
				services.AddTelemetryLogging(appSettings);
				new SetupDatabaseTypes(appSettings, services).BuilderDb();

				services.RemoveAll(typeof(IHostedService));
				services.AddSingleton<IHostedService, ImageClassificationQueuedHostedService>();
				services.AddHostedService<ImageClassificationCliPumpHostedService>();
			});

		var host = hostBuilder.Build();
		using ( var scope = host.Services.CreateScope() )
		{
			var provider = scope.ServiceProvider;
			var logger = provider.GetRequiredService<IWebLogger>();
			var appSettings = provider.GetRequiredService<AppSettings>();
			await RunMigrations.Run(provider.GetRequiredService<ApplicationDbContext>(), logger,
				appSettings);
		}

		await host.RunAsync();
	}
}

public sealed class ImageClassificationCliPumpHostedService(
	IServiceScopeFactory scopeFactory,
	AppSettings appSettings,
	IWebLogger logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("ImageClassification CLI worker started");
		while ( !stoppingToken.IsCancellationRequested )
		{
			using var scope = scopeFactory.CreateScope();
			var batchRunner = scope.ServiceProvider
				.GetRequiredService<IImageClassificationBatchRunner>();
			var queued = await batchRunner.EnqueueBatchAsync(stoppingToken);
			logger.LogInformation(
				$"[ImageClassificationCliPumpHostedService] queued {queued} items");

			var delay = TimeSpan.FromSeconds(Math.Max(15, appSettings.Queue.DatabasePollIntervalInMilliseconds / 10.0));
			await Task.Delay(delay, stoppingToken);
		}
	}
}



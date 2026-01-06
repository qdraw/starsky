using Microsoft.Extensions.Hosting;
using starsky.foundation.cloudimport.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.cloudimport.Services;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public class CloudImportScheduledService(
	ICloudImportService cloudImportService,
	IWebLogger logger,
	AppSettings appSettings)
	: BackgroundService
{
	private readonly Dictionary<string, Task> _providerTasks = new();

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await RunAsync(stoppingToken);
	}

	public async Task<bool?> RunAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Cloud Sync Scheduled Service started");

		var enabledProviders =
			appSettings.CloudImport?.Providers.Where(p => p.Enabled).ToList() ?? [];

		if ( enabledProviders.Count == 0 )
		{
			logger.LogInformation(
				"No enabled cloud sync providers found, scheduled service will not run");
			return false;
		}

		logger.LogInformation($"Starting scheduled sync for {enabledProviders.Count} provider(s)");

		// Start a task for each enabled provider
		foreach ( var provider in enabledProviders )
		{
			var task = RunProviderSyncAsync(provider, stoppingToken);
			_providerTasks[provider.Id] = task;
		}

		// Wait for all provider tasks to complete
		await Task.WhenAll(_providerTasks.Values);

		logger.LogInformation("Cloud Sync Scheduled Service stopped");
		return true;
	}

	private async Task RunProviderSyncAsync(CloudImportProviderSettings provider,
		CancellationToken stoppingToken)
	{
		logger.LogInformation(
			$"Starting scheduled sync task for provider '{provider.Id}' ({provider.Provider})");

		while ( !stoppingToken.IsCancellationRequested )
		{
			try
			{
				var delay = GetNextDelay(provider);
				logger.LogInformation(
					$"Next cloud sync for provider '{provider.Id}' will run in {delay.TotalMinutes:F1} minutes");

				await Task.Delay(delay, stoppingToken);

				if ( stoppingToken.IsCancellationRequested )
				{
					break;
				}

				logger.LogInformation(
					$"Starting scheduled cloud sync for provider '{provider.Id}'");
				await cloudImportService.SyncAsync(provider.Id, CloudImportTriggerType.Scheduled);
			}
			catch ( TaskCanceledException )
			{
				break;
			}
			catch ( Exception ex )
			{
				logger.LogError(ex,
					$"Error during scheduled cloud sync for provider '{provider.Id}'");
				// Wait a bit before retrying after error
				try
				{
					await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
				}
				catch ( TaskCanceledException )
				{
					break;
				}
			}
		}

		logger.LogInformation(
			$"Scheduled sync task for provider '{provider.Id}' has stopped");
	}

	private static TimeSpan GetNextDelay(CloudImportProviderSettings provider)
	{
		if ( provider.SyncFrequencyMinutes > 0 )
		{
			return TimeSpan.FromMinutes(provider.SyncFrequencyMinutes);
		}

		if ( provider.SyncFrequencyHours > 0 )
		{
			return TimeSpan.FromHours(provider.SyncFrequencyHours);
		}

		return TimeSpan.FromHours(24);
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Cloud Sync Scheduled Service is stopping");
		return base.StopAsync(cancellationToken);
	}
}

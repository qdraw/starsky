using Microsoft.Extensions.Hosting;
using starsky.foundation.cloudsync.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.cloudsync.Services;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public class CloudSyncScheduledService(
	ICloudSyncService cloudSyncService,
	IWebLogger logger,
	CloudSyncSettings settings)
	: BackgroundService
{
	private readonly Dictionary<string, Task> _providerTasks = new();

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Cloud Sync Scheduled Service started");

		var enabledProviders = settings.Providers.Where(p => p.Enabled).ToList();

		if ( !enabledProviders.Any() )
		{
			logger.LogInformation(
				"No enabled cloud sync providers found, scheduled service will not run");
			return;
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
	}

	private async Task RunProviderSyncAsync(CloudSyncProviderSettings provider,
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

				if ( !stoppingToken.IsCancellationRequested )
				{
					logger.LogInformation(
						$"Starting scheduled cloud sync for provider '{provider.Id}'");
					await cloudSyncService.SyncAsync(provider.Id, CloudSyncTriggerType.Scheduled);
				}
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

	private TimeSpan GetNextDelay(CloudSyncProviderSettings provider)
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

	public override Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Cloud Sync Scheduled Service is stopping");
		return base.StopAsync(stoppingToken);
	}
}

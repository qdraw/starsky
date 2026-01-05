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
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Cloud Sync Scheduled Service started");

		if ( !settings.Enabled )
		{
			logger.LogInformation("Cloud sync is disabled, scheduled service will not run");
			return;
		}

		while ( !stoppingToken.IsCancellationRequested )
		{
			try
			{
				var delay = GetNextDelay();
				logger.LogInformation(
					$"Next cloud sync will run in {delay.TotalMinutes:F1} minutes");

				await Task.Delay(delay, stoppingToken);

				if ( !stoppingToken.IsCancellationRequested )
				{
					logger.LogInformation("Starting scheduled cloud sync");
					await cloudSyncService.SyncAsync(CloudSyncTriggerType.Scheduled);
				}
			}
			catch ( TaskCanceledException )
			{
				break;
			}
			catch ( Exception ex )
			{
				logger.LogError(ex, "Error during scheduled cloud sync");
				// Wait a bit before retrying after error
				await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
			}
		}

		logger.LogInformation("Cloud Sync Scheduled Service stopped");
	}

	private TimeSpan GetNextDelay()
	{
		if ( settings.SyncFrequencyMinutes > 0 )
		{
			return TimeSpan.FromMinutes(settings.SyncFrequencyMinutes);
		}

		if ( settings.SyncFrequencyHours > 0 )
		{
			return TimeSpan.FromHours(settings.SyncFrequencyHours);
		}

		return TimeSpan.FromHours(24);
	}

	public override Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Cloud Sync Scheduled Service is stopping");
		return base.StopAsync(stoppingToken);
	}
}

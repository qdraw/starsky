using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using starsky.feature.cloudimport.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.cloudimport.Services;

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
		logger.LogInformation("Cloud Import Scheduled Service started");

		var enabledProviders =
			appSettings.CloudImport?.Providers.Where(p => p.Enabled &&
			                                              ( p.SyncFrequencyHours > 0 ||
			                                                p.SyncFrequencyMinutes > 0 ))
				.ToList() ?? [];

		if ( enabledProviders.Count == 0 )
		{
			logger.LogInformation(
				"No enabled Cloud Import providers found, scheduled service will not run");
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

		logger.LogInformation("Cloud Import Scheduled Service stopped");
		return true;
	}

	internal async Task RunProviderSyncAsync(CloudImportProviderSettings provider,
		CancellationToken stoppingToken)
	{
		logger.LogInformation(
			$"Starting scheduled sync task for provider '{provider.Id}' ({provider.Provider})");

		while ( !stoppingToken.IsCancellationRequested )
		{
			try
			{
				if ( await RunProviderSyncSingleAsync(provider, stoppingToken) )
				{
					break;
				}
			}
			catch ( TaskCanceledException )
			{
				break;
			}
			catch ( Exception ex )
			{
				logger.LogError(ex,
					$"Error during scheduled Cloud Import for provider '{provider.Id}'");
				try
				{
					await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
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

	internal async Task<bool> RunProviderSyncSingleAsync(CloudImportProviderSettings provider,
		CancellationToken stoppingToken)
	{
		var delay = GetNextDelay(provider);
		if ( delay >= TimeSpan.FromHours(150) )
		{
			return true;
		}

		logger.LogInformation(
			$"Next Cloud Import for provider '{provider.Id}' will run in {delay.TotalMinutes:F1} minutes");

		try
		{
			await Task.Delay(delay, stoppingToken);
		}
		catch (OperationCanceledException)
		{
			return true;
		}

		if ( stoppingToken.IsCancellationRequested )
		{
			return true;
		}

		logger.LogInformation(
			$"Starting scheduled Cloud Import for provider '{provider.Id}'");
		await cloudImportService.SyncAsync(provider.Id, CloudImportTriggerType.Scheduled);
		return false;
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

		return TimeSpan.FromHours(150);
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Cloud Import Scheduled Service is stopping");
		return base.StopAsync(cancellationToken);
	}
}

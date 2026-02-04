using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Formats;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IHostedService),
	InjectionLifetime = InjectionLifetime.Singleton)]
public class CleanThumbnailHostedService : BackgroundService
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public CleanThumbnailHostedService(IServiceScopeFactory serviceScopeFactory)
	{
		_serviceScopeFactory = serviceScopeFactory;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// why Task.Yield -> https://medium.com/@thepen0411/how-to-resolve-the-net-background-service-blocking-issue-c96086de8acd
		await Task.Yield();
		await StartBackgroundAsync(TimeSpan.FromMinutes(15), stoppingToken);
	}

	internal async Task<List<string>> StartBackgroundAsync(TimeSpan delay,
		CancellationToken cancellationToken)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
		var settingService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
		if ( !await ContinueDueSettings(appSettings, settingService) )
		{
			return [];
		}

		await Task.Delay(delay, cancellationToken);

		var thumbnailCleaner = scope.ServiceProvider.GetRequiredService<IThumbnailCleaner>();
		return await thumbnailCleaner.CleanAllUnusedFilesAsync();
	}

	internal static async Task<bool> ContinueDueSettings(AppSettings appSettings,
		ISettingsService settingService)
	{
		if ( appSettings.ThumbnailCleanupSkipOnStartup == true )
		{
			return false;
		}

		var lastRun = await settingService.GetSetting<DateTime?>(
			SettingsType.CleanUpThumbnailDatabaseLastRun);
		var shouldBeLaterThan = DateTime.UtcNow.AddDays(-4);
		var continueRun = !lastRun.HasValue || lastRun.Value <= shouldBeLaterThan;

		await settingService.AddOrUpdateSetting(SettingsType.CleanUpThumbnailDatabaseLastRun,
			DateTime.UtcNow.ToString(
				DateTime.UtcNow.ToDefaultSettingsFormat()));

		return continueRun;
	}
}

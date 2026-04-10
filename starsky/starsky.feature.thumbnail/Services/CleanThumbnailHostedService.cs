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
public class CleanThumbnailHostedService(
	IServiceScopeFactory serviceScopeFactory,
	IHostApplicationLifetime hostApplicationLifetime)
	: IHostedService
{
	private CancellationTokenSource? _runCancellationTokenSource;
	private IDisposable? _applicationStartedRegistration;
	private Task? _runningTask;
	internal TimeSpan StartupDelay { get; set; } = TimeSpan.FromMinutes(15);

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_runCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
			cancellationToken);

		// Run cleanup only after app startup and do not block startup pipeline.
		_applicationStartedRegistration = hostApplicationLifetime.ApplicationStarted.Register(() =>
		{
			var token = _runCancellationTokenSource.Token;
			_runningTask = Task.Run(async () =>
			{
				try
				{
					await StartBackgroundAsync(StartupDelay, token);
				}
				catch ( OperationCanceledException ) when ( token.IsCancellationRequested )
				{
					// Expected when service is stopping.
				}
				catch
				{
					// Suppress background exceptions to keep host startup unaffected.
				}
			}, CancellationToken.None);
		});

		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		_applicationStartedRegistration?.Dispose();
		if ( _runCancellationTokenSource != null )
		{
			await _runCancellationTokenSource.CancelAsync();
		}

		if ( _runningTask != null )
		{
			try
			{
				await _runningTask.WaitAsync(cancellationToken);
			}
			catch ( OperationCanceledException )
			{
				// Stop cancellation requested.
			}
		}

		_runCancellationTokenSource?.Dispose();
	}

	internal async Task<List<string>> StartBackgroundAsync(TimeSpan delay,
		CancellationToken cancellationToken)
	{
		using var scope = serviceScopeFactory.CreateScope();
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

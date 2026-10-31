using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.settings.Enums;
using starsky.foundation.settings.Formats;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IHostedService),
	InjectionLifetime = InjectionLifetime.Singleton)]
public class CleanThumbnailHostedService(
	IServiceScopeFactory serviceScopeFactory,
	IHostApplicationLifetime hostApplicationLifetime,
	ITenantRegistry? tenantRegistry = null)
	: IHostedService
{
	private CancellationTokenRegistration? _applicationStartedRegistration;
	private CancellationTokenSource? _runCancellationTokenSource;
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

	[SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
	[SuppressMessage("Usage", "S6966: Await DisposeAsync instead")]
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
		var registry = tenantRegistry ?? new TenantRegistry();

		// the last run is a setting of the tenant: ask each tenant if it is due
		var dueTenants = await TenantJobs.SelectForTenantsAsync(serviceScopeFactory,
			registry.ActiveTenantIds.ToArray(),
			async (serviceProvider, tenantId) =>
			{
				var appSettings = serviceProvider.GetRequiredService<AppSettings>();
				var settingService = serviceProvider.GetRequiredService<ISettingsService>();
				return await ContinueDueSettings(appSettings, settingService) ? tenantId : null;
			}, cancellationToken: cancellationToken);

		if ( dueTenants.Count == 0 )
		{
			return [];
		}

		await Task.Delay(delay, cancellationToken);

		var cleaned = await TenantJobs.SelectForTenantsAsync(serviceScopeFactory, dueTenants,
			async (serviceProvider, _) =>
			{
				var thumbnailCleaner = serviceProvider.GetRequiredService<IThumbnailCleaner>();
				return await thumbnailCleaner.CleanAllUnusedFilesAsync();
			}, cancellationToken: cancellationToken);
		return cleaned.SelectMany(p => p).ToList();
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

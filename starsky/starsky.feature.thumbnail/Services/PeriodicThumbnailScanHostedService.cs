using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.thumbnail.Services;

/// <summary>
///     @see:
///     https://medium.com/medialesson/run-and-manage-periodic-background-tasks-in-asp-net-core-6-with-c-578a31f4b7a3
/// </summary>
[Service(typeof(IHostedService),
	InjectionLifetime = InjectionLifetime.Singleton)]
public class PeriodicThumbnailScanHostedService : IHostedService
{
	private readonly IServiceScopeFactory _factory;
	private readonly IHostApplicationLifetime _hostApplicationLifetime;
	private readonly IWebLogger _logger;
	private readonly ITenantRegistry _tenantRegistry;
	private readonly CancellationTokenSource _stopCts = new();
	private int _executionCount;
	private Task? _runTask;

	public PeriodicThumbnailScanHostedService(AppSettings appSettings,
		IWebLogger logger,
		IServiceScopeFactory factory,
		IHostApplicationLifetime hostApplicationLifetime,
		ITenantRegistry? tenantRegistry = null)
	{
		_tenantRegistry = tenantRegistry ?? new TenantRegistry();
		_logger = logger;
		_factory = factory;
		_hostApplicationLifetime = hostApplicationLifetime;

		if ( appSettings.ThumbnailGenerationIntervalInMinutes >= MinimumIntervalInMinutes )
		{
			Period = TimeSpan.FromMinutes(appSettings
				.ThumbnailGenerationIntervalInMinutes.Value);
			IsEnabled = true;
			return;
		}

		Period = TimeSpan.FromMinutes(60);
	}

	internal TimeSpan Period { get; set; }

	internal int MinimumIntervalInMinutes { get; set; } = 3;

	internal bool IsEnabled { get; set; }

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_runTask = StartBackgroundAsync(false, _stopCts.Token);

		_hostApplicationLifetime.ApplicationStarted.Register(() =>
		{
			// Run as background app
			_ = RunStartupJobAsync();
		});

		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await _stopCts.CancelAsync();
		if ( _runTask != null )
		{
			await _runTask.ConfigureAwait(false);
		}

		_stopCts.Dispose();
	}

	private async Task RunStartupJobAsync()
	{
		try
		{
			await RunJob(_stopCts.Token);
		}
		catch ( OperationCanceledException )
		{
			// Expected when the host is shutting down
		}
		catch ( Exception exception )
		{
			_logger.LogError(
				$"Failed startup execution in {nameof(PeriodicThumbnailScanHostedService)} with {exception.Message}",
				exception);
		}
	}

	internal async Task<bool?> StartBackgroundAsync(bool startDirect,
		CancellationToken cancellationToken)
	{
		if ( startDirect )
		{
			await RunJob(cancellationToken);
		}

		if ( !IsEnabled )
		{
			return false;
		}

		try
		{
			using var timer = new PeriodicTimer(Period);
			while (
				!cancellationToken.IsCancellationRequested &&
				await timer.WaitForNextTickAsync(cancellationToken) )
			{
				await RunJob(cancellationToken);
			}
		}
		catch ( OperationCanceledException )
		{
			// Expected when the host is shutting down
		}

		return null;
	}

	internal async Task<bool?> RunJob(CancellationToken cancellationToken = default)
	{
		if ( !IsEnabled )
		{
			_logger.LogInformation(
				$"Skipped {nameof(PeriodicThumbnailScanHostedService)}");
			return false;
		}

		cancellationToken.ThrowIfCancellationRequested();

		try
		{
			// each tenant has its own index and thumbnail folder
			var failures = await TenantJobs.ForEachActiveTenantAsync(_factory, _tenantRegistry,
				async (serviceProvider, _) =>
				{
					var service = serviceProvider
						.GetRequiredService<IDatabaseThumbnailGenerationService>();
					await service.StartBackgroundQueue();
				}, (tenantId, exception) =>
				{
					_logger.LogError(
						$"Failed to execute {nameof(PeriodicThumbnailScanHostedService)} " +
						$"for tenant {tenantId} with exception message {exception.Message} - " +
						$"{exception.StackTrace} - {exception.InnerException?.StackTrace}",
						exception);
				}, cancellationToken);

			if ( failures.Count > 0 )
			{
				return null;
			}

			_executionCount++;
			// Executed PeriodicThumbnailScanHostedService
			_logger.LogDebug(
				$"Executed {nameof(PeriodicThumbnailScanHostedService)} -" +
				$" Count: {_executionCount} ({DateTime.UtcNow:HH:mm:ss})");
			return true;
		}
		catch ( Exception exception )
		{
			_logger.LogError(
				$"Failed to execute {nameof(PeriodicThumbnailScanHostedService)} " +
				$"with exception message {exception.Message} - {exception.StackTrace} - " +
				$"{exception.InnerException?.StackTrace}", exception);
		}

		return null;
	}
}

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

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.thumbnail.Services;

/// <summary>
/// @see: https://medium.com/medialesson/run-and-manage-periodic-background-tasks-in-asp-net-core-6-with-c-578a31f4b7a3
/// </summary>
[Service(typeof(IHostedService),
	InjectionLifetime = InjectionLifetime.Singleton)]
public class PeriodicThumbnailScanHostedService : BackgroundService
{
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _factory;
	private int _executionCount;

	internal TimeSpan Period { get; set; }

	internal int MinimumIntervalInMinutes { get; set; } = 3;

	internal bool IsEnabled { get; set; }

	public PeriodicThumbnailScanHostedService(AppSettings appSettings,
		IWebLogger logger,
		IServiceScopeFactory factory)
	{
		_logger = logger;
		_factory = factory;

		if ( appSettings.ThumbnailGenerationIntervalInMinutes >= MinimumIntervalInMinutes )
		{
			Period = TimeSpan.FromMinutes(appSettings
				.ThumbnailGenerationIntervalInMinutes.Value);
			IsEnabled = true;
			return;
		}

		Period = TimeSpan.FromMinutes(60);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// why Task.Yield -> https://medium.com/@thepen0411/how-to-resolve-the-net-background-service-blocking-issue-c96086de8acd
		await Task.Yield();
		await StartBackgroundAsync(IsEnabled, stoppingToken);
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
		catch ( OperationCanceledException exception )
		{
			_logger.LogError("[StartBackgroundAsync] catch-ed OperationCanceledException",
				exception);
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
			await using var asyncScope = _factory.CreateAsyncScope();
			var service = asyncScope.ServiceProvider
				.GetRequiredService<IDatabaseThumbnailGenerationService>();
			await service.StartBackgroundQueue();
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

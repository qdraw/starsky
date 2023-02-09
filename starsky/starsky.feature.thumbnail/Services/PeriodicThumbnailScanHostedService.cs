using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
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
		await StartBackgroundAsync(IsEnabled, stoppingToken);
	}

	internal async Task StartBackgroundAsync(bool startDirect, CancellationToken cancellationToken)
	{
		if ( startDirect )
		{
			await RunJob(cancellationToken);
		}
		
		if ( !IsEnabled)
		{
			return;
		}
		
		using var timer = new PeriodicTimer(Period);
		while (
			!cancellationToken.IsCancellationRequested &&
			await timer.WaitForNextTickAsync(cancellationToken))
		{
			await RunJob(cancellationToken);
		}
	}

	internal async Task<bool?> RunJob(CancellationToken cancellationToken = default)
	{
		if (! IsEnabled )
		{
			_logger.LogInformation(
				$"Skipped {nameof(PeriodicThumbnailScanHostedService)}");
			return false;
		}
		
		cancellationToken.ThrowIfCancellationRequested();

		try
		{
			await using var asyncScope = _factory.CreateAsyncScope();
			var service = asyncScope.ServiceProvider.GetRequiredService<IDatabaseThumbnailGenerationService>();
			await service.StartBackgroundQueue(DateTime.UtcNow.Add(Period));
			_executionCount++;
			// Executed PeriodicThumbnailScanHostedService
			_logger.LogDebug(
				$"Executed {nameof(PeriodicThumbnailScanHostedService)} -" +
				$" Count: {_executionCount} ({DateTime.UtcNow:HH:mm:ss})");
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogInformation(
				$"Failed to execute {nameof(PeriodicThumbnailScanHostedService)} " +
				$"with exception message {ex.Message}. Good luck next round!");
		}
		return null;
	}
}

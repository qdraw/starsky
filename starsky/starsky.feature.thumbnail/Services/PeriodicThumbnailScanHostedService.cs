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
	private int _executionCount = 0;

	internal TimeSpan Period { get; set; }
	
	private bool IsEnabled { get; set; }
	
	public PeriodicThumbnailScanHostedService(AppSettings appSettings,
		IWebLogger logger, 
		IServiceScopeFactory factory)
	{
		_logger = logger;
		_factory = factory;
		
		if ( appSettings.ThumbnailGenerationIntervalInMinutes is >= 2 )
		{
			Period = TimeSpan.FromMinutes(appSettings
				.ThumbnailGenerationIntervalInMinutes.Value);
			IsEnabled = true;
			return;
		}
		
		Period = TimeSpan.FromMinutes(15);
	}


	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await StartBackgroundAsync(stoppingToken);
	}

	internal async Task StartBackgroundAsync(CancellationToken cancellationToken)
	{
		using var timer = new PeriodicTimer(Period);
		while (
			!cancellationToken.IsCancellationRequested &&
			await timer.WaitForNextTickAsync(cancellationToken))
		{
			if (! IsEnabled )
			{
				_logger.LogInformation(
					$"Skipped {nameof(PeriodicThumbnailScanHostedService)}");
				continue;
			}
			
			try
			{
				await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
				var service = asyncScope.ServiceProvider.GetRequiredService<IDatabaseThumbnailGenerationService>();
				await service.StartBackgroundQueue(); 
				_executionCount++;
				_logger.LogInformation(
					$"Executed {nameof(PeriodicThumbnailScanHostedService)} - Count: {_executionCount}");
			}
			catch (Exception ex)
			{
				_logger.LogInformation(
					$"Failed to execute {nameof(PeriodicThumbnailScanHostedService)} with exception message {ex.Message}. Good luck next round!");
			}
		}
	}
}

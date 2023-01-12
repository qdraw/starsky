using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.platform.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.thumbnail.Services;

/// <summary>
/// @see: https://medium.com/medialesson/run-and-manage-periodic-background-tasks-in-asp-net-core-6-with-c-578a31f4b7a3
/// </summary>
public class PeriodicThumbnailScanHostedService : IHostedService
{
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _factory;
	private int _executionCount = 0;
	
	public TimeSpan Period { get; set; } = TimeSpan.FromMinutes(15);
	
	public bool IsEnabled { get; set; }

	public PeriodicThumbnailScanHostedService(
		IWebLogger logger, 
		IServiceScopeFactory factory)
	{
		_logger = logger;
		_factory = factory;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var timer = new PeriodicTimer(Period);
		while (
			!cancellationToken.IsCancellationRequested &&
			await timer.WaitForNextTickAsync(cancellationToken))
		{
			if (! IsEnabled )
			{
				_logger.LogInformation(
					"Skipped PeriodicHostedService");
				continue;
			}
			
			try
			{
				// await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
				// SampleService sampleService = asyncScope.ServiceProvider.GetRequiredService<SampleService>();
				// await sampleService.DoSomethingAsync();
				_executionCount++;
				_logger.LogInformation(
					$"Executed PeriodicHostedService - Count: {_executionCount}");
			}
			catch (Exception ex)
			{
				_logger.LogInformation(
					$"Failed to execute PeriodicHostedService with exception message {ex.Message}. Good luck next round!");
			}
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		IsEnabled = false;
		return Task.CompletedTask;
	}
}

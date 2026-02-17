using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.database.Data;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.feature.search.Services;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public class SearchSuggestionsInflateHostedService(
	IServiceScopeFactory scopeFactory,
	IMemoryCache memoryCache,
	IWebLogger logger,
	AppSettings appSettings)
	: IHostedService
{
	private readonly CancellationTokenSource _stopCts = new();
	private Task? _runTask;

	internal TimeSpan Interval { get; init; } = new(0, 120, 10);

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_runTask = RunAsync(_stopCts.Token);
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

	private async Task RunAsync(CancellationToken cancellationToken)
	{
		try
		{
			await InflateOnceAsync().ConfigureAwait(false);

			using var timer = new PeriodicTimer(Interval);
			while ( await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false) )
			{
				await InflateOnceAsync().ConfigureAwait(false);
			}
		}
		catch ( OperationCanceledException ) when ( cancellationToken.IsCancellationRequested )
		{
			// Graceful shutdown
		}
		catch ( Exception exception )
		{
			logger.LogError("SearchSuggestionsInflateHostedService failed: " + exception.Message,
				exception);
		}
	}

	private async Task InflateOnceAsync()
	{
		using var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await new SearchSuggestionsService(dbContext, memoryCache, logger, appSettings)
			.Inflate().ConfigureAwait(false);
		logger.LogDebug("SearchSuggestionsInflateHostedService: Cache inflated successfully.");
	}
}

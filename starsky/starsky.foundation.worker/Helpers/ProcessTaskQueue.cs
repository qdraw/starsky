using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Helpers;

public static class ProcessTaskQueue
{
	public const int PriorityLaneUpdate = 1;
	public const int PriorityLaneDiskWatcher = 2;
	public const int PriorityLaneThumbnail = 3;

	public static readonly BoundedChannelOptions DefaultBoundedChannelOptions =
		new(int.MaxValue) { FullMode = BoundedChannelFullMode.Wait };

	public static Tuple<TimeSpan, double> RoundUp(AppSettings appSettings)
	{
		if ( appSettings.UseDiskWatcherIntervalInMilliseconds <= 0 )
		{
			return new Tuple<TimeSpan, double>(TimeSpan.Zero, 0);
		}

		var current = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
		var atMinuteInBlock = current % appSettings.UseDiskWatcherIntervalInMilliseconds;
		var msToAdd = appSettings.UseDiskWatcherIntervalInMilliseconds - atMinuteInBlock;
		return new Tuple<TimeSpan, double>(TimeSpan.FromMilliseconds(msToAdd), current);
	}

	public static async Task ProcessBatchedLoopAsync(
		IBaseBackgroundTaskQueue taskQueue, IWebLogger logger, AppSettings appSettings,
		CancellationToken cancellationToken, IServiceScopeFactory? scopeFactory = null)
	{
		await Task.Yield();

		while ( !cancellationToken.IsCancellationRequested )
		{
			try
			{
				var (secondsToWait, _) = RoundUp(appSettings);
				if ( secondsToWait.TotalMilliseconds > 10 )
				{
					await Task.Delay(secondsToWait, cancellationToken);
				}

				var taskQueueCount = taskQueue.Count();
				if ( taskQueueCount <= 0 )
				{
					continue;
				}

				var toDoItems = new List<BackgroundTaskQueueJob>();

				for ( var i = 0; i < taskQueueCount; i++ )
				{
					var queueJob = await taskQueue.DequeueJobAsync(cancellationToken);
					toDoItems.Add(queueJob);
				}

				var afterDistinct = toDoItems.DistinctBy(p => p.MetaData).ToList();

				foreach ( var queueJob in afterDistinct )
				{
					var name = taskQueue.GetType().ToString().Split(".").LastOrDefault();
					logger.LogInformation($"[{name}] next task: {queueJob.MetaData}");

					await ExecuteTask(queueJob, logger, null, cancellationToken, scopeFactory);
				}

				logger.LogInformation(
					$"[{taskQueue.GetType().ToString().Split(".").LastOrDefault()}] next done & wait ");
			}
			catch ( TaskCanceledException )
			{
				// do nothing
			}
		}
	}

	private static async Task ExecuteTask(
		BackgroundTaskQueueJob? queueJob,
		IWebLogger logger,
		IBaseBackgroundTaskQueue? taskQueue, CancellationToken cancellationToken,
		IServiceScopeFactory? scopeFactory = null)
	{
		var activity = CreateActivity(queueJob?.TraceParentId, queueJob?.MetaData);

		try
		{
			if ( taskQueue != null )
			{
				queueJob = await taskQueue.DequeueJobAsync(cancellationToken);
				activity = CreateActivity(queueJob.TraceParentId, queueJob.MetaData);
			}

			if ( queueJob == null )
			{
				throw new InvalidOperationException("Queued job is null");
			}

			activity.Start();
			var executed = await TryExecuteViaRegisteredHandlersAsync(scopeFactory, queueJob,
				cancellationToken);

			if ( !executed )
			{
				throw new InvalidOperationException(
					$"No handler mapping for job type: {queueJob.JobType}");
			}

			StopActivity(activity);
		}
		catch ( OperationCanceledException )
		{
			// do nothing! Prevent throwing if stoppingToken was signaled
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Error occurred executing task work item. {ex.Message}");
		}
	}

	private static Activity CreateActivity(string? parentTraceId, string? metaData)
	{
		metaData ??= nameof(ProcessTaskQueue);
		var activity = new Activity(metaData);
		if ( parentTraceId == null )
		{
			return activity;
		}

		activity.SetParentId(parentTraceId);
		return activity;
	}

	private static void StopActivity(Activity activity)
	{
		if ( activity.Duration == TimeSpan.Zero )
		{
			activity.SetEndTime(DateTime.UtcNow);
		}

		activity.Stop();
	}

	public static async Task ProcessTaskQueueAsync(IBaseBackgroundTaskQueue taskQueue,
		IWebLogger logger, CancellationToken cancellationToken,
		IServiceScopeFactory? scopeFactory = null)
	{
		logger.LogInformation($"Queued Hosted Service {taskQueue.GetType().Name} is " +
		                      $"starting on {Environment.MachineName}");

		while ( !cancellationToken.IsCancellationRequested )
		{
			await ExecuteTask(null!, logger, taskQueue, cancellationToken, scopeFactory);
		}
	}

	private static async Task<bool> TryExecuteViaRegisteredHandlersAsync(
		IServiceScopeFactory? scopeFactory,
		BackgroundTaskQueueJob queueJob,
		CancellationToken cancellationToken)
	{
		if ( scopeFactory == null || string.IsNullOrWhiteSpace(queueJob.JobType) )
		{
			return false;
		}

		using var scope = scopeFactory.CreateScope();
		var handlers = scope.ServiceProvider.GetServices<IBackgroundJobHandler>();
		var handler = handlers.FirstOrDefault(h => h.JobType == queueJob.JobType);
		if ( handler == null )
		{
			return false;
		}

		await handler.ExecuteAsync(queueJob.PayloadJson, cancellationToken);
		return true;
	}
}

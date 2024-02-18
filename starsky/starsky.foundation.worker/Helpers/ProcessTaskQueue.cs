using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Helpers
{
	public static class ProcessTaskQueue
	{
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
			CancellationToken cancellationToken)
		{
			await Task.Yield();

			while ( !cancellationToken.IsCancellationRequested )
			{
				try
				{
					var secondsToWait = RoundUp(appSettings).Item1;
					if ( secondsToWait.TotalMilliseconds > 10 )
					{
						await Task.Delay(secondsToWait, cancellationToken);
					}

					var taskQueueCount = taskQueue.Count();
					if ( taskQueueCount <= 0 )
					{
						continue;
					}

					var toDoItems = new List<Tuple<Func<CancellationToken, ValueTask>,
						string?, string?>>();

					for ( var i = 0; i < taskQueueCount; i++ )
					{
						var (workItem, metaData, parentTraceId) =
							await taskQueue.DequeueAsync(cancellationToken);
						toDoItems.Add(
							new Tuple<Func<CancellationToken, ValueTask>, string?, string?>(
								workItem,
								metaData, parentTraceId));
					}

					var afterDistinct = toDoItems.DistinctBy(p => p.Item2).ToList();

					foreach ( var (task, meta, _) in afterDistinct )
					{
						var name = taskQueue.GetType().ToString().Split(".").LastOrDefault();
						logger.LogInformation($"[] {name} next task: {meta}");

						await ExecuteTask(task, logger, null, cancellationToken);
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
			Func<CancellationToken, ValueTask> workItem,
			IWebLogger logger,
			IBaseBackgroundTaskQueue? taskQueue, CancellationToken cancellationToken)
		{
			string? metaData = null;
			var activity = CreateActivity(null, metaData);

			try
			{
				if ( taskQueue != null )
				{
					string? parentTraceId;
					// Dequeue here
					( workItem, metaData, parentTraceId ) =
						await taskQueue.DequeueAsync(cancellationToken);

					// set as parent for activity
					activity = CreateActivity(parentTraceId, metaData);
				}

				activity.Start();

				await workItem(cancellationToken);

				StopActivity(activity);
			}
			catch ( OperationCanceledException )
			{
				// do nothing! Prevent throwing if stoppingToken was signaled
			}
			catch ( Exception ex )
			{
				logger.LogError(ex, "Error occurred executing task work item.");
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
			IWebLogger logger, CancellationToken cancellationToken)
		{
			logger.LogInformation($"Queued Hosted Service {taskQueue.GetType().Name} is " +
			                      $"starting on {Environment.MachineName}");

			while ( !cancellationToken.IsCancellationRequested )
			{
				await ExecuteTask(null!, logger, taskQueue, cancellationToken);
			}
		}

		public static readonly BoundedChannelOptions DefaultBoundedChannelOptions =
			new(int.MaxValue) { FullMode = BoundedChannelFullMode.Wait };

		public static ValueTask QueueBackgroundWorkItemAsync(
			Channel<Tuple<Func<CancellationToken, ValueTask>, string?, string?>> channel,
			Func<CancellationToken, ValueTask> workItem,
			string? metaData = null, string? traceParentId = null)
		{
			ArgumentNullException.ThrowIfNull(workItem);
			return QueueBackgroundWorkItemInternalAsync(channel, workItem, metaData, traceParentId);
		}

		private static async ValueTask QueueBackgroundWorkItemInternalAsync(
			Channel<Tuple<Func<CancellationToken, ValueTask>, string?, string?>> channel,
			Func<CancellationToken, ValueTask> workItem,
			string? metaData = null, string? traceParentId = null)
		{
			await channel.Writer.WriteAsync(
				new Tuple<Func<CancellationToken, ValueTask>, string?, string?>(workItem, metaData,
					traceParentId));
		}
	}
}

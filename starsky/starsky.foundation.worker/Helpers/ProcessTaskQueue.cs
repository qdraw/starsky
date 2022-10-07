using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Helpers
{
	public static class ProcessTaskQueue
	{
		public static Tuple<TimeSpan,double> RoundUp(AppSettings appSettings)
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
			CancellationToken stoppingToken)
		{
			await Task.Yield();
			
			while (stoppingToken.IsCancellationRequested == false)
			{
				await Task.Delay(RoundUp(appSettings).Item1, stoppingToken);

				var taskQueueCount = taskQueue.Count();
				if ( taskQueueCount <= 0 )
				{
					continue;
				}
				
				var toDoItems = new List<Tuple<Func<CancellationToken, ValueTask>,
					string>>();

				for ( var i = 0; i < taskQueueCount; i++ )
				{
					var (workItem,metaData) = await taskQueue.DequeueAsync(stoppingToken);
					toDoItems.Add(new Tuple<Func<CancellationToken, ValueTask>, string>(workItem,metaData));
				}

				var afterDistinct = toDoItems.DistinctBy(p => p.Item2).ToList();

				foreach ( var (task, meta) in afterDistinct )
				{
					logger.LogInformation($"[{nameof(taskQueue)}] next task: " + meta);
					await ExecuteTask(task, stoppingToken, logger);
				}
			}
		}

		private static async Task ExecuteTask(
			Func<CancellationToken, ValueTask> workItem,
			CancellationToken cancellationToken, IWebLogger logger,
			IBaseBackgroundTaskQueue taskQueue = null)
		{
			try
			{
				if ( taskQueue != null )
				{
					(workItem, _ )=
						await taskQueue.DequeueAsync(cancellationToken);
				}
				await workItem(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				// do nothing! Prevent throwing if stoppingToken was signaled
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error occurred executing task work item.");
			}
		}

		public static async Task ProcessTaskQueueAsync(IBaseBackgroundTaskQueue taskQueue, IWebLogger logger, CancellationToken stoppingToken)
		{
			logger.LogInformation($"Queued Hosted Service {taskQueue.GetType().Name} is " +
			                       $"starting on {Environment.MachineName}");
		
			while (!stoppingToken.IsCancellationRequested)
			{
				await ExecuteTask(null, stoppingToken, logger, taskQueue);
			}
		}
	}
}

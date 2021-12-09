using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.foundation.worker.Helpers
{
	public static class  ProcessTaskQueue
	{
		public static async Task ProcessTaskQueueAsync(IBaseBackgroundTaskQueue taskQueue, IWebLogger logger, CancellationToken stoppingToken)
		{
			logger.LogInformation($"Queued Hosted Service {taskQueue.GetType().Name} is " +
			                       $"starting on {Environment.MachineName}");
			while (!stoppingToken.IsCancellationRequested)
			{
				var workItem = await taskQueue.DequeueAsync(stoppingToken);
				try
				{
					await workItem(stoppingToken);
				}
				catch (Exception exception)
				{
					logger.LogError(exception,  
						$"Error occurred executing workItem ", nameof(workItem));
				}
			}
			logger.LogInformation("Queued Hosted Service has stopped");
		}
	}
}

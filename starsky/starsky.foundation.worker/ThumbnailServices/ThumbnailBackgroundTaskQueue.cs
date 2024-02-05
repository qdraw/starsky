using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.worker.CpuEventListener.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.ThumbnailServices.Exceptions;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starsky.foundation.worker.ThumbnailServices
{
	/// <summary>
	/// @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
	/// </summary>
	[Service(typeof(IThumbnailQueuedHostedService),
		InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class ThumbnailBackgroundTaskQueue : IThumbnailQueuedHostedService
	{
		private readonly ICpuUsageListener _cpuUsageListenerService;
		private readonly IWebLogger _logger;
		private readonly AppSettings _appSettings;
		private readonly Channel<Tuple<Func<CancellationToken, ValueTask>, string>> _queue;

		public ThumbnailBackgroundTaskQueue(ICpuUsageListener cpuUsageListenerService,
			IWebLogger logger, AppSettings appSettings)
		{
			_cpuUsageListenerService = cpuUsageListenerService;
			_logger = logger;
			_appSettings = appSettings;
			_queue = Channel.CreateBounded<Tuple<Func<CancellationToken, ValueTask>, string>>(
				ProcessTaskQueue.DefaultBoundedChannelOptions);
		}

		public int Count()
		{
			return _queue.Reader.Count;
		}

		[SuppressMessage("ReSharper", "InvertIf")]
		public ValueTask QueueBackgroundWorkItemAsync(
			Func<CancellationToken, ValueTask> workItem, string metaData)
		{
			if ( _cpuUsageListenerService.CpuUsageMean > _appSettings.CpuUsageMaxPercentage )
			{
				_logger.LogInformation("CPU is to high, skip thumbnail generation");
				throw new ToManyUsageException($"QueueBackgroundWorkItemAsync: " +
				                               $"Skip {metaData} because of high CPU usage");
			}

			return ProcessTaskQueue.QueueBackgroundWorkItemAsync(_queue,
				workItem, metaData);
		}

		public async ValueTask<Tuple<Func<CancellationToken, ValueTask>, string>> DequeueAsync(
			CancellationToken cancellationToken)
		{
			MetricsHelper.Add(nameof(ThumbnailBackgroundTaskQueue), "Items in queue", Count());
			var workItem =
				await _queue.Reader.ReadAsync(cancellationToken);
			return workItem;
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using starsky.foundation.injection;
using starsky.foundation.worker.Helpers;

namespace starsky.foundation.sync.WatcherBackgroundService
{
	/// <summary>
	/// @see: https://www.c-sharpcorner.com/article/how-to-call-background-service-from-net-core-web-api/
	/// </summary>
	[Service(typeof(IDiskWatcherBackgroundTaskQueue), InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class DiskWatcherBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
	{
		private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = 
			new ConcurrentQueue<Func<CancellationToken, Task>>();
		private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
		private readonly TelemetryClient _telemetryClient;

		public DiskWatcherBackgroundTaskQueue(TelemetryClient telemetryClient = null)
		{
			_telemetryClient = telemetryClient;
		}
		
		public void QueueBackgroundWorkItem(
			Func<CancellationToken, Task> workItem)
		{
			BaseBackgroundTaskQueue.QueueBackgroundWorkItem(workItem, _workItems, _signal);
		}

		internal bool TrackQueue()
		{
			if ( _telemetryClient == null ) return false;
			var sample = new MetricTelemetry {
				Sum = _workItems.Count, 
				Min = 0,
				Name = nameof(DiskWatcherBackgroundTaskQueue), 
				Timestamp = DateTimeOffset.UtcNow,
				MetricNamespace = "Queue"
			};
			_telemetryClient.TrackMetric(sample);
			_telemetryClient.TrackTrace($"[{nameof(DiskWatcherBackgroundTaskQueue)}] contains {_workItems.Count} items", SeverityLevel.Verbose);
			return true;
		}

		public async Task<Func<CancellationToken, Task>> DequeueAsync(
			CancellationToken cancellationToken)
		{
			var workItem = await BaseBackgroundTaskQueue.DequeueAsync(
				cancellationToken,
				_workItems, _signal);
			TrackQueue();
			return workItem;
		}
	}
}

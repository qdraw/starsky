using System.Diagnostics.Metrics;
using starsky.foundation.injection;
using starsky.foundation.platform.MetricsNamespaces;

namespace starsky.foundation.sync.Metrics;

[Service(typeof(DiskWatcherBackgroundTaskQueueMetrics),
	InjectionLifetime = InjectionLifetime.Singleton)]
public class DiskWatcherBackgroundTaskQueueMetrics
{
	public int Value { get; set; }

	public DiskWatcherBackgroundTaskQueueMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(ActivitySourceMeter.SyncNameSpace);
		const string name = nameof(DiskWatcherBackgroundTaskQueueMetrics);
		meter.CreateObservableGauge(name, ObserveValue);
	}

	private int ObserveValue()
	{
		return Value;
	}
}
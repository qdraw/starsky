using System.Diagnostics.Metrics;
using starsky.foundation.injection;
using starsky.foundation.platform.MetricsNamespaces;

namespace starsky.foundation.worker.Metrics;

[Service(typeof(ThumbnailBackgroundQueuedMetrics),
	InjectionLifetime = InjectionLifetime.Singleton)]
public class ThumbnailBackgroundQueuedMetrics
{
	public int Value { get; set; }

	public ThumbnailBackgroundQueuedMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(ActivitySourceMeter.WorkerNameSpace);
		const string name = nameof(ThumbnailBackgroundQueuedMetrics);
		meter.CreateObservableGauge(name, ObserveValue);
	}

	private int ObserveValue()
	{
		return Value;
	}
}

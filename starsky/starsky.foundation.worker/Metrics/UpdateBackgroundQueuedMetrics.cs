using System.Diagnostics.Metrics;
using starsky.foundation.injection;
using starsky.foundation.platform.MetricsNamespaces;

namespace starsky.foundation.worker.Metrics;

[Service(typeof(UpdateBackgroundQueuedMetrics),
	InjectionLifetime = InjectionLifetime.Singleton)]
public class UpdateBackgroundQueuedMetrics
{
	public int Value { get; set; }

	public UpdateBackgroundQueuedMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(ActivitySourceMeter.WorkerNameSpace);
		const string name = "App." + nameof(UpdateBackgroundQueuedMetrics);
		meter.CreateObservableGauge(name, ObserveValue);
	}

	internal int ObserveValue()
	{
		return Value;
	}
}

using System.Diagnostics.Metrics;
using starsky.foundation.diagnosticsource.ActivitySource;

namespace starsky.foundation.diagnosticsource.Metrics;

public class HatCoMetrics : IMeterFactory
{
	private readonly ObservableGauge<int> _hatsSold;

	public HatCoMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(ActivitySourceMeter.NameSpace);
		
		_hatsSold = meter.CreateObservableGauge<int>("total-categories", ObserveValue, "Category", "Get total amount of categories");
		
	}
	
	public Meter Create(MeterOptions options) => new Meter(options);

	private static int ObserveValue()
	{
		throw new NotImplementedException();
	}

	public void HatsSold(int quantity)
	{
		_hatsSold.Meter.
	}

	public void Dispose()
	{
		// TODO release managed resources here
	}

}

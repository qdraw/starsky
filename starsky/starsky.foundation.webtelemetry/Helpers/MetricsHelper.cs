using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace starsky.foundation.webtelemetry.Helpers;

public static class MetricsHelper
{
	public static void Add(TelemetryClient? telemetryClient, string name, double value)
	{
		if ( telemetryClient == null )
		{
			return;
		}
		
		var sample = new MetricTelemetry
		{
			Name = name, 
			Sum = value
		};
		telemetryClient.TrackMetric(sample);
	}
}

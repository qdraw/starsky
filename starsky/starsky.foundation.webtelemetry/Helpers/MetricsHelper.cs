using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace starsky.foundation.webtelemetry.Helpers;

public static class MetricsHelper
{
	public static bool Add(TelemetryClient? telemetryClient, string name, int value)
	{
		if ( telemetryClient == null )
		{
			return false;
		}
		
		var sample = new MetricTelemetry
		{
			Name = name, 
			Sum = value
		};
		
		telemetryClient.TrackMetric(sample);
		return true;
	}
}

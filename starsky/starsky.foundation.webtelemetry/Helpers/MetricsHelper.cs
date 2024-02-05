using System.Diagnostics.Metrics;

namespace starsky.foundation.webtelemetry.Helpers;

public static class MetricsHelper
{
	public static bool Add(string name, string description, int value)
	{
		using var meter = new Meter(name, "1.0");
		var successCounter = meter.CreateCounter<long>(name, description: description);

		return true;
	}
}

using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace starskytest.FakeMocks
{
	public class FakeITelemetryProcessor : ITelemetryProcessor
	{
		public List<ITelemetry> Received { get; set; } = new List<ITelemetry>();
		
		public void Process(ITelemetry item)
		{
			Received.Add(item);
		}
	}
}

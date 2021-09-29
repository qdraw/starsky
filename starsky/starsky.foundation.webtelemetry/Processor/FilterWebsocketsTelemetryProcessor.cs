using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace starsky.foundation.webtelemetry.Processor
{
	public class FilterWebsocketsTelemetryProcessor : ITelemetryProcessor
	{
		private readonly ITelemetryProcessor _next;

		public FilterWebsocketsTelemetryProcessor(ITelemetryProcessor next)
		{
			// Next TelemetryProcessor in the chain
			_next = next;
		}

		public void Process(ITelemetry item)
		{
			if (item is RequestTelemetry request)
			{
				if (request.ResponseCode == "101")
				{
					return;
				}
			}

			// Send the item to the next TelemetryProcessor
			_next.Process(item);
		}
	}
}

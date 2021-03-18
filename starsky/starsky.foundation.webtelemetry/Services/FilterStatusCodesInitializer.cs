using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace starsky.foundation.webtelemetry.Services
{
	public class FilterStatusCodesInitializer : ITelemetryInitializer
	{
		public void Initialize(ITelemetry telemetry)
		{
			var request = telemetry as RequestTelemetry;
			if ( request == null ) return;
			
			switch (request.ResponseCode)
			{
				case "401":
				case "404":
					request.Success = true;
					break;

			}
		}
	}

}

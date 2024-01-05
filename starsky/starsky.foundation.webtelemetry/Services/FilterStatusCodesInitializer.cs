using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using starsky.foundation.injection;

namespace starsky.foundation.webtelemetry.Services
{
	/// <summary>
	/// App Insights Filter Status Codes Initializer
	/// Remove when App insights is phased out
	/// </summary>
	[Service(typeof(ITelemetryInitializer), InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class FilterStatusCodesInitializer : ITelemetryInitializer
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

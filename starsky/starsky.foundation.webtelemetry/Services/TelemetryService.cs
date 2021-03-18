using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Interfaces;

namespace starsky.foundation.webtelemetry.Services
{
	[Service(typeof(ITelemetryService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class TelemetryService : ITelemetryService
	{
		private readonly TelemetryClient _telemetry;

		public TelemetryService(AppSettings appSettings)
		{
			if (appSettings == null ||  string.IsNullOrEmpty(appSettings.ApplicationInsightsInstrumentationKey) ) return;
			_telemetry = new TelemetryClient(TelemetryConfiguration.CreateDefault())
			{
				InstrumentationKey = appSettings.ApplicationInsightsInstrumentationKey
			};
		}
		public bool TrackException(Exception exception)
		{
			if ( _telemetry == null ) return false;
			_telemetry.TrackException(exception);
			return true;
		}
	}

}

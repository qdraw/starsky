using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.Models;

namespace starsky.foundation.webtelemetry.Extensions
{
	public static class ApplicationInsightsExtension
	{
		public static void AddMonitoring(this IServiceCollection services, AppSettings appSettings)
		{
			if ( string.IsNullOrWhiteSpace(appSettings.ApplicationInsightsInstrumentationKey) )
			{
				return;
			}

			// https://docs.microsoft.com/en-us/azure/azure-monitor/app/telemetry-channels
			services.AddSingleton(typeof(ITelemetryChannel),
				new ServerTelemetryChannel()
				{
					StorageFolder = appSettings.TempFolder,
				});

			services.AddApplicationInsightsTelemetry(
				new ApplicationInsightsServiceOptions
				{
					ApplicationVersion = appSettings.AppVersion,
					EnableDependencyTrackingTelemetryModule = true,
					EnableHeartbeat = true,
					EnableAuthenticationTrackingJavaScript = true,
					InstrumentationKey = appSettings
						.ApplicationInsightsInstrumentationKey,
				});
			
			services.AddApplicationInsightsKubernetesEnricher();
		}
	}
}

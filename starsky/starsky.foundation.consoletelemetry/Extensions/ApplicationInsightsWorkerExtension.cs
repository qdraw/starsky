using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.consoletelemetry.Initializers;
using starsky.foundation.platform.Models;

namespace starsky.foundation.consoletelemetry.Extensions
{
	public static class ApplicationInsightsWorkerExtension
	{
		/// <summary>
		/// Add Metrics & Monitoring for Application Insights
		/// </summary>
		/// <param name="services">collection service</param>
		/// <param name="appSettings">to use for ApplicationInsights InstrumentationKey</param>
		/// <param name="appType">application type</param>
		public static void AddMonitoringWorkerService(this IServiceCollection services,
			AppSettings appSettings, AppSettings.StarskyAppType appType)
		{
			if ( string.IsNullOrWhiteSpace(appSettings
				    .ApplicationInsightsInstrumentationKey) )
			{
				return;
			}
			
			appSettings.ApplicationType = appType;
			services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer($"{appType}"));
			
			services.AddApplicationInsightsTelemetryWorkerService(new ApplicationInsightsServiceOptions
			{
				InstrumentationKey = appSettings
					.ApplicationInsightsInstrumentationKey,
				ApplicationVersion = appSettings.AppVersion,
				EnableDependencyTrackingTelemetryModule = true,
				EnableHeartbeat = true
			});
			
		}
	}
}

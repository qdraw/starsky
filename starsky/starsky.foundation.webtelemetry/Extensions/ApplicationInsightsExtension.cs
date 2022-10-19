using System.Runtime.CompilerServices;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.EventCounterCollector;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Initializers;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.webtelemetry.Extensions
{
	public static class ApplicationInsightsExtension
	{
		/// <summary>
		/// Add Metrics & Monitoring for Application Insights (AddApplicationInsights)
		/// </summary>
		/// <param name="services">collection service</param>
		/// <param name="appSettings">to use for ApplicationInsights InstrumentationKey</param>
		public static void AddMonitoring(this IServiceCollection services, AppSettings appSettings)
		{
			if ( string.IsNullOrWhiteSpace(appSettings.ApplicationInsightsConnectionString) )
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
					EnableEventCounterCollectionModule = true,
					ConnectionString = appSettings.ApplicationInsightsConnectionString
				});
			
			services.AddSingleton<ITelemetryInitializer>(new CloudRoleNameInitializer($"{AppSettings.StarskyAppType.WebController}"));

			services.ConfigureTelemetryModule<EventCounterCollectionModule>(
				(module, _) => SetEventCounterCollectionModule(module));
			
		}

		/// <summary>
		/// Add Event Counters to Application Insights
		/// @see: https://docs.microsoft.com/en-us/azure/azure-monitor/app/eventcounters
		/// </summary>
		/// <param name="module">Modules</param>
		internal static void SetEventCounterCollectionModule(
			EventCounterCollectionModule module)
		{
			// in .NET Core 3 there are no default Counters (Event counters)
			module.Counters.Clear();
			// https://docs.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
			module.Counters.Add(
				new EventCounterCollectionRequest("System.Runtime",
					"gen-0-size"));
			module.Counters.Add(
				new EventCounterCollectionRequest("System.Runtime",
					"time-in-gc"));
			module.Counters.Add(
				new EventCounterCollectionRequest("System.Runtime",
					"cpu-usage"));
			// memory usage
			module.Counters.Add(
				new EventCounterCollectionRequest("System.Runtime",
					"working-set"));	
			// Retrieves the number of bytes currently thought to be allocated
			module.Counters.Add(
				new EventCounterCollectionRequest("System.Runtime",
					"gc-heap-size"));	
			module.Counters.Add(
				new EventCounterCollectionRequest("Microsoft.AspNetCore.Hosting",
					"current-request"));
		}
	}
}

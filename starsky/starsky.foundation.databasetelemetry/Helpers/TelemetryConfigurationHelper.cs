#nullable enable
using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.databasetelemetry.Helpers
{
	public static class TelemetryConfigurationHelper
	{
		public static TelemetryClient? InitTelemetryClient(string appInsightsConnectionString, string roleName, IWebLogger? logger, TelemetryClient? telemetryClient)
		{
			TelemetryClient? Clean(Exception exception)
			{
				logger?.LogInformation($"catch-ed exception; {exception.Message} ", exception);
				logger?.LogInformation("run GC.Collect next -->");
				GC.Collect();
				return null;
			}

			try
			{
				if ( telemetryClient == null )
				{
					var telemetryConfiguration =
						CreateTelemetryConfiguration(appInsightsConnectionString);
					if ( telemetryConfiguration == null ) return null;
					telemetryClient =
						new TelemetryClient(telemetryConfiguration);
					telemetryClient.Context.Cloud.RoleName = roleName;
					telemetryClient.Context.Cloud.RoleInstance =
						Environment.MachineName;
				}
				
				var module = CreateDatabaseDependencyTrackingTelemetryModule();
				module.Initialize(telemetryClient.TelemetryConfiguration);
				return telemetryClient;
			}
			catch ( OutOfMemoryException exception )
			{
				return Clean(exception);
			}
			catch (System.Threading.Tasks.TaskSchedulerException exception)
			{
				return Clean(exception);
			}
		}

		private static TelemetryConfiguration? CreateTelemetryConfiguration(string appInsightsInstrumentationKey)
		{
			var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
			telemetryConfiguration.InstrumentationKey = appInsightsInstrumentationKey;
			return telemetryConfiguration;
		}

		private static DependencyTrackingTelemetryModule CreateDatabaseDependencyTrackingTelemetryModule()
		{
			var module = new DependencyTrackingTelemetryModule();
			module.IncludeDiagnosticSourceActivities.Add("Database");
			module.EnableSqlCommandTextInstrumentation = true;
			return module;
		}
	}
}

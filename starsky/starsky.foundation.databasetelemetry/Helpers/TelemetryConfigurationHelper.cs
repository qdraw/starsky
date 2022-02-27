using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.databasetelemetry.Helpers
{
	public static class TelemetryConfigurationHelper
	{
		public static TelemetryClient InitTelemetryClient(string appInsightsConnectionString, string roleName, IWebLogger logger)
		{
			try
			{
				var module = CreateDatabaseDependencyTrackingTelemetryModule();
				var telemetryConfiguration = CreateTelemetryConfiguration(appInsightsConnectionString);
				if ( telemetryConfiguration == null ) return null;
				var telemetryClient = new TelemetryClient(telemetryConfiguration);
				telemetryClient.Context.Cloud.RoleName = roleName;
				telemetryClient.Context.Cloud.RoleInstance = Environment.MachineName;
				module.Initialize(telemetryConfiguration);
				return telemetryClient;
			}
			catch (OutOfMemoryException exception)
			{
				logger.LogInformation($"catch-ed exception; {exception.Message} ", exception);
				logger.LogInformation("run GC.Collect next -->");
				GC.Collect();
				return null;
			}
		}

		private static TelemetryConfiguration CreateTelemetryConfiguration(string appInsightsInstrumentationKey)
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

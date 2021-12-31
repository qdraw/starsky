using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;

namespace starsky.foundation.databasetelemetry.Helpers
{
	public static class TelemetryConfigurationHelper
	{
		public static TelemetryClient InitTelemetryClient(string appInsightsConnectionString, string roleName)
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

		private static TelemetryConfiguration CreateTelemetryConfiguration(string appInsightsInstrumentationKey)
		{
			try
			{
				var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
				telemetryConfiguration.InstrumentationKey = appInsightsInstrumentationKey;
				return telemetryConfiguration;
			}
			catch (OutOfMemoryException)
			{
				return null;
			}
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

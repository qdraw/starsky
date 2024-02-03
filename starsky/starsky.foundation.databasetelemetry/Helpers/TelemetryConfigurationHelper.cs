using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using starsky.foundation.databasetelemetry.Processor;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.databasetelemetry.Helpers
{
	public static class TelemetryConfigurationHelper
	{
		public static TelemetryClient? InitTelemetryClient(string appInsightsConnectionString, string roleName, IWebLogger? logger, TelemetryClient? telemetryClient)
		{
			try
			{
				// Should skip to avoid memory issues
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
					logger?.LogInformation("Added TelemetryClient [should avoid due memory issues]");
				}
				
				var module = CreateDatabaseDependencyTrackingTelemetryModule();
				module.Initialize(telemetryClient.TelemetryConfiguration);
				return telemetryClient;
			}
			catch ( OutOfMemoryException )
			{
				return null;
			}
			catch (System.Threading.Tasks.TaskSchedulerException )
			{
				return null;
			}
		}

		private static TelemetryConfiguration? CreateTelemetryConfiguration(string appInsightsConnectionString)
		{
			var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
			telemetryConfiguration.ConnectionString = appInsightsConnectionString;
			telemetryConfiguration.TelemetryProcessorChainBuilder.Use(next => new FilterWebsocketsTelemetryProcessor(next));
			telemetryConfiguration.TelemetryProcessorChainBuilder.Build();
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

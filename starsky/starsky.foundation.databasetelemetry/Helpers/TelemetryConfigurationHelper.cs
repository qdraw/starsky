using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;

namespace starsky.foundation.databasetelemetry.Helpers
{
	public static class TelemetryConfigurationHelper
	{
		public static TelemetryClient InitTelemetryClient(string appInsightsConnectionString)
		{
			var module = CreateDatabaseDependencyTrackingTelemetryModule();
			var telemetryConfiguration = CreateTelemetryConfiguration(appInsightsConnectionString);
			var telemetryClient = new TelemetryClient(telemetryConfiguration);
			module.Initialize(telemetryConfiguration);
			return telemetryClient;
		}

		private static TelemetryConfiguration CreateTelemetryConfiguration(string appInsightsInstrumentationKey)
		{
			TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
			telemetryConfiguration.InstrumentationKey = appInsightsInstrumentationKey;
			telemetryConfiguration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
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

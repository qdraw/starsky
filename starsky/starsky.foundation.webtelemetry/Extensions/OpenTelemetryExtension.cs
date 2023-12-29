using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.webtelemetry.Extensions;

public static class OpenTelemetryExtension
{
	/// <summary>
	/// Add Metrics & Monitoring for OpenTelemetry
	/// </summary>
	/// <param name="services">collection service</param>
	/// <param name="appSettings">to use for ApplicationInsights InstrumentationKey</param>
	public static void AddOpenTelemetryMonitoring(
		this IServiceCollection services, AppSettings appSettings)
	{
		if ( string.IsNullOrWhiteSpace(appSettings.ApplicationInsightsConnectionString) )
		{
			return;
		}
		
		const string serviceName = "roll-dice";

		services.builder.Logging.AddOpenTelemetry(options =>
		{
			options
				.SetResourceBuilder(
					ResourceBuilder.CreateDefault()
						.AddService(serviceName))
				.AddConsoleExporter();
		});
		builder.Services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(serviceName))
			.WithTracing(tracing => tracing
				.AddAspNetCoreInstrumentation()
				.AddConsoleExporter())
			.WithMetrics(metrics => metrics
				.AddAspNetCoreInstrumentation()
				.AddConsoleExporter());
	}
}

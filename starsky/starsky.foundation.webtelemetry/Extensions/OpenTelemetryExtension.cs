using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
		if ( string.IsNullOrWhiteSpace(appSettings.OpenTelemetryEndpoint) )
		{
			return;
		}

		services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(
				serviceNamespace: appSettings.Name,
				serviceName: appSettings.Name,
				serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version
					?.ToString(),
				serviceInstanceId: Environment.MachineName
			).AddAttributes(new Dictionary<string, object>
			{
				{
					"deployment.environment",
					Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
					!
				}
			}))
			.WithTracing(tracing => tracing.AddAspNetCoreInstrumentation()
				.AddConsoleExporter()
				.AddOtlpExporter(
					o =>
					{
						o.Endpoint = new Uri(appSettings.OpenTelemetryEndpoint);
						o.Protocol = OtlpExportProtocol.HttpProtobuf;
						o.Headers = appSettings.OpenTelemetryHeader;
					}
				)
			)
			.WithMetrics(metrics =>
				metrics.AddAspNetCoreInstrumentation()
					.AddRuntimeInstrumentation()
					.AddConsoleExporter()
					.AddOtlpExporter(	
						o =>
					{
						o.Endpoint = new Uri(appSettings.OpenTelemetryEndpoint);
						o.Protocol = OtlpExportProtocol.HttpProtobuf;
						o.Headers = appSettings.OpenTelemetryHeader;
					})
				);
	}
}

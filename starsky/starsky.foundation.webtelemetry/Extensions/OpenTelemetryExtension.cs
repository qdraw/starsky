using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using starsky.foundation.platform.Interfaces;
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
		var telemetryBuilder = services.AddOpenTelemetry()
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
						Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!
				}
			}));
		
		if ( !string.IsNullOrWhiteSpace(appSettings.OpenTelemetry.TracesEndpoint) )
		{
			telemetryBuilder.WithTracing(tracing => tracing
				.AddAspNetCoreInstrumentation()
				.AddOtlpExporter(
					o =>
					{
						o.Endpoint =
							new Uri(appSettings.OpenTelemetry.TracesEndpoint);
						o.Protocol = OtlpExportProtocol.HttpProtobuf;
						o.Headers = appSettings.OpenTelemetry.GetTracesHeader();
					}
				).SetResourceBuilder(
					ResourceBuilder.CreateDefault()
						.AddService(appSettings.OpenTelemetry.GetServiceName())
				)
			);
		}

		if ( string.IsNullOrWhiteSpace(
			    appSettings.OpenTelemetry.MetricsEndpoint) )
		{
			return;
		}

		telemetryBuilder.WithMetrics(metrics =>
			metrics.AddAspNetCoreInstrumentation()
				.AddRuntimeInstrumentation()
				.AddHttpClientInstrumentation()
				.AddOtlpExporter(
					o =>
					{
						o.Endpoint = new Uri(appSettings.OpenTelemetry.MetricsEndpoint);
						o.Protocol = OtlpExportProtocol.HttpProtobuf;
						o.Headers = appSettings.OpenTelemetry.GetMetricsHeader();
					})
				.SetResourceBuilder(
					ResourceBuilder.CreateDefault()
						.AddService(appSettings.OpenTelemetry.GetServiceName())	
				)
		);

	}
}

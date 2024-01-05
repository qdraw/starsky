using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
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
	/// <param name="appSettings">to use for OpenTelemetry keys and info</param>
	public static void AddOpenTelemetryMonitoring(
		this IServiceCollection services, AppSettings appSettings)
	{
		var telemetryBuilder = services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(
				serviceNamespace: appSettings.OpenTelemetry.GetServiceName(),
				serviceName: appSettings.OpenTelemetry.GetServiceName(),
				serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version
					?.ToString(),
				serviceInstanceId: Environment.MachineName
			).AddAttributes(new Dictionary<string, object>
			{
				{
					"deployment.environment",
						Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty
				}
			}));
		
		if ( !string.IsNullOrWhiteSpace(appSettings.OpenTelemetry.TracesEndpoint) )
		{
			telemetryBuilder.WithTracing(tracing => tracing
				.AddAspNetCoreInstrumentation(o => o.Filter = FilterPath)
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
	
	internal static bool FilterPath(HttpContext context)
	{
		if ( (context.Request.Path.Value?.EndsWith("/realtime") == true || 
		     context.Request.Path.Value?.EndsWith("/api/health") == true || 
		     context.Request.Path.Value?.EndsWith("/api/health/details") == true || 
		     context.Request.Path.Value?.EndsWith("/api/open-telemetry/trace") == true) 
		     && context.Response.StatusCode == 200)
		{
			return false;
		}

		if ( context.Request.Path.Value?.EndsWith("/api/index") == true
		     && context.Response.StatusCode == 401)
		{
			return false;
		}
		
		return true;
	}
}

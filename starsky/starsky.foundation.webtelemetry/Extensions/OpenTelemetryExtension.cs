using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using starsky.foundation.platform.MetricsNamespaces;
using starsky.foundation.platform.Models;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.webtelemetry.Extensions;

public static class OpenTelemetryExtension
{
	/// <summary>
	///     Add Metrics and Monitoring for OpenTelemetry
	/// </summary>
	/// <param name="services">collection service</param>
	/// <param name="appSettings">to use for OpenTelemetry keys and info</param>
	public static void AddOpenTelemetryMonitoring(
		this IServiceCollection services, AppSettings appSettings)
	{
		if ( appSettings.OpenTelemetry == null )
		{
			return;
		}

		var telemetryBuilder = services.AddOpenTelemetry()
			.ConfigureResource(resource => resource.AddService(
				serviceNamespace: appSettings.OpenTelemetry.GetServiceName(),
				serviceName: appSettings.OpenTelemetry.GetServiceName(),
				serviceVersion: Assembly.GetEntryAssembly()?.GetName().Version
					?.ToString(),
				serviceInstanceId: Environment.MachineName
			).AddAttributes(new Dictionary<string, object>
			{
				{ "deployment.environment", appSettings.OpenTelemetry.GetEnvironmentName() },
				{ "service.name", appSettings.OpenTelemetry.GetServiceName() },
				{ "service.namespace", appSettings.OpenTelemetry.GetServiceName() },
				{ "service.instance.id", appSettings.OpenTelemetry.GetServiceName() }
			}));

		if ( !string.IsNullOrWhiteSpace(appSettings.OpenTelemetry.TracesEndpoint) )
		{
			// AddEntityFrameworkCoreInstrumentation from OpenTelemetry.Instrumentation.EntityFrameworkCore
			telemetryBuilder.WithTracing(tracing => tracing
				.AddAspNetCoreInstrumentation(o => o.Filter = FilterPath)
				.AddEntityFrameworkCoreInstrumentation(p =>
				{
					// DbStatementForText can contain sensitive data
#if DEBUG
					p.SetDbStatementForText = true;
#else
					p.SetDbStatementForText = false;
#endif
				})
				.AddOtlpExporter(o =>
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

		// AddHttpClientInstrumentation from OpenTelemetry.Instrumentation.Http
		telemetryBuilder.WithMetrics(metrics =>
			metrics
				.AddAspNetCoreInstrumentation()
				.AddRuntimeInstrumentation()
				.AddMeter(ActivitySourceMeter.SyncNameSpace)
				.AddMeter(ActivitySourceMeter.WorkerNameSpace)
				.AddOtlpExporter(o =>
				{
					o.ExportProcessorType = ExportProcessorType.Batch;
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
		if ( ( context.Request.Path.Value?.EndsWith("/realtime") == true ||
		       context.Request.Path.Value?.EndsWith("/api/health") == true ||
		       context.Request.Path.Value?.EndsWith("/api/health/details") == true ||
		       context.Request.Path.Value?.EndsWith("/api/open-telemetry/trace") == true )
		     && context.Response.StatusCode == 200 )
		{
			return false;
		}

		if ( context.Request.Path.Value?.EndsWith("/api/index") == true
		     && context.Response.StatusCode == 401 )
		{
			return false;
		}

		return true;
	}
}

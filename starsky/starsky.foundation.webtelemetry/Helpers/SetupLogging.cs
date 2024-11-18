using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;

namespace starsky.foundation.webtelemetry.Helpers;

public static class SetupLogging
{
	private const string HostNameKey = "host.name";
	private static readonly KeyValuePair<string, object> HostNameKeyValue = new(HostNameKey,
		Environment.MachineName);

	[SuppressMessage("Usage", "S4792:Make sure that this logger's configuration is safe.")]
	public static void AddTelemetryLogging(this IServiceCollection services,
		AppSettings appSettings)
	{
		services.AddLogging(logging =>
		{
			logging.ClearProviders();
			logging.AddConsole();

			if ( !string.IsNullOrEmpty(appSettings.OpenTelemetry?.LogsEndpoint) )
			{
				logging.AddOpenTelemetry(
					builder =>
						builder.AddOtlpExporter(
								options =>
								{
									options.Protocol = OtlpExportProtocol.HttpProtobuf;
									options.Headers = appSettings.OpenTelemetry.GetLogsHeader();
									options.Endpoint =
										new Uri(appSettings.OpenTelemetry.LogsEndpoint);
								})
							.SetResourceBuilder(
								ResourceBuilder.CreateDefault()
									.AddService(appSettings.OpenTelemetry.GetServiceName())
									.AddAttributes([HostNameKeyValue])
							)
				);
			}
		});

		services.AddScoped<IWebLogger, WebLogger>();
	}
}

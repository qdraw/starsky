using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
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
	internal const string HostNameKey = "host.name";
	internal const string DeploymentEnvironmentName = "deployment.environment";
	internal const string AppVersionName = "service.version";
	internal const string AppVersionBuildDateTimeName = "service.build_datetime";
	internal const string FrameworkDescriptionName = "runtime.framework";

	[SuppressMessage("Usage", "S4792:Make sure that this logger's configuration is safe.")]
	public static void AddTelemetryLogging(this IServiceCollection services,
		AppSettings appSettings)
	{
		services.AddLogging(logging =>
		{
			logging.ClearProviders();
			logging.AddConsole();

			if ( string.IsNullOrEmpty(appSettings.OpenTelemetry?.LogsEndpoint) )
			{
				return;
			}

			logging.AddOpenTelemetry(builder =>
				builder.AddOtlpExporter(options =>
					{
						options.Protocol = OtlpExportProtocol.HttpProtobuf;
						options.Headers = appSettings.OpenTelemetry.GetLogsHeader();
						options.Endpoint =
							new Uri(appSettings.OpenTelemetry.LogsEndpoint);
					})
					.SetResourceBuilder(
						ResourceBuilder.CreateDefault()
							.AddService(appSettings.OpenTelemetry.GetServiceName())
							.AddAttributes(GetTelemetryAttributes(appSettings))
					)
			);
		});

		services.AddScoped<IWebLogger, WebLogger>();
	}

	internal static List<KeyValuePair<string, object>> GetTelemetryAttributes(
		AppSettings appSettings)
	{
		return
		[
			new KeyValuePair<string, object>(HostNameKey, Environment.MachineName),
			// ASPNETCORE_ENVIRONMENT
			new KeyValuePair<string, object>(DeploymentEnvironmentName,
				appSettings.OpenTelemetry!.GetEnvironmentName()),
			new KeyValuePair<string, object>(AppVersionName, appSettings.AppVersion),
			new KeyValuePair<string, object>(AppVersionBuildDateTimeName,
				appSettings.AppVersionBuildDateTime.ToString(
					new CultureInfo("nl-NL"))),
			new KeyValuePair<string, object>(FrameworkDescriptionName,
				RuntimeInformation.FrameworkDescription),
		];
	}
}

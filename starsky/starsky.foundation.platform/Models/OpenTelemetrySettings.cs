using System;

namespace starsky.foundation.platform.Models;

public class OpenTelemetrySettings
{
	public string? Header { get; set; }

	/// <summary>
	/// Which container is the logging stored
	/// </summary>
	public string? ServiceName { get; set; }
	public string? TracesEndpoint { get; set; }
	public string? TracesHeader { get; set; }

	public string? MetricsEndpoint { get; set; }
	public string? MetricsHeader { get; set; }

	public string? LogsEndpoint { get; set; }
	
	/// <summary>
	/// The header for the logs endpoint
	/// use apikey=value for the header name apikey
	/// </summary>
	public string? LogsHeader { get; set; }
	
	/// <summary>
	/// Overwrite the `deployment.environment` value
	/// </summary>
	public string? EnvironmentName { get; set; }

	/// <summary>
	/// Give back the EnvironmentName or if empty the ASPNETCORE_ENVIRONMENT variable
	/// </summary>
	/// <returns>EnvironmentName or if empty the ASPNETCORE_ENVIRONMENT variable</returns>
	public string GetEnvironmentName()
	{
		return string.IsNullOrWhiteSpace(EnvironmentName)
			? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "production"
			: EnvironmentName;
	}

	public string GetServiceName()
	{
		return string.IsNullOrWhiteSpace(ServiceName)
			? "Starsky"
			: ServiceName;
	}

	public string? GetLogsHeader()
	{
		return string.IsNullOrWhiteSpace(LogsHeader)
			? Header
			: LogsHeader;
	}

	public string? GetMetricsHeader()
	{
		return string.IsNullOrWhiteSpace(MetricsHeader)
			? Header
			: MetricsHeader;
	}

	public string? GetTracesHeader()
	{
		return string.IsNullOrWhiteSpace(TracesHeader)
			? Header
			: TracesHeader;
	}
}

namespace starsky.foundation.platform.Models;

public class OpenTelemetrySettings
{
	public string Header { get; set; }

	public string ServiceName { get; set; }
	public string TracesEndpoint { get; set; }
	public string TracesHeader { get; set; }

	public string MetricsEndpoint { get; set; }
	public string MetricsHeader { get; set; }

	public string LogsEndpoint { get; set; }
	public string LogsHeader { get; set; }

	public string GetServiceName()
	{
		return string.IsNullOrWhiteSpace(ServiceName)
			? "Starsky"
			: ServiceName ;
	}

	public string GetLogsHeader()
	{
		return string.IsNullOrWhiteSpace(LogsHeader)
			? Header
			: LogsHeader;
	}
	
	public string GetMetricsHeader()
	{
		return string.IsNullOrWhiteSpace(MetricsHeader)
			? Header
			: MetricsHeader;
	}

	public string GetTracesHeader()
	{
		return string.IsNullOrWhiteSpace(TracesHeader)
			? Header
			: TracesHeader;
	}
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class OpenTelemetrySettingsTest
{
	[TestMethod]
	public void ServiceNameDefault()
	{
		var result = new OpenTelemetrySettings().GetServiceName();
		Assert.AreEqual("Starsky", result);
	}

	[TestMethod]
	public void ServiceNameProperty()
	{
		var result = new OpenTelemetrySettings { ServiceName = "test-service" }.GetServiceName();

		Assert.AreEqual("test-service", result);
	}

	[TestMethod]
	public void GetLogsHeaderDefault()
	{
		var result = new OpenTelemetrySettings().GetLogsHeader();
		Assert.IsNull(result);
	}

	[TestMethod]
	public void GetLogsHeaderFallback()
	{
		var result = new OpenTelemetrySettings { Header = "logs" }.GetLogsHeader();
		Assert.AreEqual("logs", result);
	}

	[TestMethod]
	public void GetLogsShowProperty()
	{
		var result = new OpenTelemetrySettings { LogsHeader = "logs" }.GetLogsHeader();
		Assert.AreEqual("logs", result);
	}

	[TestMethod]
	public void GetMetricsHeaderDefault()
	{
		var result = new OpenTelemetrySettings().GetMetricsHeader();
		Assert.IsNull(result);
	}

	[TestMethod]
	public void GetMetricsHeaderFallback()
	{
		var result = new OpenTelemetrySettings { Header = "metrics" }.GetMetricsHeader();
		Assert.AreEqual("metrics", result);
	}

	[TestMethod]
	public void GetMetricsShowProperty()
	{
		var result = new OpenTelemetrySettings { MetricsHeader = "metrics" }.GetMetricsHeader();
		Assert.AreEqual("metrics", result);
	}

	[TestMethod]
	public void GetTracesHeaderDefault()
	{
		var result = new OpenTelemetrySettings().GetTracesHeader();
		Assert.IsNull(result);
	}

	[TestMethod]
	public void GetTracesHeaderFallback()
	{
		var result = new OpenTelemetrySettings { Header = "traces" }.GetTracesHeader();
		Assert.AreEqual("traces", result);
	}

	[TestMethod]
	public void GetTracesShowProperty()
	{
		var result = new OpenTelemetrySettings { MetricsHeader = "traces" }.GetMetricsHeader();
		Assert.AreEqual("traces", result);
	}

	[TestMethod]
	[DataRow("Development", null, "Development")]
	[DataRow(null, "Staging", "Staging")]
	[DataRow(null, null, "production")]
	public void GetEnvironmentName_TheoryTests(string? environmentName, string? environmentVariable,
		string expected)
	{
		// Arrange
		Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environmentVariable);
		var settings = new OpenTelemetrySettings { EnvironmentName = environmentName };

		// Act
		var result = settings.GetEnvironmentName();

		// Assert
		Assert.AreEqual(expected, result);

		// Cleanup
		Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
	}
}

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
		Assert.AreEqual("Starsky",result);
	}
	
	[TestMethod]
	public void ServiceNameProperty()
	{
		var result = new OpenTelemetrySettings
		{
			ServiceName = "test-service"
		}.GetServiceName();
		
		Assert.AreEqual("test-service",result);
	}
	
	[TestMethod]
	public void GetLogsHeaderDefault()
	{
		var result = new OpenTelemetrySettings().GetLogsHeader();
		Assert.AreEqual(null,result);
	}
	
	[TestMethod]
	public void GetLogsHeaderFallback()
	{
		var result = new OpenTelemetrySettings{Header = "logs"}.GetLogsHeader();
		Assert.AreEqual("logs",result);
	}
	
	[TestMethod]
	public void GetLogsShowProperty()
	{
		var result = new OpenTelemetrySettings{LogsHeader = "logs"}.GetLogsHeader();
		Assert.AreEqual("logs",result);
	}
	
	[TestMethod]
	public void GetMetricsHeaderDefault()
	{
		var result = new OpenTelemetrySettings().GetMetricsHeader();
		Assert.AreEqual(null,result);
	}
	
	[TestMethod]
	public void GetMetricsHeaderFallback()
	{
		var result = new OpenTelemetrySettings{Header = "metrics"}.GetMetricsHeader();
		Assert.AreEqual("metrics",result);
	}
	
	[TestMethod]
	public void GetMetricsShowProperty()
	{
		var result = new OpenTelemetrySettings{MetricsHeader = "metrics"}.GetMetricsHeader();
		Assert.AreEqual("metrics",result);
	}
	
	[TestMethod]
	public void GetTracesHeaderDefault()
	{
		var result = new OpenTelemetrySettings().GetTracesHeader();
		Assert.AreEqual(null,result);
	}
	
	[TestMethod]
	public void GetTracesHeaderFallback()
	{
		var result = new OpenTelemetrySettings{Header = "traces"}.GetTracesHeader();
		Assert.AreEqual("traces",result);
	}
	
	[TestMethod]
	public void GetTracesShowProperty()
	{
		var result = new OpenTelemetrySettings{MetricsHeader = "traces"}.GetMetricsHeader();
		Assert.AreEqual("traces",result);
	}
}

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
	public void GetLogsHeaderDefault()
	{
		var result = new OpenTelemetrySettings().GetLogsHeader();
		Assert.AreEqual(null,result);
	}
	
	[TestMethod]
	public void GetLogsHeaderFallback()
	{
		var result = new OpenTelemetrySettings{Header = "1"}.GetLogsHeader();
		Assert.AreEqual("1",result);
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
		var result = new OpenTelemetrySettings{Header = "1"}.GetMetricsHeader();
		Assert.AreEqual("1",result);
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
		var result = new OpenTelemetrySettings{Header = "1"}.GetTracesHeader();
		Assert.AreEqual("1",result);
	}
}

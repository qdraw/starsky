using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.webtelemetry.Helpers;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

[TestClass]
public class MetricsHelperTest
{
	[TestMethod]
	public void Add_Null_isFalse()
	{
		var value = MetricsHelper.Add(null,"test",1);
		Assert.AreEqual(false, value);
	}
	
	[TestMethod]
	public void Add_TelemetrySet()
	{
		var testGuid = Guid.NewGuid().ToString();
		var value = MetricsHelper.Add(new TelemetryClient(new TelemetryConfiguration()
		{
			ConnectionString = $"InstrumentationKey={testGuid}",
		}),"test",1);
		
		Assert.AreEqual(true, value);
	}
}

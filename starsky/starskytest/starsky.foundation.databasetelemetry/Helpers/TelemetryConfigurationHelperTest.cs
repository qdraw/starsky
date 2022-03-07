using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.databasetelemetry.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.databasetelemetry.Helpers
{
	[TestClass]
	public class TelemetryConfigurationHelperTest
	{
		[TestMethod]
		public void InitTelemetryClientTest()
		{
			var result = TelemetryConfigurationHelper.InitTelemetryClient(
				"test","role", new FakeIWebLogger(), null);
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void UseExistingTelemetryClient()
		{
			var testGuid = Guid.NewGuid().ToString();
			var telemetryClient = new TelemetryClient(
					new TelemetryConfiguration(testGuid));
			
			var result = TelemetryConfigurationHelper.InitTelemetryClient(
				"test","role", new FakeIWebLogger(), telemetryClient);
			Assert.AreEqual(testGuid, result?.TelemetryConfiguration.InstrumentationKey);
		}
	}
}

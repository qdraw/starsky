using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Services;

namespace starskytest.starsky.foundation.webtelemetry.Services
{
	[TestClass]
	public sealed class TelemetryServiceTest
	{
		[TestMethod]
		public void TelemetryService_WithContent()
		{
			var testGuid = Guid.NewGuid().ToString();
			var result = new TelemetryService(new AppSettings
			{
				ApplicationInsightsConnectionString = $"InstrumentationKey={testGuid}"
			}).TrackException(new Exception("test"));
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void TelemetryService_Disabled()
		{
			var result = new TelemetryService(new AppSettings
			{
				ApplicationInsightsConnectionString = null!
			}).TrackException(new Exception("test"));
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void TelemetryService_NullDisabled()
		{
			var result = new TelemetryService(null).TrackException(new Exception("test"));
			Assert.IsFalse(result);
		}
	}
}

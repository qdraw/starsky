using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;

namespace starskytest.starsky.foundation.platform.Services
{
	[TestClass]
	public class TelemetryServiceTest
	{
		[TestMethod]
		public void TelemetryService_WithContent()
		{
			var result = new TelemetryService(new AppSettings
			{
				ApplicationInsightsInstrumentationKey = "_some"
			}).TrackException(new Exception("test"));
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void TelemetryService_Disabled()
		{
			var result = new TelemetryService(new AppSettings
			{
				ApplicationInsightsInstrumentationKey = null
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

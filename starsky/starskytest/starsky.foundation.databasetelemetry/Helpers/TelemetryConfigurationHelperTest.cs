using System;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.databasetelemetry.Helpers;
using starsky.foundation.platform.Interfaces;
using starskytest.FakeMocks;
using starskytest.Helpers;

namespace starskytest.starsky.foundation.databasetelemetry.Helpers
{
	[TestClass]
	public sealed class TelemetryConfigurationHelperTest
	{
		[TestMethod]
		public void InitTelemetryClientTest()
		{
			var testGuid = Guid.NewGuid().ToString();

			var result = TelemetryConfigurationHelper.InitTelemetryClient(
				$"InstrumentationKey={testGuid}","role", new FakeIWebLogger(), null);
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void UseExistingTelemetryClient()
		{
			var testGuid = Guid.NewGuid().ToString();
		
			var mockTelemetryChannel = new MockTelemetryChannel();
			var configuration = new TelemetryConfiguration
			{
				TelemetryChannel = mockTelemetryChannel,
				ConnectionString = $"InstrumentationKey={testGuid}",
			};
			
			var telemetryClient = new TelemetryClient(configuration);
			
			var result = TelemetryConfigurationHelper.InitTelemetryClient(
				"test","role", new FakeIWebLogger(), telemetryClient);
			Assert.AreEqual(testGuid, result?.TelemetryConfiguration.InstrumentationKey);
		}
		
		[TestMethod]
		public void Clean_Exception_LogsExceptionMessageAndReturnsNull()
		{
			// Arrange
			var expectedMessage = "test exception";
			var exception = new Exception(expectedMessage);
			IWebLogger logger = new FakeIWebLogger();
            
			// Act
			var result = TelemetryConfigurationHelper.Clean(exception, logger);
            
			// Assert
			Assert.IsNull(result);
			Assert.AreEqual(2, ((FakeIWebLogger)logger).TrackedInformation.Count);
			Assert.IsTrue(((FakeIWebLogger)logger).TrackedInformation.LastOrDefault().Item2.Contains(expectedMessage));
		}
	}
}

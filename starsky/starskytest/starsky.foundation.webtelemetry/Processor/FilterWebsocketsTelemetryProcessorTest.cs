using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.webtelemetry.Processor;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.webtelemetry.Processor
{
	[TestClass]
	public class FilterWebsocketsTelemetryProcessorTest
	{
		[TestMethod]
		public void IgnoreWhenWebsocket()
		{
			var tel = new RequestTelemetry { ResponseCode = "101" };
			var fakeITelemetryProcessor = new FakeITelemetryProcessor();
			new FilterWebsocketsTelemetryProcessor(fakeITelemetryProcessor).Process(tel);
			Assert.AreEqual(0,fakeITelemetryProcessor.Received.Count);
		}
		
		[TestMethod]
		public void SetWhenStatus200()
		{
			var tel = new RequestTelemetry { ResponseCode = "200" };
			var fakeITelemetryProcessor = new FakeITelemetryProcessor();
			new FilterWebsocketsTelemetryProcessor(fakeITelemetryProcessor).Process(tel);
			Assert.AreEqual(1,fakeITelemetryProcessor.Received.Count);
		}
	}
}

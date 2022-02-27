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
			var result = TelemetryConfigurationHelper.InitTelemetryClient("test","role", new FakeIWebLogger());
			Assert.IsNotNull(result);
		}
	}
}

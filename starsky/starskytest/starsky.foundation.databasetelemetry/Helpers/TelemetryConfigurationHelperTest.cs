using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.databasetelemetry.Helpers;

namespace starskytest.starsky.foundation.databasetelemetry.Helpers
{
	[TestClass]
	public class TelemetryConfigurationHelperTest
	{
		[TestMethod]
		public void InitTelemetryClientTest()
		{
			var result = TelemetryConfigurationHelper.InitTelemetryClient("test","role");
			Assert.IsNotNull(result);
		}
	}
}

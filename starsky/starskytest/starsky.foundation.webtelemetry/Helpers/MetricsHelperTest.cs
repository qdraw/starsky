using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.webtelemetry.Helpers;

namespace starskytest.starsky.foundation.webtelemetry.Helpers;

[TestClass]
public class MetricsHelperTest
{
	[TestMethod]
	public void Add_Null_isFalse()
	{
		var value = MetricsHelper.Add("test", "test", 1);
		Assert.AreEqual(false, value);
	}
}

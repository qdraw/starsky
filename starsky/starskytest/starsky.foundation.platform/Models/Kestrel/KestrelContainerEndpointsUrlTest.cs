using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models.Kestrel;

namespace starskytest.starsky.foundation.platform.Models.Kestrel;

[TestClass]
public class KestrelContainerEndpointsUrlTest
{
	[TestMethod]
	public void KestrelContainerEndpointsUrlTest_Default()
	{
		var kestrelContainer = new KestrelContainerEndpointsUrl();
		Assert.IsNotNull(kestrelContainer);
		Assert.IsNull(kestrelContainer.Url);
	}
}

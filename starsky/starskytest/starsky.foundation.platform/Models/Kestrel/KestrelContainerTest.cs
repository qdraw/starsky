using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models.Kestrel;

namespace starskytest.starsky.foundation.platform.Models.Kestrel;

[TestClass]
public class KestrelContainerTest
{
	[TestMethod]
	public void KestrelContainerTest_Default()
	{
		var kestrelContainer = new KestrelContainer();
		Assert.IsNotNull(kestrelContainer);
		Assert.IsNull(kestrelContainer.Endpoints);
	}
	
	[TestMethod]
	public void KestrelContainerTest_Default1()
	{
		var kestrelContainer = new KestrelContainer
		{
			Endpoints = new KestrelContainerEndpoints()
		};
		Assert.IsNotNull(kestrelContainer);
		Assert.IsNotNull(kestrelContainer.Endpoints);
	}
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models.Kestrel;

namespace starskytest.starsky.foundation.platform.Models.Kestrel;

[TestClass]
public class KestrelContainerEndpointsTest
{
	[TestMethod]
	public void KestrelContainerEndpointsTest_Default()
	{
		var kestrelContainer = new KestrelContainerEndpoints();
		Assert.IsNotNull(kestrelContainer);
		Assert.IsNull(kestrelContainer.Https);
		Assert.IsNull(kestrelContainer.Http);
	}
	
	[TestMethod]
	public void KestrelContainerEndpointsTest_WithHttps()
	{
		var kestrelContainer = new KestrelContainerEndpoints
		{
			Https = new KestrelContainerEndpointsUrl
			{
				Url = "https://localhost:5001"
			}
		};
		
		Assert.IsNotNull(kestrelContainer);
		Assert.IsNotNull(kestrelContainer.Https);
		Assert.IsNull(kestrelContainer.Http);
		Assert.AreEqual("https://localhost:5001",kestrelContainer.Https.Url);
	}
		
	[TestMethod]
	public void KestrelContainerEndpointsTest_WithHttp()
	{
		var kestrelContainer = new KestrelContainerEndpoints
		{
			Http = new KestrelContainerEndpointsUrl
			{
				Url = "http://localhost:5001"
			}
		};
		
		Assert.IsNotNull(kestrelContainer);
		Assert.IsNotNull(kestrelContainer.Http);
		Assert.IsNull(kestrelContainer.Https);
		Assert.AreEqual("http://localhost:5001",kestrelContainer.Http.Url);
	}
}

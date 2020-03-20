using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.injection;

namespace starskytest.starsky.foundation.injection
{
	[TestClass]
	public class ServiceAttributeTest
	{
		[TestMethod]
		public void ServiceAttribute_defaultOption()
		{
			Assert.AreEqual(InjectionLifetime.Scoped,new ServiceAttribute().InjectionLifetime);
		}
	}
}

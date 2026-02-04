using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class AppSettingsKeyValueTest
{
	[TestMethod]
	public void AppSettingsKeyValue1_Deconstruct()
	{
		var model = new AppSettingsKeyValue
		{
			Key = "1",
			Value = "2"
		};

		var (key, value) = model;
		
		Assert.AreEqual("1",key);
		Assert.AreEqual("2",value);
	}
}

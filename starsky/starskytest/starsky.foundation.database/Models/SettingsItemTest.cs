using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public class SettingsItemTest
{
	[TestMethod]
	public void SettingsItemTestExist()
	{
		var item = new SettingsItem { IsUserEditable = false, UserId = 1 };
		Assert.AreEqual(false,item.IsUserEditable);
	}
	
	[TestMethod]
	public void SettingsItemTestExist2()
	{
		var item = new SettingsItem { IsUserEditable = false, UserId = 1 };
		Assert.AreEqual(1,item.UserId);
	}
}

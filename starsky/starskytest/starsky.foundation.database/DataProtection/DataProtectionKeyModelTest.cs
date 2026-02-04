using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.DataProtection;

[TestClass]
public class DataProtectionKeyModelTest
{
	[TestMethod]
	public void IdTest()
	{
		var model = new DataProtectionKey { Id = 1 };
		Assert.AreEqual(1, model.Id);
	}
}

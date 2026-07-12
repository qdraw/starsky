using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public class InstanceIdTest
{
	[TestMethod]
	public void FileIndexItemTest_CreateInstanceId()
	{
		var value = InstanceId.CreateNewInstanceId();
		Assert.IsTrue(value.StartsWith("xmp.iid:", StringComparison.Ordinal));
		Assert.IsTrue(Guid.TryParse(value.Replace("xmp.iid:", string.Empty), out _));
	}
}

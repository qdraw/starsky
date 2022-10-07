using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public class ReflectionValueHelperTest
{
	private class Foo
	{
#pragma warning disable CS0414
		private string _bar = "test";
#pragma warning restore CS0414
	}

	private class FooNothing
	{
	}
	
	[TestMethod]
	public void TestReadField()
	{
		var result = new Foo().GetReflectionFieldValue<string>("_bar");
		Assert.AreEqual("test", result);
	}
	
	[TestMethod]
	public void TestReadFieldNothing()
	{
		var result = new FooNothing().GetReflectionFieldValue<string>("_bar");
		Assert.AreEqual(null, result);
	}
}

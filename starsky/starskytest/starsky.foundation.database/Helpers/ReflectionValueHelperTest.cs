using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public class ReflectionValueHelperTest
{
	[SuppressMessage("Usage",
		"S1144:Unused private types or members should be removed")]
	[SuppressMessage("Usage",
		"S2933:Fields that are only assigned in the constructor should be \"readonly\"")]
	private class Foo
	{
#pragma warning disable CS0414
		private string _bar = "test";
#pragma warning restore CS0414
	}

	[SuppressMessage("Usage", "S2094:Classes should not be empty")]
	private class FooNothing
	{
		// nothing here
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

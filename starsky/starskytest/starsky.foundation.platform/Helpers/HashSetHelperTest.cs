using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class HashSetHelperTest
{
	[TestMethod]
	public void HashSetHelperTest_NoContent()
	{
		var result = HashSetHelper.StringToHashSet(string.Empty);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void HashSetHelperTest_HashSetDuplicateContent()
	{
		// hashsets does not support duplicates, but test the comma implementation
		var result = HashSetHelper.StringToHashSet("test, test");
		var actualResult = HashSetHelper.HashSetToString(result);
		Assert.AreEqual("test", actualResult);
	}

	[TestMethod]
	public void HashSetHelperTest_HashSetDoubleComma()
	{
		// testing with double commas those are not supported by the c# exif read tool
		var result = HashSetHelper.StringToHashSet(",,,,,test,, test1");
		var actualResult = HashSetHelper.HashSetToString(result);
		Assert.AreEqual("test, test1", actualResult);
	}

	[TestMethod]
	public void HashSetHelperTest_HashSetSingleComma()
	{
		// testing with double commas those are not supported by the c# exif read tool
		var result = HashSetHelper.StringToHashSet("test,test1");
		var actualResult = HashSetHelper.HashSetToString(result);
		Assert.AreEqual("test, test1", actualResult);
	}

	[TestMethod]
	public void HashSetHelperTest_DoubleSpaces()
	{
		var hashSetResult = HashSetHelper.StringToHashSet("test0,   test1 , test2,  test3");
		Assert.HasCount(4, hashSetResult);

		Assert.AreEqual("test0", hashSetResult.ToList()[0]);
		Assert.AreEqual("test1", hashSetResult.ToList()[1]);
		Assert.AreEqual("test2", hashSetResult.ToList()[2]);
		Assert.AreEqual("test3", hashSetResult.ToList()[3]);
	}

	[TestMethod]
	public void HashSetHelperTest_SetToStringNull()
	{
		Assert.AreEqual(string.Empty, HashSetHelper.HashSetToString(null!));
	}

	[TestMethod]
	public void HashSetHelperTest_ListToStringNullTest()
	{
		var item = HashSetHelper.ListToString(null!);
		Assert.AreEqual(0, item.Length);
	}
}

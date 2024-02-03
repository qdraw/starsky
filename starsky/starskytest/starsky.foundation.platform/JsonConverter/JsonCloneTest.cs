using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.foundation.platform.JsonConverter;

[TestClass]
public class JsonCloneTest
{
	private class MyClass
	{
		public int? Number { get; set; }
	}
	
	[TestMethod]
	public void ShouldCopyValue()
	{
		var testClass = new MyClass
		{
			Number = 1
		};
		var result = testClass.CloneViaJson();
		
		Assert.AreEqual(1, result?.Number);
	}
	
	[TestMethod]
	public void ShouldCopyNullValue()
	{
		MyClass? testClass = null;
		
		var result = testClass.CloneViaJson();
		
		Assert.IsNull(result);
	}
}

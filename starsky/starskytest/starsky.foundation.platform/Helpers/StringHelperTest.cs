using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers
{
	[TestClass]
	public class StringHelperTest
	{
		[TestMethod]
		public void AsciiNullReplacer_01()
		{
			var result = StringHelper.AsciiNullReplacer("\\0");
			Assert.AreEqual(string.Empty,result);
		}
		
		[TestMethod]
		public void AsciiNullReplacer_02()
		{
			var result = StringHelper.AsciiNullReplacer("\\\\0");
			Assert.AreEqual(string.Empty,result);
		}
		
		[TestMethod]
		public void AsciiNullReplacer_03()
		{
			var result = StringHelper.AsciiNullReplacer("\\\\0test");
			Assert.AreEqual("\\\\0test",result);
		}
	}
}

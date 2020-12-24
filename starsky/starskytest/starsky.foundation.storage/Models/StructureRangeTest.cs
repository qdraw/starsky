using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Models;

namespace starskytest.starsky.foundation.storage.Models
{
	[TestClass]
	public class StructureRangeTest
	{
		[TestMethod]
		public void StructureRangePattern()
		{
			var result = new StructureRange {Pattern = "test"};
			Assert.AreEqual("test",result.Pattern);
		}
		
		[TestMethod]
		public void StructureRangeEnd()
		{
			var result = new StructureRange {End = 1};
			Assert.AreEqual(1,result.End);
		}
	}
}

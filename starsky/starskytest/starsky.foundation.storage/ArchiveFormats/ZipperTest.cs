using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.ArchiveFormats;

namespace starskytest.starsky.foundation.storage.ArchiveFormats
{
	[TestClass]
	public class ZipperTest
	{
		[TestMethod]
		public void NotFound()
		{
			var result =  new Zipper().ExtractZip("not-found","t");
			Assert.IsFalse(result);
		}
	}
}

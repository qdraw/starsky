using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services
{
	[TestClass]
	public class ThumbnailServiceTest
	{
		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public void NotFound()
		{
			new ThumbnailService(new FakeSelectorStorage()).CreateThumb("/not-found");
			// expect exception not found
		}
		
		[TestMethod]
		public void NotFoundNonExistingHash()
		{
			var result = new ThumbnailService(new FakeSelectorStorage())
				.CreateThumb("/not-found","non-existing-hash");
			Assert.IsFalse(result);
		}
	}
}

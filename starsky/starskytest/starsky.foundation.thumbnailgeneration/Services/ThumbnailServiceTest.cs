using System.IO;
using System.Threading.Tasks;
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
		public async Task NotFound()
		{
			await new ThumbnailService(new FakeSelectorStorage()).CreateThumb("/not-found");
			// expect exception not found
		}
		
		[TestMethod]
		public async Task NotFoundNonExistingHash()
		{
			var result = await new ThumbnailService(new FakeSelectorStorage())
				.CreateThumb("/not-found","non-existing-hash");
			Assert.IsFalse(result);
		}
	}
}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services
{
	[TestClass]
	public sealed class ThumbnailServiceTest
	{
		[TestMethod]
		public async Task NotFound()
		{
			var resultModels = await new ThumbnailService(new FakeSelectorStorage(), 
				new FakeIWebLogger(), new AppSettings()).CreateThumbnailAsync("/not-found");
			
			Assert.IsFalse(resultModels.FirstOrDefault()!.Success);
		}
		
		[TestMethod]
		public async Task NotFoundNonExistingHash()
		{
			var result = await new ThumbnailService(new FakeSelectorStorage(), new FakeIWebLogger(), new AppSettings())
				.CreateThumbAsync("/not-found","non-existing-hash");
			Assert.IsFalse(result.FirstOrDefault()!.Success);
		}
	}
}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services;

[TestClass]
public sealed class ThumbnailServiceTest
{
	[TestMethod]
	public async Task NotFound()
	{
		var sut = new ThumbnailService(new FakeSelectorStorage(),
			new FakeIWebLogger(), new AppSettings(),
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery()));
		var resultModels = await sut.GenerateThumbnail("/not-found");

		Assert.IsFalse(resultModels.FirstOrDefault()!.Success);
	}

	[TestMethod]
	public async Task NotFoundNonExistingHash()
	{
		var sut = new ThumbnailService(new FakeSelectorStorage(),
			new FakeIWebLogger(), new AppSettings(),
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery()));
		var result = await sut.GenerateThumbnail("/not-found", "non-existing-hash");
		Assert.IsFalse(result.FirstOrDefault()!.Success);
	}
}

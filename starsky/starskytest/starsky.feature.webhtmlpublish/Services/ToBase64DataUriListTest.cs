using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageCorrupt;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public sealed class ToBase64DataUriListTest
{
	[TestMethod]
	public async Task TestIfContainsDataImageBaseHash()
	{
		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" }, new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var thumbnailService = new ThumbnailService(new FakeSelectorStorage(fakeStorage),
			new FakeIWebLogger(), new AppSettings(),
			new FakeIUpdateStatusGeneratedThumbnailService(), new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(fakeStorage), new FakeIWebLogger()));

		var result = await new ToBase64DataUriList(thumbnailService)
			.Create(
				new List<FileIndexItem> { new("/test.jpg") });
		Assert.IsTrue(result[0].Contains("data:image/png;base64,"));
	}

	[TestMethod]
	public async Task TestIfContainsDataImageBaseHash_CorruptOutput()
	{
		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { new CreateAnImageCorrupt().Bytes.ToArray() });
		var thumbnailService = new ThumbnailService(new FakeSelectorStorage(fakeStorage),
			new FakeIWebLogger(), new AppSettings(),
			new FakeIUpdateStatusGeneratedThumbnailService(), new FakeIVideoProcess(),
			new FileHashSubPathStorage(new FakeSelectorStorage(fakeStorage), new FakeIWebLogger()));

		var result = await new ToBase64DataUriList(thumbnailService)
			.Create(
				new List<FileIndexItem> { new("/test.jpg") });
		// to fallback image (1px x 1px)
		Assert.AreEqual("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAA" +
		                "C1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=", result[0]);
	}
}

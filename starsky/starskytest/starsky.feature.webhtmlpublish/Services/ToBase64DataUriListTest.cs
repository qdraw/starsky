using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageCorrupt;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public sealed class ToBase64DataUriListTest
{
	private static ThumbnailService SetThumbnailService(IStorage storage)
	{
		return new ThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings(),
			new FakeIUpdateStatusGeneratedThumbnailService(),
			new FileHashSubPathStorage(new FakeSelectorStorage(storage), new FakeIWebLogger()),
			new ThumbnailGeneratorFactory(new FakeSelectorStorage(storage), new FakeIWebLogger(),
				new FakeIVideoProcess(new FakeSelectorStorage(storage)),
				new FakeINativePreviewThumbnailGenerator(),
				new EmbeddedRawThumbnailGenerator(new FakeSelectorStorage(storage),
					new FakeEmbeddedRawThumbnailService(new FakeSelectorStorage(storage)),
					new FakeIWebLogger())));
	}

	[TestMethod]
	public async Task TestIfContainsDataImageBaseHash()
	{
		var fakeStorage = new FakeIStorage(["/"],
			["/test.jpg"], new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var thumbnailService = SetThumbnailService(fakeStorage);

		var result = await new ToBase64DataUriList(thumbnailService)
			.Create(
				[new FileIndexItem("/test.jpg")]);
		Assert.Contains("data:image/png;base64,", result[0]);
	}

	[TestMethod]
	public async Task TestIfContainsDataImageBaseHash_CorruptOutput()
	{
		var fakeStorage = new FakeIStorage(["/"],
			["/test.jpg"],
			new List<byte[]> { new CreateAnImageCorrupt().Bytes.ToArray() });
		var thumbnailService = SetThumbnailService(fakeStorage);

		var result = await new ToBase64DataUriList(thumbnailService)
			.Create(
				[new FileIndexItem("/test.jpg")]);
		// to fallback image (1px x 1px)
		Assert.AreEqual("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAA" +
		                "C1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=", result[0]);
	}
}

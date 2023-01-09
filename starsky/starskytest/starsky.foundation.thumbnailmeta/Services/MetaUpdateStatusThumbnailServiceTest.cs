using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailmeta.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailmeta.Services;

[TestClass]
public class MetaUpdateStatusThumbnailServiceTest
{
	[TestMethod]
	public async Task MetaUpdateStatusThumbnailService()
	{
		var query = new FakeIThumbnailQuery();
		var service = new MetaUpdateStatusThumbnailService(query,
			new FakeSelectorStorage());
		await service.UpdateStatusThumbnail(new List<(bool, bool, string, string)>
		{
			( true, true, "/test.jpg", "test" ),
			( false, true, "/false.jpg", "test1" ),
			( false, false, "/false.mp4", "test1" )
		});

		var thumbnailItems = await query.Get();
		Assert.AreEqual(2, thumbnailItems.Count);
	}
}

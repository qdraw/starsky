using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.sync.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Helpers;

[TestClass]
public class NewItemTest
{
	[TestMethod]
	public async Task NewItemTest_KeepDefault()
	{
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var newItem =
			await new NewItem(storage, new FakeReadMeta(), new FakeIWebLogger())
				.PrepareUpdateFileItemAsync(
					new FileIndexItem("/test.jpg") { LastChanged = new List<string> { "test" } },
					100);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, newItem.Status);

		Assert.AreEqual(100, newItem.Size);
		Assert.IsTrue(newItem.InstanceId!.StartsWith("xmp.iid:", StringComparison.Ordinal));
		Assert.IsTrue(Guid.TryParse(newItem.InstanceId.Replace("xmp.iid:", string.Empty),
			out _));
	}

	[TestMethod]
	public async Task NewItemTest_SetOkAndSame()
	{
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string> { "/test.jpg" });
		var newItem =
			await new NewItem(storage, new FakeReadMeta(), new FakeIWebLogger())
				.PrepareUpdateFileItemAsync(
					new FileIndexItem("/test.jpg")
					{
						Tags = "test, fake read meta", LastChanged = new List<string>()
					}, 100);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, newItem.Status);

		Assert.AreEqual(100, newItem.Size);
		Assert.IsTrue(newItem.InstanceId!.StartsWith("xmp.iid:", StringComparison.Ordinal));
		Assert.IsTrue(Guid.TryParse(newItem.InstanceId.Replace("xmp.iid:", string.Empty),
			out _));
	}
}

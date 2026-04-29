using System.Collections.Generic;
using System.Text;
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

		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, newItem.Status);

		Assert.AreEqual(100, newItem.Size);
	}

	[TestMethod]
	public async Task NewFileItemAsync_WithJsonSidecar_AppliesTagsFromSidecar()
	{
		// Arrange: file + JSON sidecar with specific tags
		const string jsonTags = "sync-json-sidecar-tags";
		var jsonContent = $"{{\"item\":{{\"tags\":\"{jsonTags}\"}}}}";
		var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);

		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/.starsky.test.jpg.json" },
			new List<byte[]> { new byte[100], jsonBytes });

		var newItem = await new NewItem(storage, new FakeReadMeta(), new FakeIWebLogger())
			.NewFileItemAsync(new FileIndexItem("/test.jpg"));

		Assert.AreEqual(jsonTags, newItem.Tags);
	}

	[TestMethod]
	public async Task NewFileItemAsync_WithoutJsonSidecar_UsesExifData()
	{
		// Arrange: only the image file, no JSON sidecar
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { new byte[100] });

		var newItem = await new NewItem(storage, new FakeReadMeta(), new FakeIWebLogger())
			.NewFileItemAsync(new FileIndexItem("/test.jpg"));

		// FakeReadMeta returns "test, fake read meta" as Tags
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, newItem.Status);
	}

	[TestMethod]
	public async Task PrepareUpdateFileItemAsync_WithJsonSidecar_DetectsChanges()
	{
		// DB item has empty tags; JSON sidecar has new tags → change should be detected
		const string jsonTags = "new-tags-from-json";
		var jsonContent = $"{{\"item\":{{\"tags\":\"{jsonTags}\"}}}}";
		var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);

		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/.starsky.test.jpg.json" },
			new List<byte[]> { new byte[100], jsonBytes });

		var dbItem = new FileIndexItem("/test.jpg")
		{
			Tags = string.Empty,
			LastChanged = new List<string>()
		};

		var result = await new NewItem(storage, new FakeReadMeta(), new FakeIWebLogger())
			.PrepareUpdateFileItemAsync(dbItem, 100);

		// The JSON sidecar tags should have been merged into the comparison
		Assert.AreEqual(jsonTags, result.Tags);
	}

	[TestMethod]
	public async Task PrepareUpdateFileItemAsync_WithJsonSidecar_WhenDBMatchesJson_SetsOkAndSame()
	{
		// DB item already has the same tags as the JSON sidecar → OkAndSame
		const string jsonTags = "test, fake read meta"; // matches FakeReadMeta output
		var jsonContent = $"{{\"item\":{{\"tags\":\"{jsonTags}\"}}}}";
		var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);

		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/.starsky.test.jpg.json" },
			new List<byte[]> { new byte[100], jsonBytes });

		var dbItem = new FileIndexItem("/test.jpg")
		{
			Tags = jsonTags,
			LastChanged = new List<string>()
		};

		var result = await new NewItem(storage, new FakeReadMeta(), new FakeIWebLogger())
			.PrepareUpdateFileItemAsync(dbItem, 100);

		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result.Status);
	}

	[TestMethod]
	public async Task ReadAndApplyJsonSidecar_InvalidJson_DoesNotCrash()
	{
		// Invalid JSON in the sidecar should not crash the sync
		var invalidJsonBytes = Encoding.UTF8.GetBytes("{ not valid json }");

		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg", "/.starsky.test.jpg.json" },
			new List<byte[]> { new byte[100], invalidJsonBytes });

		// Should not throw
		var result = await new NewItem(storage, new FakeReadMeta(), new FakeIWebLogger())
			.NewFileItemAsync(new FileIndexItem("/test.jpg"));

		Assert.IsNotNull(result);
	}
}

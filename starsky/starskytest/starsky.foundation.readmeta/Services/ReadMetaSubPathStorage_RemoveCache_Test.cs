using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.readmeta.Services;

[TestClass]
public sealed class ReadMetaSubPathStorageRemoveCacheTest
{
	[TestMethod]
	public void RemoveReadMetaCache_RemovesAllKeys_ForGivenItems()
	{
		var appSettings = new AppSettings();
		var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var subPathStorage = new FakeIStorage();
		var thumbnailStorage = new FakeIStorage();
		var hostFsStorage = new FakeIStorage();
		var tempStorage = new FakeIStorage();

		var selector = new FakeSelectorStorageByType(subPathStorage, thumbnailStorage, hostFsStorage, tempStorage);

		var readMetaSubPath = new ReadMetaSubPathStorage(selector, appSettings, new FakeIWebLogger(), memoryCache);

		const string path1 = "/a.jpg";
		const string path2 = "/b.jpg";
		memoryCache.Set("info_" + path1, new FileIndexItem { FilePath = path1 });
		memoryCache.Set("info_" + path2, new FileIndexItem { FilePath = path2 });

		var items = new List<FileIndexItem>
		{
			new(path1),
			new(path2)
		};

		readMetaSubPath.RemoveReadMetaCache(items);

		Assert.IsFalse(memoryCache.TryGetValue("info_" + path1, out _));
		Assert.IsFalse(memoryCache.TryGetValue("info_" + path2, out _));
	}

	[TestMethod]
	public void RemoveReadMetaCache_DoesNotRemove_WhenAddMemoryCacheDisabled()
	{
		var appSettings = new AppSettings { AddMemoryCache = false };
		var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var subPathStorage = new FakeIStorage();
		var thumbnailStorage = new FakeIStorage();
		var hostFsStorage = new FakeIStorage();
		var tempStorage = new FakeIStorage();

		var selector = new FakeSelectorStorageByType(subPathStorage, thumbnailStorage, hostFsStorage, tempStorage);
		var readMetaSubPath = new ReadMetaSubPathStorage(selector, appSettings, new FakeIWebLogger(), memoryCache);

		const string path = "/c.jpg";
		memoryCache.Set("info_" + path, new FileIndexItem { FilePath = path });

		readMetaSubPath.RemoveReadMetaCache(new List<FileIndexItem> { new(path) });

		Assert.IsTrue(memoryCache.TryGetValue("info_" + path, out _));
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.readmeta.Services;

[TestClass]
public sealed class ReadMetaReadMetaBothCacheTest
{
	[TestMethod]
	public async Task ReadMeta_ReadMetaBothTest_ReadBothWithFilePath()
	{
		var appSettings = new AppSettings { StorageFolder = new CreateAnImage().BasePath };
		var iStorage = new StorageSubPathFilesystem(appSettings, new FakeIWebLogger());

		var listOfFiles = new List<string> { new CreateAnImage().DbPath };
		var fakeCache =
			new FakeMemoryCache(new Dictionary<string, object>());
		var listOfMetas = await new ReadMeta(iStorage, appSettings, fakeCache, new FakeIWebLogger())
			.ReadExifAndXmpFromFileAddFilePathHashAsync(listOfFiles);
		Assert.AreEqual(new CreateAnImage().DbPath.Remove(0, 1),
			listOfMetas.FirstOrDefault()?.FileName);
	}

	[TestMethod]
	public void ReadMeta_ReadMetaBothTest_RemoveCache()
	{
		var appSettings = new AppSettings { StorageFolder = new CreateAnImage().BasePath };
		var iStorage = new StorageSubPathFilesystem(appSettings, new FakeIWebLogger());
		var fakeCache =
			new FakeMemoryCache(new Dictionary<string, object>());
		new ReadMeta(iStorage, appSettings, fakeCache, new FakeIWebLogger())
			.RemoveReadMetaCache("fakeString");
		Assert.IsNotNull(appSettings);
	}

	[TestMethod]
	public async Task ReadMeta_ReadMetaBothTest_FakeReadEntry()
	{
		var iStorage = new FakeIStorage();
		var fakeCache =
			new FakeMemoryCache(new Dictionary<string, object>
			{
				{ "info_test", new FileIndexItem { Tags = "test" } }
			});
		var result = ( await new ReadMeta(iStorage, null!, fakeCache,
				new FakeIWebLogger())
			.ReadExifAndXmpFromFileAsync("test") )?.Tags;
		Assert.AreEqual("test", result);
	}

	[TestMethod]
	public void RemoveReadMetaCache_RemovesAllKeys_ForGivenItems()
	{
		var appSettings = new AppSettings();
		var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var iStorage = new FakeIStorage();
		var readMeta = new ReadMeta(iStorage, appSettings, memoryCache, new FakeIWebLogger());

		const string path1 = "/test1.jpg";
		const string path2 = "/test2.jpg";
		memoryCache.Set("info_" + path1, new FileIndexItem { FilePath = path1 });
		memoryCache.Set("info_" + path2, new FileIndexItem { FilePath = path2 });

		var items = new List<FileIndexItem> { new(path1), new(path2) };

		readMeta.RemoveReadMetaCache(items);

		Assert.IsFalse(memoryCache.TryGetValue("info_" + path1, out _));
		Assert.IsFalse(memoryCache.TryGetValue("info_" + path2, out _));
	}

	[TestMethod]
	public void RemoveReadMetaCache_DoesNotRemove_WhenAddMemoryCacheDisabled()
	{
		var appSettings = new AppSettings { AddMemoryCache = false };
		var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var iStorage = new FakeIStorage();
		var readMeta = new ReadMeta(iStorage, appSettings, memoryCache, new FakeIWebLogger());

		var path = "/test.jpg";
		memoryCache.Set("info_" + path, new FileIndexItem { FilePath = path });

		readMeta.RemoveReadMetaCache(new List<FileIndexItem> { new(path) });

		Assert.IsTrue(memoryCache.TryGetValue("info_" + path, out _));
	}
}

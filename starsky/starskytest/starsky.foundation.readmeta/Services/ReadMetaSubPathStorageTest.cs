using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.readmeta.Services;

[TestClass]
public class ReadMetaSubPathStorageTest
{
	[TestMethod]
	public void ReadExifAndXmpFromFile()
	{
		var fakeStorage = new FakeIStorage();
		var readMetaSubPathStorage = new ReadMetaSubPathStorage(new FakeSelectorStorage(fakeStorage), new AppSettings(), new FakeMemoryCache(), new FakeIWebLogger());
		var result = readMetaSubPathStorage.ReadExifAndXmpFromFile("test.jpg");
		Assert.AreEqual("test.jpg", result.FileName);	
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result.Status);
	}
	
	[TestMethod]
	public void ReadExifAndXmpFromFileAddFilePathHash()
	{
		var fakeStorage = new FakeIStorage();
		var readMetaSubPathStorage = new ReadMetaSubPathStorage(new FakeSelectorStorage(fakeStorage), new AppSettings(), new FakeMemoryCache(), new FakeIWebLogger());
		var result = readMetaSubPathStorage.ReadExifAndXmpFromFileAddFilePathHash(new List<string>(), new List<string>());
		Assert.AreEqual(0, result.Count);
	}
	
	[TestMethod]
	public void UpdateReadMetaCache()
	{
		var fakeStorage = new FakeIStorage();
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetService<IMemoryCache>();
		
		var readMetaSubPathStorage = new ReadMetaSubPathStorage(new FakeSelectorStorage(fakeStorage), new AppSettings(), memoryCache, new FakeIWebLogger());
		var addItem = new FileIndexItem("/test.jpg");
		readMetaSubPathStorage.UpdateReadMetaCache(new List<FileIndexItem>{addItem});
		var actualJson = JsonSerializer.Serialize(memoryCache.Get("info_/test.jpg"),DefaultJsonSerializer.CamelCaseNoEnters);
		var expectedJson = JsonSerializer.Serialize(addItem,DefaultJsonSerializer.CamelCaseNoEnters);

		Assert.AreEqual(expectedJson, actualJson);
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.readmeta.Services
{
	[TestClass]
	public sealed class ReadMetaReadMetaBothCacheTest
	{
		[TestMethod]
		public async Task ReadMeta_ReadMetaBothTest_ReadBothWithFilePath()
		{
			var appSettings = new AppSettings {StorageFolder = new CreateAnImage().BasePath};
			var iStorage = new StorageSubPathFilesystem(appSettings, new FakeIWebLogger());

			var listOfFiles = new List<string>{ new CreateAnImage().DbPath};
			var fakeCache =
				new FakeMemoryCache(new Dictionary<string, object>());
			var listOfMetas = await new ReadMeta(iStorage,appSettings,fakeCache, new FakeIWebLogger())
				.ReadExifAndXmpFromFileAddFilePathHashAsync(listOfFiles);
			Assert.AreEqual(new CreateAnImage().DbPath.Remove(0,1), 
				listOfMetas.FirstOrDefault()?.FileName);
		}

		[TestMethod]
		public void ReadMeta_ReadMetaBothTest_RemoveCache()
		{
			var appSettings = new AppSettings {StorageFolder = new CreateAnImage().BasePath};
			var iStorage = new StorageSubPathFilesystem(appSettings, new FakeIWebLogger());
			var fakeCache =
				new FakeMemoryCache(new Dictionary<string, object>());
			new ReadMeta(iStorage,appSettings, fakeCache, new FakeIWebLogger())
				.RemoveReadMetaCache("fakeString");
			Assert.IsNotNull(appSettings);
		}

		[TestMethod]
		public async Task ReadMeta_ReadMetaBothTest_FakeReadEntry()
		{
			var iStorage = new FakeIStorage();
			var fakeCache =
				new FakeMemoryCache(new Dictionary<string, object>{{"info_test",
					new FileIndexItem(){Tags = "test"}}});
			var result = ( await new ReadMeta(iStorage, null!, fakeCache,
					new FakeIWebLogger())
				.ReadExifAndXmpFromFileAsync("test") )?.Tags;
			Assert.AreEqual("test",result);
		}
	}
}

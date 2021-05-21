using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Services
{
	[TestClass]
	public class ReadMeta_ReadMetaBoth_Cache_Test
	{

        
		[TestMethod]
		public void ReadMeta_ReadMetaBothTest_ReadBothWithFilePath()
		{

			var appsettings = new AppSettings {StorageFolder = new CreateAnImage().BasePath};
			var iStorage = new StorageSubPathFilesystem(appsettings);

			var listofFiles = new List<string>{ new CreateAnImage().DbPath};
			var fakeCache =
				new FakeMemoryCache(new Dictionary<string, object>());
			var listOfMetas = new ReadMeta(iStorage,appsettings,fakeCache)
				.ReadExifAndXmpFromFileAddFilePathHash(listofFiles);
			Assert.AreEqual(new CreateAnImage().DbPath.Remove(0,1), 
				listOfMetas.FirstOrDefault().FileName);
		}

		[TestMethod]
		public void ReadMeta_ReadMetaBothTest_RemoveCache()
		{
			var appsettings = new AppSettings {StorageFolder = new CreateAnImage().BasePath};
			var iStorage = new StorageSubPathFilesystem(appsettings);
			var fakeCache =
				new FakeMemoryCache(new Dictionary<string, object>());
			new ReadMeta(iStorage,appsettings, fakeCache)
				.RemoveReadMetaCache("fakeString");
		}

//        [TestMethod]
//        public void ReadMeta_ReadMetaBothTest_FakeCreateEntry()
//        {
//	        var iStorage = new FakeIStorage(null,new List<string>{"/test.jpg"},CreateAnImage.Bytes);
//
//            // fakely add item to cache
//            new ReadMeta(iStorage,new AppSettings(), _fakeCache)
//	            .ReadExifAndXmpFromFile("/test.jpg",ExtensionRolesHelper.ImageFormat.jpg);
//        }

		[TestMethod]
		public void ReadMeta_ReadMetaBothTest_FakeReadEntry()
		{
			var iStorage = new FakeIStorage();
			var fakeCache =
				new FakeMemoryCache(new Dictionary<string, object>{{"info_test",new FileIndexItem(){Tags = "test"}}});
			Assert.AreEqual("test",new ReadMeta(iStorage,null, fakeCache).ReadExifAndXmpFromFile("test").Tags);
		}
	}
}

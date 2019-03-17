using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private ServiceProvider _serviceProvider;
        private IMemoryCache _fakeCache;

        public ReadMeta_ReadMetaBoth_Cache_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMemoryCache, FakeMemoryCache>();
            _serviceProvider = services.BuildServiceProvider();
            _fakeCache = _serviceProvider.GetRequiredService<IMemoryCache>();
        }
        [TestMethod]
        public void ReadMeta_ReadMetaBothTest_ReadBothWithFilePath()
        {

            var appsettings = new AppSettings {StorageFolder = new CreateAnImage().BasePath};
	        var iStorage = new StorageSubPathFilesystem(appsettings);

            var listofFiles = new string[]{ new CreateAnImage().FullFilePath};
            var listOfMetas = new ReadMeta(iStorage,appsettings,_fakeCache)
                .ReadExifAndXmpFromFileAddFilePathHash(listofFiles);
            Assert.AreEqual(new CreateAnImage().DbPath.Remove(0,1), 
                listOfMetas.FirstOrDefault().FileName);
        }

        [TestMethod]
        public void ReadMeta_ReadMetaBothTest_RemoveCache()
        {
            var appsettings = new AppSettings {StorageFolder = new CreateAnImage().BasePath};
	        var iStorage = new StorageSubPathFilesystem(appsettings);

            new ReadMeta(iStorage,appsettings, _fakeCache)
                    .RemoveReadMetaCache("fakeString");
        }

        [TestMethod]
        public void ReadMeta_ReadMetaBothTest_FakeCreateEntry()
        {
            var createAnImage = new CreateAnImage();
            var appsettings = new AppSettings {StorageFolder = createAnImage.BasePath};
	        var iStorage = new StorageSubPathFilesystem(appsettings);

            // fakely add item to cache
            new ReadMeta(iStorage,appsettings, _fakeCache).ReadExifAndXmpFromFile(createAnImage.FullFilePath,ExtensionRolesHelper.ImageFormat.jpg);
        }

        [TestMethod]
        public void ReadMeta_ReadMetaBothTest_FakeReadEntry()
        {
	        var iStorage = new FakeIStorage();
            Assert.AreEqual("test",new ReadMeta(iStorage,null, _fakeCache).ReadExifAndXmpFromFile("test",ExtensionRolesHelper.ImageFormat.jpg).Tags);
        }
    }
}

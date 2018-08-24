using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starsky.Services;
using starskytests.FakeMocks;

namespace starskytests.Services
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
            var listofFiles = new string[]{ new CreateAnImage().FullFilePath};
            var listOfMetas = new ReadMeta(appsettings,_fakeCache)
                .ReadExifAndXmpFromFileAddFilePath(listofFiles);
            Assert.AreEqual(new CreateAnImage().DbPath.Remove(0,1), 
                listOfMetas.FirstOrDefault().FileName);
        }

        [TestMethod]
        public void ReadMeta_ReadMetaBothTest_RemoveCache()
        {
            var appsettings = new AppSettings {StorageFolder = new CreateAnImage().BasePath};
            var fakeStringList = new List<string> {"fakeString"};
            new ReadMeta(appsettings, _fakeCache)
                    .RemoveReadMetaCache(fakeStringList);
        }

        [TestMethod]
        public void ReadMeta_ReadMetaBothTest_FakeCreateEntry()
        {
            var createAnImage = new CreateAnImage();
            var appsettings = new AppSettings {StorageFolder = createAnImage.BasePath};

            new ReadMeta(appsettings, _fakeCache).ReadExifAndXmpFromFile(createAnImage.FullFilePath);
            

        }
    }
}
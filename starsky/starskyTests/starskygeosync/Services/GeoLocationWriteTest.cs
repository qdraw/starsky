using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Interfaces;
using starsky.Models;
using starskyGeoCli.Services;
using starskytests.Models;

namespace starskytests.starskygeosync.Services
{
    [TestClass]
    public class GeoLocationWriteTest
    {
        private readonly IExiftool _exiftool;
        private AppSettings _appSettings;

        public GeoLocationWriteTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, FakeExiftool>();    
            
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            
            _exiftool = serviceProvider.GetRequiredService<IExiftool>();
            
            // get the service
            _appSettings = new AppSettings();
            
        }

        [TestMethod]
        public void GeoLocationWriteLoopFolderTest()
        {
            var metaFilesInDirectory = new List<FileIndexItem>();
            new GeoLocationWrite(_appSettings, _exiftool).LoopFolder(metaFilesInDirectory, true);
        }
    }
}
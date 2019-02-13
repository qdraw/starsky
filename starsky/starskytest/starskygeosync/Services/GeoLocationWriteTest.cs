using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Interfaces;
using starskycore.Models;
using starskyGeoCli.Services;
using starskytests.Models;

namespace starskytests.starskygeosync.Services
{
    [TestClass]
    public class GeoLocationWriteTest
    {
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;

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
            var metaFilesInDirectory = new List<FileIndexItem>
            {
	            new FileIndexItem
	            {
		            FileName = "test.jpg", //<= used to check
		            ParentDirectory = "/",
		            Latitude = 1,
		            Longitude = 1,
		            LocationAltitude = 1,
		            LocationCity = "city",
		            LocationState = "state",
		            LocationCountry = "country"
	            }
            };
            new GeoLocationWrite(_appSettings, _exiftool).LoopFolder(metaFilesInDirectory, true);
        }
    }
}
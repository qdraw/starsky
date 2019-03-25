using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Interfaces;
using starskycore.Models;
using starskyGeoCli.Services;
using starskytest.Models;

namespace starskytest.starskygeosync.Services
{
    [TestClass]
    public class GeoLocationWriteTest
    {
        private readonly IExifTool _exifTool;
        private readonly AppSettings _appSettings;

        public GeoLocationWriteTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IExifTool, FakeExifTool>();    
            
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            
            _exifTool = serviceProvider.GetRequiredService<IExifTool>();
            
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
            new GeoLocationWrite(_appSettings, _exifTool).LoopFolder(metaFilesInDirectory, true);
        }
    }
}
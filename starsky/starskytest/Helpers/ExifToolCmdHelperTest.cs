using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Helpers
{
    [TestClass]
    public class ExifToolCmdHelperTest
    {
        private readonly IExifTool _exifTool;
        private AppSettings _appSettings;

        public ExifToolCmdHelperTest()
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
        public void ExifToolCmdHelper_UpdateTest()
        {
            var updateModel = new FileIndexItem
            {
                Tags = "tags",
                Description = "Description",
                Latitude = 52,
                Longitude = 3,
                LocationAltitude = 41,
                LocationCity = "LocationCity",
                LocationState = "LocationState",
                LocationCountry = "LocationCountry",
                Title = "Title",
                ColorClass = FileIndexItem.Color.Trash,
                Orientation = FileIndexItem.Rotation.Rotate90Cw,
                DateTime = DateTime.Now,
            };
            var comparedNames = new List<string>{
                nameof(FileIndexItem.Tags),
                nameof(FileIndexItem.Description),
                nameof(FileIndexItem.Latitude),
                nameof(FileIndexItem.Longitude),
                nameof(FileIndexItem.LocationAltitude),
                nameof(FileIndexItem.LocationCity),
                nameof(FileIndexItem.LocationState),
                nameof(FileIndexItem.LocationCountry),
                nameof(FileIndexItem.Title),
                nameof(FileIndexItem.ColorClass),
                nameof(FileIndexItem.Orientation),
                nameof(FileIndexItem.DateTime),
            };
            
            var inputSubPaths = new List<string>
            {
                "/test.jpg"
            };
	        var storage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg"},new List<byte[]>(),new List<string>{null});

            var helperResult = new ExifToolCmdHelper(_exifTool, storage ,new FakeReadMeta()).Update(updateModel, inputSubPaths, comparedNames);
            
            Assert.AreEqual(true,helperResult.Contains(updateModel.Tags));
            Assert.AreEqual(true,helperResult.Contains(updateModel.Description));
            Assert.AreEqual(true,helperResult.Contains(updateModel.Latitude.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true,helperResult.Contains(updateModel.Longitude.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true,helperResult.Contains(updateModel.LocationAltitude.ToString(CultureInfo.InvariantCulture)));
            Assert.AreEqual(true,helperResult.Contains(updateModel.LocationCity));
            Assert.AreEqual(true,helperResult.Contains(updateModel.LocationState));
            Assert.AreEqual(true,helperResult.Contains(updateModel.LocationCountry));
            Assert.AreEqual(true,helperResult.Contains(updateModel.Title));
        }

        [TestMethod]
        public void ExifToolCmdHelper_Update_UpdateLocationAltitudeCommandTest()
        {
            var updateModel = new FileIndexItem
            {
                LocationAltitude = -41,
            };
            var comparedNames = new List<string>{
                nameof(FileIndexItem.LocationAltitude),
            };
            
	        var folderPaths = new List<string>{"/"};

            var inputSubPaths = new List<string>{"/test.jpg"};

            var helperResult = new ExifToolCmdHelper(_exifTool, new FakeIStorage(folderPaths,inputSubPaths,null,new List<string>{"?"}),new FakeReadMeta()).Update(updateModel, inputSubPaths, comparedNames);
            
            Assert.AreEqual(true,helperResult.Contains("-GPSAltitude=\"-41"));
            Assert.AreEqual(true,helperResult.Contains("gpsaltituderef#=\"1"));

        }

//        [TestMethod]
//        public void ExifToolCmdHelper_Quoted()
//        {
//            var helperResult = new ExifToolCmdHelper(_appSettings, _exifTool).Quoted(null, "test");
//            Assert.AreEqual("\"test\"",helperResult.ToString());
//        }

	    
        [TestMethod]
        public void ExifToolCmdHelper_CopyExifPublish()
        {
	        var folderPaths = new List<string>{"/"};
	        var inputSubPaths = new List<string>{"/test.jpg"};

	        var storage =
		        new FakeIStorage(folderPaths, inputSubPaths, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}, new List<string> {"?"});
	        
	        var fakeReadMeta = new ReadMeta(storage);
            var helperResult = new ExifToolCmdHelper(_exifTool, storage, fakeReadMeta).CopyExifPublish("/test.jpg", "/test2");
            Assert.AreEqual(true,helperResult.Contains("HistorySoftwareAgent"));
        }
    }
}
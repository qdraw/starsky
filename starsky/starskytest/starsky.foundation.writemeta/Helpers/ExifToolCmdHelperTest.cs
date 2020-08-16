﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;
using starskytest.Models;
using ExifToolCmdHelper = starsky.foundation.writemeta.Helpers.ExifToolCmdHelper;

namespace starskytest.starsky.foundation.writemeta.Helpers
{
    [TestClass]
    public class ExifToolCmdHelperTest
    {
        private AppSettings _appSettings;

        public ExifToolCmdHelperTest()
        {
       
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
                ColorClass = ColorClassParser.Color.Trash,
                Orientation = FileIndexItem.Rotation.Rotate90Cw,
                DateTime = DateTime.Now,
            };
            var comparedNames = new List<string>{
                nameof(FileIndexItem.Tags).ToLowerInvariant(),
                nameof(FileIndexItem.Description).ToLowerInvariant(),
                nameof(FileIndexItem.Latitude).ToLowerInvariant(),
                nameof(FileIndexItem.Longitude).ToLowerInvariant(),
                nameof(FileIndexItem.LocationAltitude).ToLowerInvariant(),
                nameof(FileIndexItem.LocationCity).ToLowerInvariant(),
                nameof(FileIndexItem.LocationState).ToLowerInvariant(),
                nameof(FileIndexItem.LocationCountry).ToLowerInvariant(),
                nameof(FileIndexItem.Title).ToLowerInvariant(),
                nameof(FileIndexItem.ColorClass).ToLowerInvariant(),
                nameof(FileIndexItem.Orientation).ToLowerInvariant(),
                nameof(FileIndexItem.DateTime).ToLowerInvariant(),
            };
            
            var inputSubPaths = new List<string>
            {
                "/test.jpg"
            };
	        var storage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg"},new List<byte[]>());

	        var fakeExifTool = new FakeExifTool(storage,_appSettings);
            var helperResult = new ExifToolCmdHelper(fakeExifTool, storage,storage ,
	            new FakeReadMeta()).Update(updateModel, inputSubPaths, comparedNames);
            
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
                nameof(FileIndexItem.LocationAltitude).ToLowerInvariant(),
            };
            
	        var folderPaths = new List<string>{"/"};

            var inputSubPaths = new List<string>{"/test.jpg"};

	        var storage =
		        new FakeIStorage(folderPaths, inputSubPaths, null);
	        var fakeExifTool = new FakeExifTool(storage,_appSettings);

            var helperResult = new ExifToolCmdHelper(fakeExifTool, storage,storage,new FakeReadMeta()).Update(updateModel, inputSubPaths, comparedNames);
            
            Assert.AreEqual(true,helperResult.Contains("-GPSAltitude=\"-41"));
            Assert.AreEqual(true,helperResult.Contains("gpsaltituderef#=\"1"));

        }

//        [TestMethod]
//        public void ExifToolCmdHelper_Quoted()
//        {
//            var helperResult = new ExifToolCmdHelper(_appSettings, _exifTool).Quoted(null, "test");
//            Assert.AreEqual("\"test\"",helperResult.ToString());
//        }

	    

    }
}

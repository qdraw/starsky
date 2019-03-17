﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskytest.Models;

namespace starskytest.Helpers
{
    [TestClass]
    public class ExifToolCmdHelperTest
    {
        private readonly IExiftool _exiftool;
        private AppSettings _appSettings;

        public ExifToolCmdHelperTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, FakeExifTool>();    
            
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            
            _exiftool = serviceProvider.GetRequiredService<IExiftool>();
            
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
            
            var inputFullFilePaths = new List<string>
            {
                "test.jpg"
            };

            var helperResult = new ExifToolCmdHelper(_appSettings, _exiftool).Update(updateModel, inputFullFilePaths, comparedNames);
            
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
            
            var inputFullFilePaths = new List<string>();

            var helperResult = new ExifToolCmdHelper(_appSettings, _exiftool).Update(updateModel, inputFullFilePaths, comparedNames);
            
            Assert.AreEqual(true,helperResult.Contains("-GPSAltitude=\"-41"));
            Assert.AreEqual(true,helperResult.Contains("gpsaltituderef#=\"1"));

        }

        [TestMethod]
        public void ExifToolCmdHelper_Quoted()
        {
            var helperResult = new ExifToolCmdHelper(_appSettings, _exiftool).Quoted(null, "test");
            Assert.AreEqual("\"test\"",helperResult.ToString());
        }

        [TestMethod]
        public void ExifToolCmdHelper_CopyExifPublish()
        {
            var helperResult = new ExifToolCmdHelper(_appSettings, _exiftool).CopyExifPublish("test", "test");
            Assert.AreEqual(true,helperResult.Contains("HistorySoftwareAgent"));
        }
    }
}
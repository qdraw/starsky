using System;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starskycore.Helpers;
using starskycore.Services;
using starskytests.FakeCreateAn;


namespace starskytests.Services
{
    [TestClass]
    public class ReadGpxFromFileTest
    {
        
        [TestMethod]
        public void ReadGpxFromFileTest_ReturnAfterFirstFieldreadfile()
        {
            var gpxFullSourcePath = new CreateAnGpx().FullFileGpxPath;
            var returnItem = new ReadMeta(null, null).ReadGpxFromFileReturnAfterFirstField(gpxFullSourcePath);
            Assert.AreEqual(5.485941,returnItem.Longitude,0.001);
            Assert.AreEqual(51.809360,returnItem.Latitude,0.001);
            Assert.AreEqual("_20180905-fietsen-oss",returnItem.Title);
            Assert.AreEqual(7.263,returnItem.LocationAltitude,0.001);

            DateTime.TryParseExact("2018-09-05T17:31:53Z", 
                "yyyy-MM-ddTHH:mm:ssZ", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AdjustToUniversal, 
                out var expectDateTime);
            // gpx is always utc
            Assert.AreEqual(expectDateTime,returnItem.DateTime);
            
            // remove afterwards
            Files.DeleteFile(gpxFullSourcePath);
        }
        
        [TestMethod]
        public void ReadGpxFromFileTest_readfile()
        {
            var gpxFullSourcePath = new CreateAnGpx().FullFileGpxPath;
            var returnItem = new ReadMeta(null, null).ReadGpxFile(gpxFullSourcePath);
            Assert.AreEqual(5.485941,returnItem.FirstOrDefault().Longitude,0.001);
            Assert.AreEqual(51.809360,returnItem.FirstOrDefault().Latitude,0.001);
            DateTime.TryParseExact("2018-09-05T17:31:53Z", 
                "yyyy-MM-ddTHH:mm:ssZ", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AdjustToUniversal, 
                out var expectDateTime);
            // gpx is always utc
            Assert.AreEqual(expectDateTime,returnItem.FirstOrDefault().DateTime);
            Assert.AreEqual("_20180905-fietsen-oss",returnItem.FirstOrDefault().Title);
            Assert.AreEqual(7.263,returnItem.FirstOrDefault().Altitude,0.001);

            // remove afterwards
            Files.DeleteFile(gpxFullSourcePath);
        }

    }
}

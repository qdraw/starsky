using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.Services;
using starskycore.Helpers;
using starskycore.Services;
using starskytest.FakeCreateAn;

namespace starskytest.Services
{
    [TestClass]
    public class ReadGpxFromFileTest
    {
        
        [TestMethod]
        public void ReadGpxFromFileTest_ReturnAfterFirstFieldReadFile()
        {
            var gpxBytes = CreateAnGpx.Bytes;
	        MemoryStream stream = new MemoryStream(gpxBytes);
	        
            var returnItem = new ReadMetaGpx().ReadGpxFromFileReturnAfterFirstField(stream,"/test.gpx");
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
        }

        [TestMethod]
        public void ReadGpxFromFileTest_NonValidInput()
        {
	        var gpxBytes = new byte[0];
	        MemoryStream stream = new MemoryStream(gpxBytes);
	        
	        var returnItem = new ReadMetaGpx().ReadGpxFromFileReturnAfterFirstField(stream,"/test.gpx");
	        Assert.AreEqual(new DateTime(),returnItem.DateTime );
        }
        
        [TestMethod]
        public void ReadGpxFromFileTest_TestFileName()
        {
	        var gpxBytes = CreateAnGpx.Bytes;
	        MemoryStream stream = new MemoryStream(gpxBytes);

	        var returnItem =
		        new ReadMetaGpx().ReadGpxFromFileReturnAfterFirstField(stream, "/test.gpx");
	        Assert.AreEqual("test.gpx",returnItem.FileName);
	        Assert.AreEqual("/",returnItem.ParentDirectory);
        }

        [TestMethod]
        public void ReadGpxFromFileTest_ReadFile()
        {
	        var gpxBytes = CreateAnGpx.Bytes;
	        MemoryStream stream = new MemoryStream(gpxBytes);
	        var returnItem = new ReadMetaGpx().ReadGpxFile(stream);
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
        }

    }
}

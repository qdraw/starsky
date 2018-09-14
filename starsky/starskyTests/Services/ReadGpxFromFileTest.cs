using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starsky.Services;

namespace starskytests.Services
{
    [TestClass]
    public class ReadGpxFromFileTest
    {
        
        [TestMethod]
        public void ReadGpxFromFileTest_readfile()
        {
            var gpxFullSourcePath = new CreateAnGpx().FullFileGpxPath;
            var returnItem = new ReadMeta(null, null).ReadGpxFromFile(gpxFullSourcePath);
            Assert.AreEqual(5.485941,returnItem.Longitude,0.001);
            Assert.AreEqual(51.809360,returnItem.Latitude,0.001);
            DateTime.TryParse("2018-09-05T17:31:53Z", out var expectDateTime);
            Assert.AreEqual(expectDateTime,returnItem.DateTime);
            
            // remove afterwards
            Files.DeleteFile(gpxFullSourcePath);
        }

    }
}
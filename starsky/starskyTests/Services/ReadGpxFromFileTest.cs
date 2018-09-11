using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Services;

namespace starskytests.Services
{
    [TestClass]
    public class ReadGpxFromFileTest
    {
        
        [TestMethod]
        public void ReadGpxFromFileTest_readfile()
        {
            var retrunItem = new ReadMeta(null, null).ReadGpxFromFile(new CreateAnImage().FullFileGpxPath);
            Assert.AreEqual(5.485941,retrunItem.Longitude,0.001);
            Assert.AreEqual(51.809360,retrunItem.Latitude,0.001);
            DateTime.TryParse("2018-09-05T17:31:53Z", out var expectDateTime);
            Assert.AreEqual(expectDateTime,retrunItem.DateTime);
        }

    }
}
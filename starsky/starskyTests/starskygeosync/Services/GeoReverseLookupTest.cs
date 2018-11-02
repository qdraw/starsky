using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starskyGeoCli.Services;

namespace starskytests.starskygeosync.Services
{
    [TestClass]
    public class GeoReverseLookupTest
    {
        private AppSettings _appSettings;

        public GeoReverseLookupTest()
        {
            _appSettings = new AppSettings
            {
                TempFolder = new CreateAnImage().BasePath
            };
        }

        [TestMethod]
        public void GeoReverseLookup_LoopFolderLookupTest()
        {
            var cakeBakerPhoto = new FileIndexItem
            {
                Latitude = 51.6897055,
                Longitude = 5.2974817,
                FileName = "t.jpg"
            };
            
            var northSea = new FileIndexItem
            {
                Latitude = 56.3618575,
                Longitude = 3.1753435,
                FileName = "t.jpg"
            };
            
            var buenosAires = new FileIndexItem
            {
                Latitude = -34.6156625,
                Longitude = -58.5033383,
                FileName = "t.jpg" // checks if file type is suppored to write
            };
            var folderOfPhotos = new List<FileIndexItem> {cakeBakerPhoto, northSea, buenosAires};

	        Console.WriteLine(NGeoNames.GeoFileDownloader.DEFAULTGEOFILEBASEURI);
		        
            new GeoReverseLookup(_appSettings).LoopFolderLookup(folderOfPhotos,false);

            Assert.AreEqual("Argentina", buenosAires.LocationCountry);
            Assert.AreEqual(string.Empty, northSea.LocationCountry);
            Assert.AreEqual("'s-Hertogenbosch", cakeBakerPhoto.LocationCity);
            Assert.AreEqual("North Brabant", cakeBakerPhoto.LocationState);
            Assert.AreEqual("Nederland", cakeBakerPhoto.LocationCountry);

            
        }

	    [TestMethod]
	    public void GeoReverseLookup_RemoveNoUpdateItemsTest()
	    {
		    var list = new List<FileIndexItem>
		    {
			    new FileIndexItem(),
			    new FileIndexItem{ Latitude = 50, Longitude = 50}
		    };
		    var result = new GeoReverseLookup(_appSettings).RemoveNoUpdateItems(list,true);
		    Assert.AreEqual(1, result.Count);
	    }

    }
}
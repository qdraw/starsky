using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starskygeosync.Services;

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
        public void LoopFolderLookupTest()
        {
            var cakeBakerPhoto = new FileIndexItem
            {
                Latitude = 51.6897055,
                Longitude = 5.2974817
            };
            
            var northSea = new FileIndexItem
            {
                Latitude = 56.3618575,
                Longitude = 3.1753435
            };
            
            var buenosAires = new FileIndexItem
            {
                Latitude = -34.6156625,
                Longitude = -58.5033383
            };
            var folderOfPhotos = new List<FileIndexItem> {cakeBakerPhoto, northSea, buenosAires};
            
            new GeoReverseLookup(_appSettings).LoopFolderLookup(folderOfPhotos);

            Assert.AreEqual("Argentina", buenosAires.LocationCountry);
            Assert.AreEqual(string.Empty, northSea.LocationCountry);
            Assert.AreEqual("'s-Hertogenbosch", cakeBakerPhoto.LocationCity);
            Assert.AreEqual("North Brabant", cakeBakerPhoto.LocationState);

            
        }
        
        
        
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;
using starskyGeoCli.Services;
using starskytest.FakeCreateAn;

namespace starskytest.starskygeosync.Services
{
    [TestClass]
    public class GeoReverseLookupTest
    {
        private AppSettings _appSettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeoReverseLookupTest"/> class.
		/// </summary>
		public GeoReverseLookupTest()
        {
            _appSettings = new AppSettings
            {
                TempFolder = new CreateAnImage().BasePath
            };

			// We mock the data to avoid http request during tests

	        // Mockup data to: 
			// map the city
			var mockCities1000 = "2747351\t\'s-Hertogenbosch\t\'s-Hertogenbosch\t\'s Bosch,\'s-Hertogenbosch,Bois-le-Duc,Bolduque,Boscoducale,De Bosk,Den Bosch,Hertogenbosch,Herzogenbusch,Khertogenbos,Oeteldonk,Silva Ducis,Хертогенбос,’s-Hertogenbosch\t51.69917\t5.30417\tP\tPPLA\tNL\t\t06\t0796\t\t\t134520\t\t7\tEurope/Amsterdam\t2017-10-17\r\n" +
	                             "6693230\tVilla Santa Rita\tVilla Santa Rita\t\t-34.61082\t-58.481\tP\tPPLX\tAR\t\t07\t02011\t\t\t34000\t\t25\tAmerica/Argentina/Buenos_Aires\t2017-05-08\r\n" +
	                             "3713678\tBuenos Aires\tBuenos Aires\tBuenos Aires\t8.63146\t-79.94775\tP\tPPLA3\tPA\t\t13\t\t\t\t496\t\t232\tAmerica/Panama\t2017-08-16\r\n" +
	                             "3713682\tBuenos Aires\tBuenos Aires\tBuenos Aires\t8.41384\t-81.4844\tP\tPPLA2\tPA\t\t12\t\t\t\t400\t\t336\tAmerica/Panama\t2017-08-16\r\n";

	        new PlainTextFileHelper().WriteFile(Path.Combine(_appSettings.TempFolder, "cities1000.txt"),mockCities1000);

			// Mockup data to:
			// map the state and country

	        var admin1CodesASCII = "NL.07\tNorth Holland\tNorth Holland\t2749879\r\n" +
	                               "NL.06\tNorth Brabant\tNorth Brabant\t2749990\r\n" +
	                               "NL.05\tLimburg\tLimburg\t2751596\r\n" +
	                               "NL.03\tGelderland\tGelderland\t2755634\r\n" +
	                               "AR.07\tBuenos Aires F.D.\tBuenos Aires F.D.\t3433955\r\n";

			new PlainTextFileHelper().WriteFile(Path.Combine(_appSettings.TempFolder, "admin1CodesASCII.txt"), admin1CodesASCII);


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

	    [TestMethod]
	    public void GeoReverseLookup_RemoveNoUpdateItemsTest2()
	    {
		    var list = new List<FileIndexItem>
		    {
			    new FileIndexItem{LocationCity = "Apeldoorn"},
			    new FileIndexItem(),
		    };
		    
		    var result = new GeoReverseLookup(_appSettings).RemoveNoUpdateItems(list,true);
		    
	    }
	    
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services
{
	[TestClass]
	public sealed class GeoReverseLookupTest
	{
		private AppSettings _appSettings = new AppSettings();

		/// <summary>
		/// Initializes a new instance of the <see cref="GeoReverseLookupTest"/> class.
		/// </summary>
		public GeoReverseLookupTest()
		{
			Setup();
		}
		
		[ClassCleanup]
		public static void ClassCleanUp()
		{
			var path = Path.Combine(new CreateAnImage().BasePath, "tmp-dependencies") ;
			if ( new StorageHostFullPathFilesystem().ExistFolder(path))
			{
				new StorageHostFullPathFilesystem().FolderDelete(path);
			}
		}

		private void Setup()
		{
			_appSettings = new AppSettings
			{
				DependenciesFolder = Path.Combine(new CreateAnImage().BasePath, "tmp-dependencies"),
			};

						
			// create a temp folder
			if ( !new StorageHostFullPathFilesystem().ExistFolder(_appSettings.DependenciesFolder) )
			{
				new StorageHostFullPathFilesystem().CreateDirectory(_appSettings
					.DependenciesFolder);
			}
			
			// We mock the data to avoid http request during tests

			// Mockup data to: 
			// map the city
			const string mockCities1000 = "2747351\t\'s-Hertogenbosch\t\'s-Hertogenbosch\t\'s Bosch,\'s-Hertogenbosch,Bois-le-Duc,Bolduque,Boscoducale,De Bosk,Den Bosch,Hertogenbosch,Herzogenbusch,Khertogenbos,Oeteldonk,Silva Ducis,Хертогенбос,’s-Hertogenbosch\t51.69917\t5.30417\tP\tPPLA\tNL\t\t06\t0796\t\t\t134520\t\t7\tEurope/Amsterdam\t2017-10-17\r\n" +
			                              "6693230\tVilla Santa Rita\tVilla Santa Rita\t\t-34.61082\t-58.481\tP\tPPLX\tAR\t\t07\t02011\t\t\t34000\t\t25\tAmerica/Argentina/Buenos_Aires\t2017-05-08\r\n" +
			                              "3713678\tBuenos Aires\tBuenos Aires\tBuenos Aires\t8.63146\t-79.94775\tP\tPPLA3\tPA\t\t13\t\t\t\t496\t\t232\tAmerica/Panama\t2017-08-16\r\n" +
			                              "3713682\tBuenos Aires\tBuenos Aires\tBuenos Aires\t8.41384\t-81.4844\tP\tPPLA2\tPA\t\t12\t\t\t\t400\t\t336\tAmerica/Panama\t2017-08-16\r\n" +
			                              "6691831\tVatican City\tVatican City\tCitta del Vaticano,Città del Vaticano,Ciudad del Vaticano,Etat de la Cite du Vatican,Staat Vatikanstadt,Staat der Vatikanstadt,Vatican,Vatican City,Vatican City State,Vaticano,Vatikan,Vatikanas,Vatikanstaden,Vatikanstadt,batikan,batikan si,État de la Cité du Vatican,Ватикан,바티칸,바티칸 시\t41.90268\t12.45414\tP\tPPLC\tVA\tIT\t\t\t\t\t829\t55\t61\tEurope/Vatican\t2018-08-17\n";


			new StorageHostFullPathFilesystem().WriteStream(
				StringToStreamHelper.StringToStream(mockCities1000),
				Path.Combine(_appSettings.DependenciesFolder, "cities1000.txt"));
			
			// Mockup data to:
			// map the state and country

			const string admin1CodesAscii = "NL.07\tNorth Holland\tNorth Holland\t2749879\r\n" +
			                                "NL.06\tNorth Brabant\tNorth Brabant\t2749990\r\n" +
			                                "NL.05\tLimburg\tLimburg\t2751596\r\n" +
			                                "NL.03\tGelderland\tGelderland\t2755634\r\n" +
			                                "AR.07\tBuenos Aires F.D.\tBuenos Aires F.D.\t3433955\r\n";

			new StorageHostFullPathFilesystem().WriteStream(
				StringToStreamHelper.StringToStream(admin1CodesAscii),
				Path.Combine(_appSettings.DependenciesFolder, "admin1CodesASCII.txt"));
		}

		[TestMethod]
		public async Task GeoReverseLookup_LoopFolderLookupTest()
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
		        
			await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).LoopFolderLookup(folderOfPhotos,false);

			Assert.AreEqual("Argentina", buenosAires.LocationCountry);
			Assert.AreEqual("ARG", buenosAires.LocationCountryCode);
			Assert.AreEqual(string.Empty, northSea.LocationCountry);
			Assert.AreEqual("'s-Hertogenbosch", cakeBakerPhoto.LocationCity);
			Assert.AreEqual("North Brabant", cakeBakerPhoto.LocationState);
			Assert.AreEqual("Nederland", cakeBakerPhoto.LocationCountry);
			Assert.AreEqual("NLD", cakeBakerPhoto.LocationCountryCode);

		}

		[TestMethod]
		public  async Task GeoReverseLookup_NullParentFOlder()
		{
			var item = new FileIndexItem();
			var propertyObject = item.GetType().GetProperty(nameof(item.ParentDirectory));
			propertyObject!.SetValue(item, null, null);

			var result = await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).LoopFolderLookup(new List<FileIndexItem>{item},false);
			
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public async Task GeoReverseLookup_CatchError_VaticanCity()
		{
			// the Country code VA does not exist
			var vaticanCity = new FileIndexItem
			{
				Latitude = 41.9018611111,
				Longitude = 12.4581638888,
				FileName = "t.jpg" // checks if file type is suppored to write
			};
			var folderOfPhotos = new List<FileIndexItem> {vaticanCity};

			await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).LoopFolderLookup(folderOfPhotos,false);
			
			Assert.AreEqual("Vatican City", vaticanCity.LocationCity);
		}

		[TestMethod]
		public void GeoReverseLookup_RemoveNoUpdateItemsTest()
		{
			var list = new List<FileIndexItem>
			{
				new FileIndexItem(),
				new FileIndexItem{ Latitude = 50, Longitude = 50}
			};
			var result = new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(), new FakeIWebLogger())
				.RemoveNoUpdateItems(list,true);
			Assert.AreEqual(1, result.Count);
		}

		[TestMethod]
		public void GeoReverseLookup_RemoveNoUpdateItemsTest_IgnoreCity()
		{
			var list = new List<FileIndexItem>
			{
				new FileIndexItem{LocationCity = "Apeldoorn"},
				new FileIndexItem(),
			};
		    
			// ignore city
			var result = new GeoReverseLookup(_appSettings, 
				new FakeIGeoFileDownload(),new FakeIWebLogger()).RemoveNoUpdateItems(list,false);
			Assert.AreEqual(0, result.Count);
		}

		[TestMethod]
		public void GetAdmin1Name_Null()
		{
			var geoReverseLookup = new GeoReverseLookup(_appSettings,
				new FakeIGeoFileDownload(), new FakeIWebLogger());

			var result = geoReverseLookup.GetAdmin1Name(string.Empty, Array.Empty<string>());
			Assert.IsNull(result);
		}
		
		[TestMethod]
		public async Task GetAdmin1Name_DifferentLength()
		{
			var geoReverseLookup = new GeoReverseLookup(_appSettings,
				new FakeIGeoFileDownload(), new FakeIWebLogger());

			await geoReverseLookup.SetupAsync();
			var result = geoReverseLookup.GetAdmin1Name("NL", new string[3]);
			Assert.IsNull(result);
		}
		
		[TestMethod]
		public async Task GetLocation_NonValid()
		{
			var result = await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).GetLocation(516897055,52974817);
			
			Assert.IsFalse(result.IsSuccess);
		}
		
		[TestMethod]
		public async Task GetLocation_NonValid2()
		{
			var result = await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).GetLocation(0,0);
			
			Assert.IsFalse(result.IsSuccess);
		}
		
		[TestMethod]
		public async Task GetLocation_NearestPlace()
		{
			var result = await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).GetLocation(51.69917,5.304170);
			
			Assert.IsTrue(result.IsSuccess);
		}
		
		[TestMethod]
		public async Task GetLocation_NearestPlace_HitGeoDownload()
		{
			var fakeIGeoFileDownload = new FakeIGeoFileDownload();
			
			await new GeoReverseLookup(_appSettings, fakeIGeoFileDownload,
				new FakeIWebLogger()).GetLocation(51.69917,5.304170);
			
			Assert.AreEqual(1, fakeIGeoFileDownload.Count);
		}
		
		[TestMethod]
		public async Task GetLocation_NearestPlace_WithServiceScope()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IGeoFileDownload, FakeIGeoFileDownload>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			var result = await new GeoReverseLookup(_appSettings, scopeFactory,
				new FakeIWebLogger(), new FakeMemoryCache()).GetLocation(51.69917,5.304170);
			
			Assert.IsTrue(result.IsSuccess);
		}
		
		[TestMethod]
		public async Task GetLocation_NearestPlace_WithServiceScope_HitGeoDownload()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IGeoFileDownload, FakeIGeoFileDownload>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			await new GeoReverseLookup(_appSettings, scopeFactory,
				new FakeIWebLogger(), new FakeMemoryCache()).GetLocation(51.69917,5.304170);
			
			var fakeIGeoFileDownload = serviceProvider.GetRequiredService<IGeoFileDownload>() as FakeIGeoFileDownload;
			Assert.AreEqual(1, fakeIGeoFileDownload?.Count);
		}
		
		[TestMethod]
		public async Task GetLocation_NearestPlace2_Uden()
		{
			// 51.6643,5.6196 = uden
			var result = await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).GetLocation(51.6643,5.6196);
			Assert.IsTrue(result.IsSuccess);
		}
		
		
		[TestMethod]
		public async Task GetLocation_NearestPlace2_Valkenswaard()
		{
			// 51.34963/5.46038 = valkenswaard
			var result = await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).GetLocation(51.34963,5.46038);
			
			Assert.IsFalse(result.IsSuccess);
			// 40.0 km
			Assert.AreEqual("Distance to nearest place is too far", result.ErrorReason);
		}

		[TestMethod]
		public async Task GetLocation_No_nearest_place_found()
		{
			await new StorageHostFullPathFilesystem().WriteStreamAsync(
				StringToStreamHelper.StringToStream(string.Empty), // empty file yes!
				Path.Combine(_appSettings.DependenciesFolder, "cities1000.txt"));
			
			var result = await new GeoReverseLookup(_appSettings, new FakeIGeoFileDownload(),
				new FakeIWebLogger()).GetLocation(51.34963,5.46038);
			
			// and undo empty file
			Setup();
			
			Assert.IsFalse(result.IsSuccess);
			Assert.AreEqual("No nearest place found", result.ErrorReason);
		}
	}
}

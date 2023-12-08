using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services
{
	[TestClass]
	public sealed class GeoBackgroundTaskTest
	{
		private AppSettings _appSettings;
		private readonly IGeoLocationWrite _geoLocationWrite;
		private readonly IMemoryCache _memoryCache;
		private readonly FakeIGeoFileDownload _geoFileDownload;

		public GeoBackgroundTaskTest()
		{
			_appSettings = new AppSettings();
			_geoLocationWrite = new FakeIGeoLocationWrite();
			_memoryCache =
				new FakeMemoryCache(new Dictionary<string, object>());
			_geoFileDownload = new FakeIGeoFileDownload();
			Setup();
		}

		private void Setup()
		{
			_appSettings = new AppSettings
			{
				TempFolder = new CreateAnImage().BasePath,
				DependenciesFolder = Path.Combine(new CreateAnImage().BasePath, "tmp-dependencies"),
			};
			
			// create a temp folder;
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
				PlainTextFileHelper.StringToStream(mockCities1000),
				Path.Combine(_appSettings.DependenciesFolder, "cities1000.txt"));
			
			// Mockup data to:
			// map the state and country

			const string admin1CodesAscii = "NL.07\tNorth Holland\tNorth Holland\t2749879\r\n" +
			                                "NL.06\tNorth Brabant\tNorth Brabant\t2749990\r\n" +
			                                "NL.05\tLimburg\tLimburg\t2751596\r\n" +
			                                "NL.03\tGelderland\tGelderland\t2755634\r\n" +
			                                "AR.07\tBuenos Aires F.D.\tBuenos Aires F.D.\t3433955\r\n";

			new StorageHostFullPathFilesystem().WriteStream(
				PlainTextFileHelper.StringToStream(admin1CodesAscii),
				Path.Combine(_appSettings.DependenciesFolder, "admin1CodesASCII.txt"));
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
				
		[TestMethod]
		public async Task GeoBackgroundTask_WithResults_AlreadyHasGps()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/2QOYZWMPACZAJ2MABGMOZ6CCPY.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes.ToArray()}
				);
			var storageSelector = new FakeSelectorStorage(storage);
			var geoReverseLookup = new GeoReverseLookup(_appSettings, _geoFileDownload, new FakeIWebLogger(),_memoryCache);

			var controller = new GeoBackgroundTask(_appSettings, storageSelector,
				_geoLocationWrite, _memoryCache, new FakeIWebLogger(),
				geoReverseLookup);
			
			var results = await controller.GeoBackgroundTaskAsync();
		
			Assert.AreEqual(0, results.Count);
		}
		
		[TestMethod]
		public async Task GeoBackgroundTask_WithResults_NoGps()
		{
			_appSettings.Verbose = true;
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{}, 
				new List<byte[]>{}
			);
			var storageSelector = new FakeSelectorStorage(storage);
			
			var geoReverseLookup = new FakeIGeoReverseLookup(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg")
				{
					FileHash = "2QOYZWMPACZAJ2MABGMOZ6CCPY"
				}
			});

			var controller = new GeoBackgroundTask(_appSettings, storageSelector,
				_geoLocationWrite, _memoryCache, new FakeIWebLogger(),
				geoReverseLookup);
			
			var results = await controller.GeoBackgroundTaskAsync();
		
			Assert.AreEqual(1, results.Count);
		}
		
		[TestMethod]
		public async Task GeoBackgroundTask_IsNotCalled()
		{
			var storage = new FakeIStorage(); // <= main folder not found
			var storageSelector = new FakeSelectorStorage(storage);
			var geoReverseLookup = new FakeIGeoReverseLookup();

			var controller = new GeoBackgroundTask(_appSettings, storageSelector,
				_geoLocationWrite,_memoryCache, 
				new FakeIWebLogger(),  
				geoReverseLookup );

			await controller.GeoBackgroundTaskAsync();
		
			Assert.AreEqual(0, geoReverseLookup.Count);
		}
	}
}

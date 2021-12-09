using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services
{
	[TestClass]
	public class GeoBackgroundTaskTest
	{
		private readonly AppSettings _appSettings;
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
		}
		
		private IServiceScopeFactory GetScope()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task GeoBackgroundTask_WithResults_AlreadyHasGps()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/2QOYZWMPACZAJ2MABGMOZ6CCPY.jpg"}, 
				new List<byte[]>{CreateAnImage.Bytes}
				);
			var storageSelector = new FakeSelectorStorage(storage);
			var geoReverseLookup = new GeoReverseLookup(_appSettings, _geoFileDownload, _memoryCache);

			var controller = new GeoBackgroundTask(_appSettings, storageSelector,
				_geoLocationWrite, _memoryCache, new FakeIWebLogger(),
				_geoFileDownload,geoReverseLookup);
			
			// var is used
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
				_geoFileDownload,geoReverseLookup);
			
			var results = await controller.GeoBackgroundTaskAsync();
		
			Assert.AreEqual(1, results.Count);
		}
		
		
		[TestMethod]
		public void GeoBackgroundTask_IsNotCalled()
		{
			var storage = new FakeIStorage(); // <= main folder not found
			var storageSelector = new FakeSelectorStorage(storage);
			var geoReverseLookup = new FakeIGeoReverseLookup();

			var controller = new GeoBackgroundTask(_appSettings, storageSelector,
				_geoLocationWrite,_memoryCache, 
				new FakeIWebLogger(), _geoFileDownload, 
				geoReverseLookup );

			controller.GeoBackgroundTaskAsync();
		
			Assert.AreEqual(0, geoReverseLookup.Count);
		}
	}
}

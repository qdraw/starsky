using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services
{
	[TestClass]
	public class GeoCliTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public GeoCliTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, StorageHostFullPathFilesystem>();
			services.AddSingleton<ISelectorStorage, SelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task GeoCliInput_Notfound()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			var console = new FakeConsoleWrapper();
			var geoCli = new GeoCli(new FakeIGeoReverseLookup(), new FakeIGeoLocationWrite(),
				new FakeSelectorStorage(new FakeIStorage(new List<string>{})), new AppSettings(),
				console, httpClientHelper,
				new FakeIGeoFileDownload());
			await geoCli.CommandLineAsync(new List<string> {"-p",}.ToArray());

			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("not found"));
		}
		
		[TestMethod]
		public async Task GeoCliInput_RelativeUrl_HappyFlow()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			var relativeParentFolder = new AppSettings().DatabasePathToFilePath(
				new StructureService(new FakeIStorage(), new AppSettings().Structure)
					.ParseSubfolders(0),false);
			
			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes});

			var appSettings = new AppSettings();
			var geoWrite = new FakeIGeoLocationWrite();
			var geoLookup = new FakeIGeoReverseLookup();
			var console = new FakeConsoleWrapper();
			var geoCli = new GeoCli(geoLookup, geoWrite,
				new FakeSelectorStorage(storage), appSettings,
				console, httpClientHelper,
				new FakeIGeoFileDownload());
			await geoCli.CommandLineAsync(new List<string> {"-g", "0"}.ToArray());

			Assert.AreEqual(appSettings.StorageFolder, relativeParentFolder + Path.DirectorySeparatorChar);
			Assert.AreEqual(1, geoLookup.Count);
			Assert.IsTrue(storage.ExistFile("/test.jpg"));
		}
		
		[TestMethod]
		public async Task GeoCliInput_Default_HappyFlow()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			var storage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.jpg"},
				new List<byte[]> {CreateAnImage.Bytes});
			var hash =( await new FileHash(storage).GetHashCodeAsync("/test.jpg")).Key;
			storage.FileCopy("/test.jpg",$"/{hash}.jpg");

			var geoWrite = new FakeIGeoLocationWrite();
			var geoLookup = new FakeIGeoReverseLookup();
			var console = new FakeConsoleWrapper();
			var geoCli = new GeoCli(geoLookup, geoWrite,
				new FakeSelectorStorage(storage), new AppSettings(),
				console, httpClientHelper,
				new FakeIGeoFileDownload());
			await geoCli.CommandLineAsync(new List<string> {"-p",}.ToArray());

			Assert.AreEqual(1, geoLookup.Count);
			Assert.IsTrue(storage.ExistFile($"/{hash}.jpg"));
			Assert.IsTrue(storage.ExistFile("/test.jpg"));
		}
	}
}

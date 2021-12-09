using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starskycore.Interfaces;
using starskycore.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starskyGeoCore.Services
{
	[TestClass]
	public class GeoLocationWriteTest
	{
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;

		public GeoLocationWriteTest()
		{
			// get the service
			_appSettings = new AppSettings();
			_exifTool = new FakeExifTool(new FakeIStorage(),_appSettings );
		}

		[TestMethod]
		public async Task GeoLocationWriteLoopFolderTest()
		{
			var metaFilesInDirectory = new List<FileIndexItem>
			{
				new FileIndexItem
				{
					FileName = "test.jpg", //<= used to check
					ParentDirectory = "/",
					Latitude = 1,
					Longitude = 1,
					LocationAltitude = 1,
					LocationCity = "city",
					LocationState = "state",
					LocationCountry = "country"
				}
			};
			var console = new FakeConsoleWrapper();

			var fakeIStorage = new FakeIStorage();
			await new GeoLocationWrite(_appSettings, _exifTool, new FakeSelectorStorage(fakeIStorage),console).LoopFolderAsync(metaFilesInDirectory, true);
			Assert.IsNotNull(metaFilesInDirectory);
			
			Assert.AreEqual(1,console.WrittenLines.Count);
			Assert.AreEqual("🚀",console.WrittenLines[0]);
		}

		[TestMethod]
		public async Task GeoLocationWriteLoopFolderTest_verbose()
		{
			var metaFilesInDirectory = new List<FileIndexItem>
			{
				new FileIndexItem
				{
					FileName = "test.jpg", //<= used to check
					ParentDirectory = "/",
					Latitude = 1,
					Longitude = 1,
					LocationAltitude = 1,
					LocationCity = "city",
					LocationState = "state",
					LocationCountry = "country"
				}
			};
			var console = new FakeConsoleWrapper();
			await new GeoLocationWrite(new AppSettings{Verbose = true}, _exifTool, new FakeSelectorStorage(),console)
				.LoopFolderAsync(metaFilesInDirectory, 
				true);

			Assert.AreEqual(2,console.WrittenLines.Count);
			Assert.IsTrue(console.WrittenLines.LastOrDefault().Contains("GeoLocationWrite"));
		}
	}
}

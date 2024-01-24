using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.geolookup.Services
{
	[TestClass]
	public sealed class GeoIndexGpxTest
	{
		private readonly AppSettings _appSettings;
		private readonly List<FileIndexItem> _metaFilesDirectory;

		public GeoIndexGpxTest()
		{
			var createAnGpx = new CreateAnGpx();
			_appSettings = new AppSettings
			{
				StorageFolder = createAnGpx.BasePath,
				CameraTimeZone = "Europe/Minsk"
			};

			_metaFilesDirectory = new List<FileIndexItem>
			{
				new FileIndexItem
				{
					FileName = createAnGpx.FileName,
					ImageFormat = ExtensionRolesHelper.ImageFormat.gpx
				}
			};
		}

		[TestMethod]
		public void GeoIndexGpx_ConvertTimeZone_EuropeAmsterdam()
		{
			var fakeIStorage = new FakeIStorage();
			var result = new GeoIndexGpx(new AppSettings{CameraTimeZone = "Europe/Amsterdam"}, 
				fakeIStorage, new FakeIWebLogger()).ConvertTimeZone(new DateTime(2020, 04, 15,
				17, 0, 0, 0, kind: DateTimeKind.Unspecified));
			var expected = new DateTime(2020, 04, 15, 15, 0, 0, 0,
				kind: DateTimeKind.Local);
			
			Assert.AreEqual(expected, result);
		}
        
		[TestMethod]
		public void GeoIndexGpx_ConvertTimeZone_EuropeLondon()
		{
			var fakeIStorage = new FakeIStorage();
			var result = new GeoIndexGpx(new AppSettings{CameraTimeZone = "Europe/London"}, 
				fakeIStorage, new FakeIWebLogger()).ConvertTimeZone(new DateTime(2020, 01, 15,
				17, 0, 0, 0, kind: DateTimeKind.Unspecified));
			var expected = new DateTime(2020, 01, 15, 17, 0, 0, 0,
				kind: DateTimeKind.Local);
			
			Assert.AreEqual(expected, result);
		}
        
		[TestMethod]
		public void GeoIndexGpx_ConvertTimeZone_KindUtc()
		{
			var fakeIStorage = new FakeIStorage();
			var inputDateTime = new DateTime(2020, 01, 15,
				17, 0, 0, 0, kind: DateTimeKind.Unspecified);
			inputDateTime = DateTime.SpecifyKind(inputDateTime, DateTimeKind.Utc);
			var result =new GeoIndexGpx(new AppSettings{CameraTimeZone = "Europe/London"}, 
				fakeIStorage, new FakeIWebLogger()).ConvertTimeZone(inputDateTime);
			var expected = new DateTime(2020, 01, 15, 17, 0, 0, 0,
				kind: DateTimeKind.Local);
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void GeoIndexGpx_ConvertTimeZone_typeLocal_Expect_ArgumentException()
		{
			var fakeIStorage = new FakeIStorage();
			var inputDateTime = new DateTime(2020, 01, 15,
				17, 0, 0, 0, kind: DateTimeKind.Local);
			inputDateTime = DateTime.SpecifyKind(inputDateTime, DateTimeKind.Local);
			new GeoIndexGpx(new AppSettings{CameraTimeZone = "Europe/London"}, 
				fakeIStorage, new FakeIWebLogger()).ConvertTimeZone(inputDateTime);
		}

		[TestMethod]
		public async Task GeoIndexGpx_LoopFolderLookupTest()
		{
			var exampleFiles = new List<FileIndexItem>(); 
			exampleFiles.AddRange(new List<FileIndexItem>
			{
				_metaFilesDirectory.FirstOrDefault()!,
				new FileIndexItem
				{
					FileName = "01.jpg", 
					DateTime = new DateTime(2018,09,05,
						20,31,54, kind: DateTimeKind.Local) // 2018-09-05T17:31:53Z UTC > In europe/Minsk
				},
				new FileIndexItem
				{
					FileName = "NotInRange.jpg", 
					DateTime = new DateTime(2018,09,06,
						00,00,00, kind: DateTimeKind.Local)
				}
                
			});

			var fakeIStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{_metaFilesDirectory[0].FilePath!}, new List<byte[]>{CreateAnGpx.Bytes.ToArray()} );
               
			var returnFileIndexItems = await  new GeoIndexGpx(_appSettings,
				fakeIStorage, new FakeIWebLogger()).LoopFolderAsync(exampleFiles);
            
			Assert.AreEqual(null,returnFileIndexItems.Find(p => p.FileName == "NotInRange.jpg"));
			Assert.AreEqual("01.jpg",returnFileIndexItems.Find(p => p.FileName == "01.jpg")?.FileName);


		}

	}
}

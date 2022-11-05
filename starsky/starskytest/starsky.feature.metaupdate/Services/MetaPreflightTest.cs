using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.metaupdate.Services
{
	[TestClass]
	public sealed class MetaPreflightTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly FakeExifTool _exifTool;
		private readonly ReadMeta _readMeta;
		private readonly IStorage _iStorageFake;
		private readonly Query _queryWithoutCache;
		private string _exampleHash;

		public MetaPreflightTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(MetaUpdateService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext, new AppSettings(),
				null, new FakeIWebLogger(),_memoryCache);
			_queryWithoutCache = new Query(dbContext,
				new AppSettings(), null, new FakeIWebLogger());

			_appSettings = new AppSettings();

			_iStorageFake = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},
				new List<byte[]>{CreateAnImageNoExif.Bytes});
			
			_exifTool = new FakeExifTool(_iStorageFake,_appSettings);

			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/test.jpg").Key;
			_readMeta = new ReadMeta(_iStorageFake,_appSettings,_memoryCache, new FakeIWebLogger());
		}
		
				
		[TestMethod]
		public async Task Preflight_Collections_Enabled()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg"),
					new FileIndexItem("/test.dng"),
				}), 
				new AppSettings(), new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/test.jpg", "/test.dng"}, 
						new []{CreateAnImage.Bytes, CreateAnImage.Bytes}))
				,new FakeIWebLogger());
			
			var result = await metaPreflight.Preflight(
				new FileIndexItem("/test.jpg"), 
				new[] {"/test.jpg"}, true, true, 0);

			Assert.AreEqual(2, result.fileIndexResultsList.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				result.fileIndexResultsList[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				result.fileIndexResultsList[1].Status);
		}
		
		[TestMethod]
		public async Task Preflight_Collections_Disabled()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg"),
					new FileIndexItem("/test.dng"),
				}), 
				new AppSettings(), new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/test.jpg", "/test.dng"}, 
						new []{CreateAnImage.Bytes, CreateAnImage.Bytes}))
				,new FakeIWebLogger());
			
			var result = await metaPreflight.Preflight(
				new FileIndexItem("/test.jpg"), 
				new[] {"/test.jpg"}, true, false, 0);

			Assert.AreEqual(1, result.fileIndexResultsList.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				result.fileIndexResultsList[0].Status);
		}
		
		[TestMethod]
		[ExpectedException(typeof(MissingFieldException))]
		public void UpdateServiceTest_CompareAllLabelsAndRotation_NullMissingFieldException()
		{
			new MetaPreflight(null, null, null, null).
				CompareAllLabelsAndRotation(null, null,
					null, false, 0);
			// ==>> MissingFieldException
		}
		
		
		[TestMethod]
		public void UpdateServiceTest_CompareAllLabelsAndRotation_AppendIsFalse()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			
			var collectionsDetailView = new DetailView
			{
				FileIndexItem = new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "initial Value",
					FileName = "test.jpg",
					ParentDirectory = "/",
					Orientation = FileIndexItem.Rotation.Horizontal
				}
			};

			var statusModel = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "updated Value",
				FileName = "test.jpg",
				ParentDirectory = "/"
			};
			
			// Check for compare values
			new MetaPreflight(_query, _appSettings, 
					new FakeSelectorStorage(_iStorageFake),new FakeIWebLogger())
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView.FileIndexItem,
					statusModel, false, 0);
			
			// Check how that changedFileIndexItemName works
			Assert.AreEqual(1,changedFileIndexItemName["/test.jpg"].Count);
			Assert.AreEqual("tags",changedFileIndexItemName["/test.jpg"].FirstOrDefault());
			
			// Check for value
			Assert.AreEqual("updated Value", collectionsDetailView.FileIndexItem.Tags);
			Assert.AreEqual(FileIndexItem.Rotation.Horizontal, 
				collectionsDetailView.FileIndexItem.Orientation);
		}

		[TestMethod]
		public void Update_should_ignore_capital_compare()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>
			{
				{ "/test.jpg", new List<string>() }
			};
			
			var collectionsDetailView = new DetailView
			{
				FileIndexItem = new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "Value",
					FileName = "test.jpg",
					ParentDirectory = "/",
					Orientation = FileIndexItem.Rotation.Horizontal
				}
			};

			var statusModel = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "VALUE", // <-- capitals that's the diff
				FileName = "test.jpg",
				ParentDirectory = "/"
			};
			
			// Check for compare values
			new MetaPreflight(_query, _appSettings, 
					new FakeSelectorStorage(_iStorageFake),new FakeIWebLogger())
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView.FileIndexItem,
					statusModel, false, 0);
			
			Assert.AreEqual(0,changedFileIndexItemName["/test.jpg"].Count);
		}
		
		[TestMethod]
		public void UpdateServiceTest_ShouldOverwrite()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>
			{
				{ "/test.jpg", new List<string>() }
			};
			
			var collectionsDetailView = new DetailView
			{
				FileIndexItem = new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "initial Value",
					FileName = "test.jpg",
					ParentDirectory = "/",
					Orientation = FileIndexItem.Rotation.Horizontal
				}
			};

			var statusModel = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "updated Value",
				FileName = "test.jpg",
				ParentDirectory = "/"
			};
			
			// Check for compare values
			new MetaPreflight(_query, _appSettings, 
					new FakeSelectorStorage(_iStorageFake),new FakeIWebLogger())
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView.FileIndexItem,
					statusModel, false, 0);
			
			Assert.AreEqual("tags",changedFileIndexItemName["/test.jpg"][0]);
		}
	
		[TestMethod]
		public void UpdateServiceTest_CompareAllLabelsAndRotation_Rotate270Cw()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			
			var collectionsDetailView = new DetailView
			{
				FileIndexItem = new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "initial Value",
					FileName = "test.jpg",
					ParentDirectory = "/"
				}
			};
			
			// Rotate right; check if values are the same
			new MetaPreflight(_query, _appSettings, 
					new FakeSelectorStorage(_iStorageFake),new FakeIWebLogger())
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView.FileIndexItem, 
					collectionsDetailView.FileIndexItem, 
					false, -1);
			
			// Check for value
			Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw, 
				collectionsDetailView.FileIndexItem.Orientation);
		}

		[TestMethod]
		public async Task Preflight_NotFoundNotInIndex()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(), new AppSettings(), 
				new FakeSelectorStorage(),new FakeIWebLogger());
			var result = await metaPreflight.Preflight(
				new FileIndexItem("test"), 
				new[] {"test"}, true, true, 0);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, 
				result.fileIndexResultsList.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task Preflight_ReadOnly()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/readonly/test.jpg")
				}), 
				new AppSettings{ ReadOnlyFolders = new List<string>{"readonly"}}, new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/readonly/test.jpg"}, 
						new []{CreateAnImage.Bytes, })),new FakeIWebLogger());
			
			var result = await metaPreflight.Preflight(
				new FileIndexItem("/readonly/test.jpg"), 
				new[] {"/readonly/test.jpg"}, true, true, 0);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.ReadOnly, 
				result.fileIndexResultsList.FirstOrDefault().Status);
			Assert.AreEqual("", result.fileIndexResultsList.FirstOrDefault().Tags);
		}
		
		[TestMethod]
		public async Task Preflight_Deleted()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(new List<FileIndexItem>
				{
					new FileIndexItem("/deleted.jpg") {Tags = "!delete!"}
				}), 
				new AppSettings(), new FakeSelectorStorage(
					new FakeIStorage(new List<string>(), 
						new List<string>{"/deleted.jpg"}, 
						new []{CreateAnImage.Bytes, })),new FakeIWebLogger());
			
			var result = await metaPreflight.Preflight(
				new FileIndexItem("/deleted.jpg"), 
				new[] {"/deleted.jpg"}, 
				true, true, 0);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, 
				result.fileIndexResultsList.FirstOrDefault().Status);
			Assert.AreEqual("!delete!", result.fileIndexResultsList.FirstOrDefault().Tags);
		}

		[TestMethod]
		public void RotationCompare_DoNotRotate()
		{
			var metaPreflight = new MetaPreflight(_query, _appSettings,
				new FakeSelectorStorage(_iStorageFake),new FakeIWebLogger());
			var compareList = new List<string>();

			var rotationCompare = MetaPreflight.RotationCompare(0, 
				new FileIndexItem("/test.jpg"){Orientation = FileIndexItem.Rotation.Horizontal},
				compareList);
			Assert.AreEqual(rotationCompare.Orientation, FileIndexItem.Rotation.Horizontal);
			Assert.IsTrue(!compareList.Any());
		}
		
		[TestMethod]
		public void RotationCompare_Plus1()
		{
			var metaPreflight = new MetaPreflight(_query, _appSettings,
				new FakeSelectorStorage(_iStorageFake),new FakeIWebLogger());
			var compareList = new List<string>();
			var rotationCompare = MetaPreflight.RotationCompare(1, 
				new FileIndexItem("/test.jpg"){Orientation = FileIndexItem.Rotation.Horizontal},
				compareList);
			Assert.AreEqual(rotationCompare.Orientation, FileIndexItem.Rotation.Rotate90Cw);
			Assert.AreEqual(compareList.FirstOrDefault(),
				nameof(FileIndexItem.Orientation).ToLowerInvariant());
		}
		
		[TestMethod]
		public void RotationCompare_Minus1()
		{
			var metaPreflight = new MetaPreflight(_query, _appSettings,
				new FakeSelectorStorage(_iStorageFake),new FakeIWebLogger());
			var compareList = new List<string>();
			var rotationCompare = MetaPreflight.RotationCompare(-1, 
				new FileIndexItem("/test.jpg"){Orientation = FileIndexItem.Rotation.Horizontal},
				compareList);
			Assert.AreEqual(rotationCompare.Orientation, FileIndexItem.Rotation.Rotate270Cw);
			Assert.AreEqual(compareList.FirstOrDefault(),
				nameof(FileIndexItem.Orientation).ToLowerInvariant());
		}
		
		[TestMethod]
		public async Task Preflight_NotFoundSourceMissing()
		{
			var metaPreflight = new MetaPreflight(new FakeIQuery(
					new List<FileIndexItem>{new FileIndexItem("/test.jpg")}), new AppSettings(), 
				new FakeSelectorStorage(),new FakeIWebLogger());
			
			var result = await metaPreflight.Preflight(
				new FileIndexItem("/test.jpg"), 
				new[] {"/test.jpg"}, true, true, 0);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, 
				result.fileIndexResultsList.FirstOrDefault().Status);
		}


	}
}

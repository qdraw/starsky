using System;
using System.Collections.Generic;
using System.IO;
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
	public class MetaUpdateServiceTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly FakeExifTool _exifTool;
		private readonly ReadMeta _readMeta;
		private readonly IStorage _iStorageFake;
		private readonly Query _queryWithoutCache;
		private string _exampleHash;

		public MetaUpdateServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(MetaUpdateService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext,_memoryCache);
			_queryWithoutCache = new Query(dbContext,null, new AppSettings{ AddMemoryCache = false});

			_appSettings = new AppSettings();

			_iStorageFake = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg", _exampleHash,
					"/test_default.jpg"},
				new List<byte[]>{FakeCreateAn.CreateAnImageNoExif.Bytes});
			
			_exifTool = new FakeExifTool(_iStorageFake,_appSettings);

			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/test.jpg").Key;
			_readMeta = new ReadMeta(_iStorageFake,_appSettings,_memoryCache);
		}

		
		[TestMethod]
		public void UpdateService_Update_defaultTest()
		{
			var item0 = _query.AddItem(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "thisKeywordHasChanged",
				FileName = "test_default.jpg",
				Description = "noChanges",
				ParentDirectory = "/"
			});
			
			var changedFileIndexItemName = new Dictionary<string, List<string>>
			{
				{ 
					"/test_default.jpg", new List<string>
					{
						nameof(FileIndexItem.Tags)
					} 
				},
			};
			
			var fileIndexResultsList = new List<FileIndexItem>
			{
				new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "initial tags (from database)",
					FileName = "test_default.jpg",
					ParentDirectory = "/",
					Description = "keep",
				}
			};

			var updateItem = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "only used when Caching is disabled",
				FileName = "test_default.jpg",
				Description = "noChanges",
				ParentDirectory = "/"
			};

			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), _appSettings, _memoryCache);
			
			service.UpdateAsync(changedFileIndexItemName, fileIndexResultsList, updateItem, false,false,0);

			// check for item (Referenced)
			Assert.AreEqual("thisKeywordHasChanged",item0.Tags);
			// db
			Assert.AreEqual("thisKeywordHasChanged",_query.SingleItem("/test_default.jpg").FileIndexItem.Tags);
			
			Assert.AreEqual("noChanges",_query.SingleItem("/test_default.jpg").FileIndexItem.Description);

			_query.RemoveItem(item0);
		}
		
		[TestMethod]
		public async Task UpdateService_Update_toDelete()
		{
			await _query.AddItemAsync(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "",
				FileName = "test_delete.jpg",
				Description = "noChanges",
				ParentDirectory = "/delete"
			});

			var item0 = _query.GetObjectByFilePath("/delete/test_delete.jpg");
			item0.Tags = "!delete!";
			
			var changedFileIndexItemName = new Dictionary<string, List<string>>
			{
				{ 
					"/delete/test_delete.jpg", new List<string>
					{
						nameof(FileIndexItem.Tags)
					} 
				},
			};
		
			var fileIndexResultsList = new List<FileIndexItem>
			{
				item0
			};

			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), _appSettings, _memoryCache);
			
			await service.UpdateAsync(changedFileIndexItemName, fileIndexResultsList, null, false,false,0);

			// Deleted status is done in the Preflight stage
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,fileIndexResultsList[0].Status);

			// db
			Assert.AreEqual("!delete!",_query.GetObjectByFilePath("/delete/test_delete.jpg").Tags);
			
			Assert.AreEqual("noChanges",_query.GetObjectByFilePath("/delete/test_delete.jpg").Description);

			_query.RemoveItem(item0);
		}

		
		[TestMethod]
		public async Task UpdateService_Update_NoChangedFileIndexItemName_AndHasChanged()
		{
			var databaseItem = await _query.AddItemAsync(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "databaseItem",
				FileName = "test.jpg",
				ParentDirectory = "/",
				IsDirectory = false
			});

			var fileIndexResultsList = new List<FileIndexItem>
			{
				new FileIndexItem
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = "initial tags (from database)",
					FileName = "test.jpg",
					ParentDirectory = "/",
					Description = "keep",
					IsDirectory = false
				}
			};

			var toUpdateItem = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "databaseItem",
				FileName = "test.jpg",
				ParentDirectory = "/",
				IsDirectory = false
			};

			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), _appSettings, _memoryCache);
			
			await service.UpdateAsync(null, fileIndexResultsList, 
				toUpdateItem, false,false,0);
			// Second one is null

			// check for item (Referenced)
			Assert.AreEqual("databaseItem",toUpdateItem.Tags);
			// db
			Assert.AreEqual("databaseItem",_query.SingleItem("/test.jpg").FileIndexItem.Tags);

			_query.RemoveItem(databaseItem);
		}

		[TestMethod]
		public async Task Update_Write_GPX()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>{
			{
				"/test.gpx", new List<string>{"Tags"}
			}};

			_iStorageFake.WriteStream(new MemoryStream(CreateAnGpx.Bytes), "/test.gpx");
			var updateItem = new FileIndexItem("/test.gpx")
			{
				Tags = "test",
				Status = FileIndexItem.ExifStatus.Ok
			};

			await _query.AddItemAsync(updateItem);
			
			var fileIndexResultsList = new List<FileIndexItem>{updateItem};

			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), _appSettings, _memoryCache);
			
			await service.UpdateAsync(changedFileIndexItemName, fileIndexResultsList, updateItem,false,false,0);

			Assert.IsTrue(_iStorageFake.ExistFile("/.starsky.test.gpx.json"));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task Update_Exception_MissingInList()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			var fileIndexResultList = new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg") {Status = FileIndexItem.ExifStatus.Ok}
			};
			
			var service = new MetaUpdateService(_query, _exifTool,
					new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
					new FakeIWebLogger(), _appSettings, _memoryCache);
			
			await service.UpdateAsync(changedFileIndexItemName, fileIndexResultList , 
					null,false,false,0);
			// expect exception
		}
		
		[TestMethod]
		public async Task UpdateRotate()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>{
			{
				"/test.jpg", new List<string>{"orientation"}
			}};
			await _iStorageFake.WriteStreamAsync(new MemoryStream(CreateAnImage.Bytes), "/test.jpg");
			var updateItem = new FileIndexItem("/test.jpg")
			{
				Orientation = FileIndexItem.Rotation.Horizontal,
				Status = FileIndexItem.ExifStatus.Ok,
				FileHash = "test"
			};

			await _query.AddItemAsync(updateItem);
			
			var fileIndexResultsList = new List<FileIndexItem>{updateItem};

			var service = new MetaUpdateService(_query, _exifTool,
					new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
					new FakeIWebLogger(), _appSettings, _memoryCache);
				
			await service
				.UpdateAsync(changedFileIndexItemName, fileIndexResultsList, updateItem,false,
					false,1);

			// so there is something changed
			Assert.AreNotEqual("test", updateItem.FileHash);

			await _query.RemoveItemAsync(updateItem);
		}

		[TestMethod]
		public async Task ApplyOrGenerateUpdatedFileHash_Should_Update_WhenNotNull()
		{
			var detailView = new DetailView
			{
				FileIndexItem = new FileIndexItem("/test.jpg")
			};
			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), _appSettings, _memoryCache);
			
			await service.ApplyOrGenerateUpdatedFileHash(new List<string>(), detailView.FileIndexItem);
			
			Assert.IsNotNull(detailView.FileIndexItem.FileHash);
		}
	}
}

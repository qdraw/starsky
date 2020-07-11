using System.Collections.Generic;
using System.IO;
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
	public class UpdateServiceTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly FakeExifTool _exifTool;
		private readonly ReadMeta _readMeta;
		private readonly IStorage _iStorageFake;
		private readonly Query _queryWithoutCache;
		private string _exampleHash;

		public UpdateServiceTest()
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
			_queryWithoutCache = new Query(dbContext,null);

			_appSettings = new AppSettings();

			_iStorageFake = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg", _exampleHash},
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
				FileName = "test.jpg",
				Description = "noChanges",
				ParentDirectory = "/"
			});
			
			var changedFileIndexItemName = new Dictionary<string, List<string>>
			{
				{ 
					"/test.jpg", new List<string>
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
					FileName = "test.jpg",
					ParentDirectory = "/",
					Description = "keep",
				}
			};

			var updateItem = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "only used when Caching is disabled",
				FileName = "test.jpg",
				Description = "noChanges",
				ParentDirectory = "/"
			};

			new MetaUpdateService(_query,_exifTool, _readMeta, new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),  
				new FakeConsoleWrapper(new List<string>()))
				.Update(changedFileIndexItemName,fileIndexResultsList, updateItem, false,false,0);

			// check for item (Referenced)
			Assert.AreEqual("thisKeywordHasChanged",item0.Tags);
			// db
			Assert.AreEqual("thisKeywordHasChanged",_query.SingleItem("/test.jpg").FileIndexItem.Tags);
			
			Assert.AreEqual("noChanges",_query.SingleItem("/test.jpg").FileIndexItem.Description);

			_query.RemoveItem(item0);
		}

		
		[TestMethod]
		public void UpdateService_Update_NoChangedFileIndexItemName()
		{
			var item0 = _query.AddItem(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "thisKeywordHasChanged",
				FileName = "test.jpg",
				Description = "noChanges",
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

			var updateItem = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "only used when NoChangedFileIndexItemName",
				FileName = "test.jpg",
				Description = "noChanges",
				ParentDirectory = "/",
				IsDirectory = false
			};

			new MetaUpdateService(_query,_exifTool, _readMeta, 
					new FakeSelectorStorage(_iStorageFake), 
					new FakeMetaPreflight(), 
					new FakeConsoleWrapper())
				.Update(null,fileIndexResultsList, updateItem, false,false,0);
			// Second one is null

			// check for item (Referenced)
			Assert.AreEqual("only used when NoChangedFileIndexItemName",item0.Tags);
			// db
			Assert.AreEqual("only used when NoChangedFileIndexItemName",_query.SingleItem("/test.jpg").FileIndexItem.Tags);
			
			Assert.AreEqual("noChanges",_query.SingleItem("/test.jpg").FileIndexItem.Description);

			_query.RemoveItem(item0);
		}
		
		[TestMethod]
		public void UpdateService_Update_DisabledCache()
		{
			_query.AddItem(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "thisKeywordHasChanged",
				FileName = "test.jpg",
				Description = "noChanges",
				ParentDirectory = "/",
				Id = 100
			});
			
			var changedFileIndexItemName = new Dictionary<string, List<string>>
			{
				{ 
					"/test.jpg", new List<string>
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
					Tags = "initial tags",
					FileName = "test.jpg",
					FileHash = "test.jpg",
					ParentDirectory = "/",
					Description = "keep",
				}
			};
			
			var updateItem = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "only used when Caching is disabled",
				FileName = "test.jpg",
				FileHash = "test.jpg",
				Description = "noChanges",
				ParentDirectory = "/"
			};
			
			var appSettings = new AppSettings{AddMemoryCache = false};
			var readMetaWithNoCache = new ReadMeta(_iStorageFake,appSettings);
			
			new MetaUpdateService(_query,_exifTool, readMetaWithNoCache, new FakeSelectorStorage(_iStorageFake), 
					new FakeMetaPreflight(),  
					new FakeConsoleWrapper())
				.Update(changedFileIndexItemName, fileIndexResultsList, updateItem,false,false,0);

			// db
			Assert.AreEqual("only used when Caching is disabled",_query.SingleItem("/test.jpg").FileIndexItem.Tags);
			
			Assert.AreEqual("noChanges",_query.SingleItem("/test.jpg").FileIndexItem.Description);

			// need to reload again due tracking changes
			_queryWithoutCache.RemoveItem(_queryWithoutCache.SingleItem("/test.jpg").FileIndexItem);
		}

		[TestMethod]
		public void Update_Write_GPX()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>();

			_iStorageFake.WriteStream(new MemoryStream(CreateAnGpx.Bytes), "/test.gpx");
			var updateItem = new FileIndexItem("/test.gpx")
			{
				Tags = "test",
				Status = FileIndexItem.ExifStatus.Ok
			};

			_query.AddItem(updateItem);
			
			var fileIndexResultsList = new List<FileIndexItem>{updateItem};

			new MetaUpdateService(_query,_exifTool, _readMeta, new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),  
				new FakeConsoleWrapper(new List<string>()))
				.Update(changedFileIndexItemName, fileIndexResultsList, updateItem,false,false,0);

			Assert.IsTrue(_iStorageFake.ExistFile("/.starsky.test.gpx.json"));
		}
	}
}

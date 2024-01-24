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
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.metaupdate.Services
{
	[TestClass]
	public sealed class MetaUpdateServiceTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly FakeExifTool _exifTool;
		private readonly IStorage _iStorageFake;

		public MetaUpdateServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetRequiredService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(MetaUpdateService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext, new AppSettings(), null!,new FakeIWebLogger(),_memoryCache);

			_appSettings = new AppSettings();

			_iStorageFake = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg", "_exampleHash",
					"/test_default.jpg"},
				new List<byte[]>{CreateAnImageNoExif.Bytes.ToArray()});
			
			_exifTool = new FakeExifTool(_iStorageFake,_appSettings);
		}

		
		[TestMethod]
		public async Task UpdateService_Update_defaultTest()
		{
			var item0 = await _query.AddItemAsync(new FileIndexItem
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

			var readMeta = new ReadMetaSubPathStorage(
				new FakeSelectorStorage(_iStorageFake), _appSettings,
				_memoryCache, new FakeIWebLogger());
			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, 
				new FakeIThumbnailService(new FakeSelectorStorage(_iStorageFake)), new FakeIThumbnailQuery());
			
			await service.UpdateAsync(changedFileIndexItemName, fileIndexResultsList, updateItem, false,false,0);

			// check for item (Referenced)
			Assert.AreEqual("thisKeywordHasChanged",item0.Tags);
			// db
			Assert.AreEqual("thisKeywordHasChanged",_query.SingleItem("/test_default.jpg")!.FileIndexItem!.Tags);
			
			Assert.AreEqual("noChanges",_query.SingleItem("/test_default.jpg")!.FileIndexItem!.Description);

			await _query.RemoveItemAsync(item0);
		}
		
		[TestMethod]
		public async Task UpdateService_Update_toDelete()
		{
			var query = new FakeIQuery();
			await query.AddItemAsync(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "",
				FileName = "test_delete.jpg",
				Description = "noChanges",
				ParentDirectory = "/delete",
				Id = 9
			});

			var item0 = query.GetObjectByFilePath("/delete/test_delete.jpg");
			item0!.Tags = TrashKeyword.TrashKeywordString;
			
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

			var readMeta = new FakeReadMetaSubPathStorage();
			var service = new MetaUpdateService(query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, 
				new FakeIThumbnailService(new FakeSelectorStorage(_iStorageFake)), new FakeIThumbnailQuery());
			
			await service.UpdateAsync(changedFileIndexItemName, fileIndexResultsList, null, false,false,0);

			// Deleted status is done in the Preflight stage
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,fileIndexResultsList[0].Status);

			// db
			Assert.AreEqual(TrashKeyword.TrashKeywordString,query.GetObjectByFilePath("/delete/test_delete.jpg")!.Tags);
			
			Assert.AreEqual("noChanges",query.GetObjectByFilePath("/delete/test_delete.jpg")!.Description);

			await query.RemoveItemAsync(item0);
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

			var readMeta = new ReadMetaSubPathStorage(
				new FakeSelectorStorage(_iStorageFake), _appSettings,
				_memoryCache, new FakeIWebLogger());
			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, 
				new FakeIThumbnailService(new FakeSelectorStorage(_iStorageFake)), new FakeIThumbnailQuery());
			
			await service.UpdateAsync(null, fileIndexResultsList, 
				toUpdateItem, false,false,0);
			// Second one is null

			Assert.IsNotNull(_query.SingleItem("/test.jpg"));
			Assert.IsNotNull(_query.SingleItem("/test.jpg")!.FileIndexItem);
			Assert.IsNotNull(_query.SingleItem("/test.jpg")!.FileIndexItem!.Tags);

			// check for item (Referenced)
			Assert.AreEqual("databaseItem",toUpdateItem.Tags);
			// db
			Assert.AreEqual("databaseItem",_query.SingleItem("/test.jpg")!.FileIndexItem!.Tags);

			await _query.RemoveItemAsync(databaseItem);
		}

		[TestMethod]
		public async Task Update_Write_GPX()
		{
			var changedFileIndexItemName = new Dictionary<string, List<string>>{
			{
				"/test.gpx", new List<string>{"Tags"}
			}};

			await _iStorageFake.WriteStreamAsync(new MemoryStream(CreateAnGpx.Bytes.ToArray()), "/test.gpx");
			var updateItem = new FileIndexItem("/test.gpx")
			{
				Tags = "test",
				Status = FileIndexItem.ExifStatus.Ok
			};

			var query = new FakeIQuery();
			await query.AddItemAsync(updateItem);
			
			var fileIndexResultsList = new List<FileIndexItem>{updateItem};

			var readMeta = new FakeReadMetaSubPathStorage();
			var service = new MetaUpdateService(query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, 
				new FakeIThumbnailService(new FakeSelectorStorage(_iStorageFake)), new FakeIThumbnailQuery());
			
			await service.UpdateAsync(changedFileIndexItemName, fileIndexResultsList, 
				updateItem,false,false,0);

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
			
			var readMeta = new ReadMetaSubPathStorage(
				new FakeSelectorStorage(_iStorageFake), _appSettings,
				_memoryCache, new FakeIWebLogger());
			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, 
				new FakeIThumbnailService(new FakeSelectorStorage(_iStorageFake)), new FakeIThumbnailQuery());
			
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
			await _iStorageFake.WriteStreamAsync(new MemoryStream(CreateAnImage.Bytes.ToArray()), "/test.jpg");
			var updateItem = new FileIndexItem("/test.jpg")
			{
				Orientation = FileIndexItem.Rotation.Horizontal,
				Status = FileIndexItem.ExifStatus.Ok,
				FileHash = "test"
			};

			var query = new FakeIQuery();
			await query.AddItemAsync(updateItem);
			
			var fileIndexResultsList = new List<FileIndexItem>{updateItem};

			var readMeta = new FakeReadMetaSubPathStorage();
			var service = new MetaUpdateService(query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, 
				new FakeIThumbnailService(new FakeSelectorStorage(_iStorageFake)), 
				new FakeIThumbnailQuery());
				
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

			var readMeta = new ReadMetaSubPathStorage(
				new FakeSelectorStorage(_iStorageFake), _appSettings,
				_memoryCache, new FakeIWebLogger());
			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, 
				new FakeIThumbnailService(new FakeSelectorStorage(_iStorageFake)), 
				new FakeIThumbnailQuery());
			
			await service.ApplyOrGenerateUpdatedFileHash(new List<string>(), detailView.FileIndexItem);
			
			Assert.IsNotNull(detailView.FileIndexItem.FileHash);
		}

		[TestMethod]
		public async Task RotationThumbnailExecute_Rotation0_soSkip()
		{
			var thumbnailService =
				new FakeIThumbnailService(
					new FakeSelectorStorage(_iStorageFake));
			var readMeta = new ReadMetaSubPathStorage(
				new FakeSelectorStorage(_iStorageFake), _appSettings,
				_memoryCache, new FakeIWebLogger());
			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, thumbnailService, 
				new FakeIThumbnailQuery());
			
			await service.RotationThumbnailExecute(0, new FileIndexItem("/test.jpg"));
			
			Assert.AreEqual(0,thumbnailService.InputsRotate.Count);
		}
		
		[TestMethod]
		public async Task RotationThumbnailExecute2()
		{
			var thumbnailService =
				new FakeIThumbnailService(
					new FakeSelectorStorage(_iStorageFake));
			var readMeta = new ReadMetaSubPathStorage(
				new FakeSelectorStorage(_iStorageFake), _appSettings,
				_memoryCache, new FakeIWebLogger());
			
			var service = new MetaUpdateService(_query, _exifTool,
				new FakeSelectorStorage(_iStorageFake), new FakeMetaPreflight(),
				new FakeIWebLogger(), readMeta, thumbnailService, 
				new FakeIThumbnailQuery());
			
			await service.RotationThumbnailExecute(1, new FileIndexItem("/test.jpg"));
			
			Assert.AreEqual(ThumbnailNameHelper.AllThumbnailSizes.Length,
				thumbnailService.InputsRotate.Count);
			
			Assert.IsTrue(thumbnailService.InputsRotate.Exists(p => p.Item3 == 
				ThumbnailNameHelper.GetSize(ThumbnailSize.Small)));
			Assert.IsTrue(thumbnailService.InputsRotate.Exists(p => p.Item3 == 
				ThumbnailNameHelper.GetSize(ThumbnailSize.TinyMeta)));
			Assert.IsTrue(thumbnailService.InputsRotate.Exists(p => p.Item3 == 
				ThumbnailNameHelper.GetSize(ThumbnailSize.Large)));
			Assert.IsTrue(thumbnailService.InputsRotate.Exists(p => p.Item3 == 
				ThumbnailNameHelper.GetSize(ThumbnailSize.ExtraLarge)));
		}
	}
}

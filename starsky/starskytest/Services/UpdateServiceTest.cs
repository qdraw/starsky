using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.query.Interfaces;
using starsky.foundation.query.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;
using starskytest.FakeMocks;
using starskytest.Models;
using Query = starsky.foundation.query.Services.Query;

namespace starskytest.Services
{
	[TestClass]
	public class UpdateServiceTest
	{
		private readonly IMemoryCache _memoryCache;
		private IQuery _query;
		private AppSettings _appSettings;
		private FakeExifTool _exifTool;
		private ReadMeta _readMeta;
		private IStorage _iStorageFake;
		private readonly Query _queryWithoutCache;
		private string _exampleHash;

		public UpdateServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(UpdateService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext,_memoryCache);
			_queryWithoutCache = new Query(dbContext,null);

			_appSettings = new AppSettings();

			_iStorageFake = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg", _exampleHash},
				new List<byte[]>{FakeCreateAn.CreateAnImageNoExif.Bytes});
			
			_exifTool = new FakeExifTool(_iStorageFake,_appSettings);

			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/test.jpg");
			_readMeta = new ReadMeta(_iStorageFake,_appSettings,_memoryCache);
			
		}

		[TestMethod]
		[ExpectedException(typeof(MissingFieldException))]
		public void UpdateServiceTest_CompareAllLabelsAndRotation_NullMissingFieldException()
		{
			new UpdateService(null, null, null, null, null).CompareAllLabelsAndRotation(null, null,
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
			new UpdateService(_query, _exifTool, _readMeta,_iStorageFake,_iStorageFake)
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView, statusModel, false, 0);
			
			// Check how that changedFileIndexItemName works
			Assert.AreEqual(1,changedFileIndexItemName["/test.jpg"].Count);
			Assert.AreEqual("Tags",changedFileIndexItemName["/test.jpg"].FirstOrDefault());
			
			// Check for value
			Assert.AreEqual("updated Value", collectionsDetailView.FileIndexItem.Tags);
			Assert.AreEqual(FileIndexItem.Rotation.Horizontal, collectionsDetailView.FileIndexItem.Orientation);

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
			new UpdateService(_query, _exifTool, _readMeta,_iStorageFake,_iStorageFake)
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView, collectionsDetailView.FileIndexItem, false, -1);
			
			// Check for value
			Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw, collectionsDetailView.FileIndexItem.Orientation);
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

			new UpdateService(_query,_exifTool, _readMeta,_iStorageFake,_iStorageFake)
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
				ParentDirectory = "/"
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
				}
			};

			var updateItem = new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "only used when NoChangedFileIndexItemName",
				FileName = "test.jpg",
				Description = "noChanges",
				ParentDirectory = "/"
			};

			new UpdateService(_query,_exifTool, _readMeta,_iStorageFake,_iStorageFake)
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
			new UpdateService(_queryWithoutCache,_exifTool, readMetaWithNoCache, _iStorageFake, _iStorageFake)
				.Update(changedFileIndexItemName, fileIndexResultsList,updateItem,false,false,0);

			// db
			Assert.AreEqual("only used when Caching is disabled",_query.SingleItem("/test.jpg").FileIndexItem.Tags);
			
			Assert.AreEqual("noChanges",_query.SingleItem("/test.jpg").FileIndexItem.Description);

			// need to reload again due tracking changes
			_queryWithoutCache.RemoveItem(_queryWithoutCache.SingleItem("/test.jpg").FileIndexItem);
			


		}
		
	}
}

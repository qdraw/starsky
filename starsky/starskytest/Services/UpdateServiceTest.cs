using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;
using starskytest.FakeMocks;
using starskytest.Models;

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
			_appSettings = new AppSettings();
			_exifTool = new FakeExifTool();
			_iStorageFake = new FakeIStorage(new List<string>{},new List<string>{"/test.jpg"});
			_readMeta = new ReadMeta(_iStorageFake,_appSettings,_memoryCache);
		}

//		[TestMethod]
//		public void Test()
//		{
//		}
		
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
			new UpdateService(_query, _exifTool, _appSettings, _readMeta,_iStorageFake)
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
			new UpdateService(_query, _exifTool, _appSettings, _readMeta,_iStorageFake)
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView, collectionsDetailView.FileIndexItem, false, -1);
			
			// Check for value
			Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw, collectionsDetailView.FileIndexItem.Orientation);
		}
		
		
		[TestMethod]
		public void UpdateService_Update_Test1()
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
					Tags = "initial tags",
					FileName = "test.jpg",
					ParentDirectory = "/",
					Description = "keep",
				}
			};

			new UpdateService(_query,_exifTool,_appSettings, _readMeta,_iStorageFake).Update(changedFileIndexItemName,fileIndexResultsList.FirstOrDefault(), fileIndexResultsList,false,0);

			// check for item (Referenced)
			Assert.AreEqual("thisKeywordHasChanged",item0.Tags);
			// db
			Assert.AreEqual("thisKeywordHasChanged",_query.SingleItem("/test.jpg").FileIndexItem.Tags);
			
			Assert.AreEqual("noChanges",_query.SingleItem("/test.jpg").FileIndexItem.Description);

			_query.RemoveItem(item0);
		}

	}
}

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
		private FakeExiftool _exifTool;
		private ReadMeta _readMeta;

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
			_exifTool = new FakeExiftool();
			var fakeStorage = new FakeIStorage();
			_readMeta = new ReadMeta(fakeStorage,_appSettings,_memoryCache);
		}
		[TestMethod]
		public void Test()
		{
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
			new UpdateService(_query, _exifTool, _appSettings, _readMeta)
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
			new UpdateService(_query, _exifTool, _appSettings, _readMeta)
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView, collectionsDetailView.FileIndexItem, false, -1);
			
			// Check for value
			Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw, collectionsDetailView.FileIndexItem.Orientation);
		}
		
		
		[TestMethod]
		public void Test1()
		{
			var item0 = _query.AddItem(new FileIndexItem
			{
				Status = FileIndexItem.ExifStatus.Ok,
				Tags = "",
				FileName = "test.jpg",
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
					Tags = "thisValueIsUpdated",
					FileName = "test.jpg",
					ParentDirectory = "/"
				}
			};

			new UpdateService(_query,_exifTool,_appSettings, _readMeta).Update(changedFileIndexItemName,fileIndexResultsList.FirstOrDefault(), fileIndexResultsList,false,0);



			_query.RemoveItem(item0);
		}

	}
}

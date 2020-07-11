using System;
using System.Collections.Generic;
using System.Linq;
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
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.feature.metaupdate.Services
{
	[TestClass]
	public class MetaPreflightTest
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
		[ExpectedException(typeof(MissingFieldException))]
		public void UpdateServiceTest_CompareAllLabelsAndRotation_NullMissingFieldException()
		{
			new MetaPreflight(null, null, null).
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
			new MetaPreflight(_query, _appSettings, new FakeSelectorStorage(_iStorageFake))
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView, statusModel, false, 0);
			
			// Check how that changedFileIndexItemName works
			Assert.AreEqual(1,changedFileIndexItemName["/test.jpg"].Count);
			Assert.AreEqual("tags",changedFileIndexItemName["/test.jpg"].FirstOrDefault());
			
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
			new MetaPreflight(_query, _appSettings, new FakeSelectorStorage(_iStorageFake))
				.CompareAllLabelsAndRotation(changedFileIndexItemName, collectionsDetailView, collectionsDetailView.FileIndexItem, 
					false, -1);
			
			// Check for value
			Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw, collectionsDetailView.FileIndexItem.Orientation);
		}
		
	}
}

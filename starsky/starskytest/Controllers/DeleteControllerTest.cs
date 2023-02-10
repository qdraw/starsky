using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public sealed class DeleteControllerTest
	{
		
		private readonly IQuery _query;
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly ApplicationDbContext _context;
		private readonly IReadMeta _readmeta;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IStorage _iStorage;

		public DeleteControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
            
			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase("test1234");
			var options = builderDb.Options;
			_context = new ApplicationDbContext(options);
			_query = new Query(_context, 
				new AppSettings(), null, new FakeIWebLogger(),memoryCache);
            
			// Inject Fake ExifTool; dependency injection
			var services = new ServiceCollection();

			// Fake the readMeta output
			services.AddSingleton<IReadMeta, FakeReadMeta>();    
            
			// Inject Config helper
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			// random config
			_createAnImage = new CreateAnImage();
			var dict = new Dictionary<string, string>
			{
				{ "App:StorageFolder", _createAnImage.BasePath },
				{ "App:ThumbnailTempFolder",_createAnImage.BasePath },
				{ "App:Verbose", "true" }
			};
			// Start using dependency injection
			var builder = new ConfigurationBuilder();  
			// Add random config to dependency injection
			builder.AddInMemoryCollection(dict);
			// build config
			var configuration = builder.Build();
			// inject config as object to a service
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
            
			// Add Background services
			services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
			services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();
            
			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			_appSettings = serviceProvider.GetRequiredService<AppSettings>();
           
			// inject fake exiftool
			_exifTool = new FakeExifTool(_iStorage,_appSettings);
            
			_readmeta = serviceProvider.GetRequiredService<IReadMeta>();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            
			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();
	        
			_iStorage = new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger());

		}
        
		private async Task<FileIndexItem> InsertSearchData(bool delete = false)
		{
			var fileHashCode = new FileHash(_iStorage).GetHashCode(_createAnImage.DbPath).Key;
	        
			if (string.IsNullOrEmpty(_query.GetSubPathByHash(fileHashCode)))
			{
				var isDelete = string.Empty;
				if (delete) isDelete = "!delete!";
				await _query.AddItemAsync(new FileIndexItem
				{
					FileName = _createAnImage.FileName,
					ParentDirectory = "/",
					FileHash = fileHashCode,
					ColorClass = ColorClassParser.Color.Winner, // 1
					Tags = isDelete
				});
			}
			return _query.GetObjectByFilePath(_createAnImage.DbPath);
		}

        
		[TestMethod]
		public async Task ApiController_Delete_API_HappyFlow_Test()
		{
			var createAnImage = await InsertSearchData(true);
			_appSettings.DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase;
			
			// RealFs Storage
			var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger()));
			
			var deleteItem = new DeleteItem(_query,_appSettings,selectorStorage);
			var controller = new DeleteController(deleteItem);

			Console.WriteLine("createAnImage.FilePath");
			Console.WriteLine("@#~ "+ createAnImage.FilePath);

			// create an image
			var createAnImage1 = new CreateAnImage();
			Assert.IsNotNull(createAnImage1);
			
			var actionResult = controller.Delete(createAnImage.FilePath) as JsonResult;
			Assert.AreNotEqual(null,actionResult);
			var jsonCollection = actionResult.Value as List<FileIndexItem>;
			Assert.AreEqual(createAnImage.FilePath,jsonCollection.FirstOrDefault().FilePath);
			
			var createAnImage2 = new CreateAnImage(); //restore afterwards
			Assert.IsNotNull(createAnImage2);
		}

		[TestMethod]
		public async Task ApiController_Delete_API_RemoveNotAllowedFile_Test()
		{
	        
			// re add data
			var createAnImage = await InsertSearchData();
	        Assert.IsNotNull(createAnImage?.FilePath);
	        
			// Clean existing items to avoid errors
			var itemByHash = _query.SingleItem(createAnImage!.FilePath);
			Assert.IsNotNull(itemByHash);
			Assert.IsNotNull(itemByHash.FileIndexItem);

			itemByHash.FileIndexItem.Tags = string.Empty;
			await _query.UpdateItemAsync(itemByHash.FileIndexItem);
	        
			_appSettings.DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase;
	        
			var selectorStorage =
				new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger()));
			
			var deleteItem = new DeleteItem(_query,_appSettings,selectorStorage);
			var controller = new DeleteController(deleteItem);
			
			var notFoundResult = controller.Delete(createAnImage.FilePath) as NotFoundObjectResult;
			Assert.AreEqual(404,notFoundResult?.StatusCode);
			var jsonCollection = notFoundResult.Value as List<FileIndexItem>;

			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,
				jsonCollection?.FirstOrDefault()?.Status);

			await _query.RemoveItemAsync(_query.SingleItem(createAnImage.FilePath).FileIndexItem);
		}
		
		        
		[TestMethod]
		public async Task ApiController_Delete_SourceImageMissingOnDisk_WithFakeExiftool()
		{
			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "345678765434567.jpg",
				ParentDirectory = "/",
				FileHash = "345678765434567"
			});
            
			var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger()));
			var deleteItem = new DeleteItem(_query,_appSettings,selectorStorage);
			var controller = new DeleteController(deleteItem);
			var notFoundResult = controller.Delete("/345678765434567.jpg") as NotFoundObjectResult;
			Assert.AreEqual(404,notFoundResult.StatusCode);

			await _query.RemoveItemAsync(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
		}

	}
}

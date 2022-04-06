using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
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
	public class MetaUpdateControllerTest
	{
		private readonly IQuery _query;
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly IStorage _iStorage;

		public MetaUpdateControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
            
			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase("test1234");
			var options = builderDb.Options;
			var context = new ApplicationDbContext(options);
			_query = new Query(context, new AppSettings(), null, new FakeIWebLogger(),memoryCache);
            
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
           
			// inject fake exifTool
			_exifTool = new FakeExifTool(_iStorage,_appSettings);
            
			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();
	        
			_iStorage = new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger());

		}
        
		private void InsertSearchData(bool delete = false)
		{
			var fileHashCode = new FileHash(_iStorage).GetHashCode(_createAnImage.DbPath).Key;
	        
			if (string.IsNullOrEmpty(_query.GetSubPathByHash(fileHashCode)))
			{
				var isDelete = string.Empty;
				if (delete) isDelete = "!delete!";
				_query.AddItem(new FileIndexItem
				{
					FileName = _createAnImage.FileName,
					ParentDirectory = "/",
					FileHash = fileHashCode,
					ColorClass = ColorClassParser.Color.Winner, // 1
					Tags = isDelete
				});
			}

			_query.GetObjectByFilePath(_createAnImage.DbPath);
		}

		private IServiceScopeFactory NewScopeFactory()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IMetaPreflight, MetaPreflight>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, SelectorStorage>();
			services.AddSingleton<IExifTool, FakeExifTool>();
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		[TestMethod]
		public async Task ApiController_Update_AllDataIncluded_WithFakeExifTool()
		{
			var createAnImage = new CreateAnImage();
			InsertSearchData();
            
			var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger()));
	        
			var metaPreflight = new MetaPreflight(_query,_appSettings,
				selectorStorage,new FakeIWebLogger());
			var metaUpdateService = new MetaUpdateService(_query, _exifTool,
				selectorStorage, new FakeMetaPreflight(),
				new FakeIWebLogger(), new FakeReadMetaSubPathStorage());
			
			var metaReplaceService = new MetaReplaceService(_query,_appSettings,selectorStorage, new FakeIWebLogger());
			
			var controller = new MetaUpdateController(metaPreflight,metaUpdateService, metaReplaceService, _bgTaskQueue, 
				new FakeIWebSocketConnectionsService(), new FakeIWebLogger(), NewScopeFactory());

			var input = new FileIndexItem
			{
				Tags = "test"
			};
			var jsonResult = await controller.UpdateAsync(input, createAnImage.DbPath,false,
				false) as JsonResult;
			if ( jsonResult == null )
			{
				Console.WriteLine("json should not be null");
				throw new NullReferenceException(nameof(jsonResult));
			}
			var fileModel = jsonResult.Value as List<FileIndexItem>;
			//you could not test because exiftool is an external dependency

			if ( fileModel == null ) throw new NullReferenceException(nameof(fileModel));
			Assert.AreNotEqual(null,fileModel.FirstOrDefault()?.Tags);
		}
        
		[TestMethod]
		public async Task ApiController_Update_SourceImageMissingOnDisk_WithFakeExifTool()
		{
			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "ApiController_Update_SourceImageMissingOnDisk_WithFakeExifTool.jpg",
				ParentDirectory = "/",
				FileHash = "ApiController_Update_SourceImageMissingOnDisk_WithFakeExifTool"
			});
			var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger()));

			var metaPreflight = new MetaPreflight(_query,
				_appSettings,selectorStorage,new FakeIWebLogger());
			var metaUpdateService = new MetaUpdateService(_query, _exifTool,
				selectorStorage, new FakeMetaPreflight(),
				new FakeIWebLogger(), new FakeReadMetaSubPathStorage());
			
			var metaReplaceService = new MetaReplaceService(_query,_appSettings,selectorStorage, new FakeIWebLogger());
	        
			var controller = new MetaUpdateController(metaPreflight,metaUpdateService, metaReplaceService, _bgTaskQueue, 
				new FakeIWebSocketConnectionsService(), new FakeIWebLogger(),NewScopeFactory())
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var testElement = new FileIndexItem();
			var notFoundResult = await controller.UpdateAsync(testElement, "/ApiController_Update_SourceImageMissingOnDisk_WithFakeExifTool.jpg",
				false,false) as NotFoundObjectResult;
			if ( notFoundResult == null ) throw new NullReferenceException(nameof(notFoundResult));

			Assert.AreEqual(404,notFoundResult.StatusCode);

			await _query.RemoveItemAsync(_query.SingleItem("/ApiController_Update_SourceImageMissingOnDisk_WithFakeExifTool.jpg").FileIndexItem);
		}

		[TestMethod]
		public async Task Replace_SourceImageMissingOnDisk_WithFakeExifTool()
		{
			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "345678765434567.jpg",
				ParentDirectory = "/",
				FileHash = "345678765434567"
			});
			var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger()));

			var metaPreflight = new MetaPreflight(_query,
				_appSettings,selectorStorage,new FakeIWebLogger());
			var metaUpdateService = new MetaUpdateService(_query, _exifTool,
				selectorStorage, new FakeMetaPreflight(),
				new FakeIWebLogger(), new FakeReadMetaSubPathStorage());
			
			var metaReplaceService = new MetaReplaceService(_query,_appSettings,selectorStorage, new FakeIWebLogger());
	        
			var controller = new MetaUpdateController(metaPreflight,metaUpdateService, metaReplaceService, _bgTaskQueue, 
				new FakeIWebSocketConnectionsService(), new FakeIWebLogger(),NewScopeFactory())
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var notFoundResult = await controller.Replace( "/345678765434567.jpg", "test", "search", 
				string.Empty) as NotFoundObjectResult;
			if ( notFoundResult == null ) throw new NullReferenceException(nameof(notFoundResult));

			Assert.AreEqual(404,notFoundResult.StatusCode);

			await _query.RemoveItemAsync(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
		}
        
		[TestMethod]
		public async Task Replace_AllDataIncluded_WithFakeExifTool()
		{
			var item = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "test09.jpg",
				ParentDirectory = "/",
				Tags = "7test"
			});
	        
			var selectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test09.jpg"}));
	        
			var metaPreflight = new MetaPreflight(_query,
				_appSettings,selectorStorage,new FakeIWebLogger());
			var metaUpdateService = new MetaUpdateService(_query, _exifTool,
				selectorStorage, new FakeMetaPreflight(),
				new FakeIWebLogger(), new FakeReadMetaSubPathStorage());
			
			var metaReplaceService = new MetaReplaceService(_query,_appSettings,selectorStorage, new FakeIWebLogger());
			var controller = new MetaUpdateController(metaPreflight,metaUpdateService, metaReplaceService, _bgTaskQueue, 
				new FakeIWebSocketConnectionsService(), new FakeIWebLogger(),NewScopeFactory());

			var jsonResult =  await controller.Replace("/test09.jpg","Tags", "test", 
				string.Empty) as JsonResult;
			if ( jsonResult == null ) throw new NullReferenceException(nameof(jsonResult));
			var fileModel = jsonResult.Value as List<FileIndexItem>;
			if ( fileModel == null ) throw new NullReferenceException(nameof(fileModel));

			Assert.AreNotEqual(null,fileModel.FirstOrDefault()?.Tags);
			Assert.AreEqual("7", fileModel.FirstOrDefault()?.Tags);

			await _query.RemoveItemAsync(item);
		}

		[TestMethod]
		public async Task UpdateAsync_ShouldTriggerBackgroundService()
		{
			var fakeFakeIWebSocketConnectionsService =
				new FakeIWebSocketConnectionsService();
			
			var controller = new MetaUpdateController(
				new FakeMetaPreflight(),
				new FakeIMetaUpdateService(), 
				new FakeIMetaReplaceService(), 
				new FakeIUpdateBackgroundTaskQueue(), 
				fakeFakeIWebSocketConnectionsService, 
				new FakeIWebLogger(),
				null);

			await controller.UpdateAsync(new FileIndexItem{ Status =  FileIndexItem.ExifStatus.Ok}, "/test09.jpg",
				true);

			Assert.AreEqual(1,fakeFakeIWebSocketConnectionsService
				.FakeSendToAllAsync.Count(p => !p.StartsWith("[system]")));
		}
        
		[TestMethod]
		public void Replace_ShouldTriggerBackgroundService_Ok()
		{
			var fakeFakeIWebSocketConnectionsService =
				new FakeIWebSocketConnectionsService();
			var controller = new MetaUpdateController(new FakeMetaPreflight(),new FakeIMetaUpdateService(), 
				new FakeIMetaReplaceService(new List<FileIndexItem>{new FileIndexItem("/test09.jpg")
				{
					Status = FileIndexItem.ExifStatus.Ok
				}}), 
				new FakeIUpdateBackgroundTaskQueue(), fakeFakeIWebSocketConnectionsService, new FakeIWebLogger(),NewScopeFactory());

			controller.Replace("/test09.jpg", "tags", "test", "");

			Assert.AreEqual(1, fakeFakeIWebSocketConnectionsService.FakeSendToAllAsync.Count);
		}
        
		[TestMethod]
		public void Replace_ShouldTriggerBackgroundService_Fail()
		{
			var fakeFakeIWebSocketConnectionsService =
				new FakeIWebSocketConnectionsService();
			var controller = new MetaUpdateController(new FakeMetaPreflight(),new FakeIMetaUpdateService(), 
				new FakeIMetaReplaceService(new List<FileIndexItem>{new FileIndexItem("/test09.jpg")
				{
					Status = FileIndexItem.ExifStatus.OperationNotSupported
				}}), 
				new FakeIUpdateBackgroundTaskQueue(), fakeFakeIWebSocketConnectionsService, new FakeIWebLogger(),NewScopeFactory());

			controller.Replace("/test09.jpg", "tags", "test", "");

			Assert.AreEqual(0, fakeFakeIWebSocketConnectionsService.FakeSendToAllAsync.Count);
		}
	}
}

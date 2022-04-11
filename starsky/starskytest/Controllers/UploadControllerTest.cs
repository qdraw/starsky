using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.import.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.SyncServices;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public class UploadControllerTest
	{
		private readonly IQuery _query;
		private readonly IStorage _iStorage;
		private readonly AppSettings _appSettings;
		private readonly Import _import;
		private readonly IMemoryCache _memoryCache;

		public UploadControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase(nameof(ExportControllerTest));
			var options = builderDb.Options;
			var context = new ApplicationDbContext(options);
			var scopeFactory = provider.GetService<IServiceScopeFactory>();
			var services = new ServiceCollection();

			// Fake the readMeta output
			services.AddSingleton<IReadMeta, FakeReadMeta>();

			// Inject Config helper
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			// random config
			var createAnImage = new CreateAnImage();
			_appSettings = new AppSettings { 
				TempFolder = createAnImage.BasePath
			};
			_query = new Query(context, _appSettings, scopeFactory, new FakeIWebLogger(), _memoryCache);

			_iStorage = new FakeIStorage(new List<string>{"/","/test"}, 
				new List<string>{createAnImage.DbPath}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
			var readMeta = new ReadMeta(_iStorage,_appSettings);
                        
			var selectorStorage = new FakeSelectorStorage(_iStorage);

			_import = new Import(selectorStorage, _appSettings, new FakeIImportQuery(),
				new FakeExifTool(_iStorage,_appSettings), _query, new ConsoleWrapper(), 
				new FakeIMetaExifThumbnailService(), new FakeIWebLogger(), _memoryCache);

			// Start using dependency injection
			var builder = new ConfigurationBuilder();
			// Add random config to dependency injection
			// build config
			var configuration = builder.Build();
			// inject config as object to a service

			// Add Background services
			services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
			services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			
			serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		/// <summary>
		///  Add the file in the underlying request object.
		/// </summary>
		/// <returns>Controller Context with file</returns>
		private ControllerContext RequestWithFile(byte[] bytes = null)
		{
			// ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
			if ( bytes == null ) bytes = CreateAnImage.Bytes;
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers.Add("Content-Type", "application/octet-stream");
			httpContext.Request.Body = new MemoryStream(bytes);
	        
			var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
			return new ControllerContext(actionContext);
		}

		[TestMethod]
		public async Task UploadToFolder_NoToHeader_BadRequest()
		{
			var controller =
				new UploadController(_import, _appSettings, 
					new FakeSelectorStorage(new FakeIStorage()), _query, 
					new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
					new FakeIMetaExifThumbnailService())
				{
					ControllerContext = {HttpContext = new DefaultHttpContext()}
				};
			
			var actionResult = await controller.UploadToFolder()as BadRequestObjectResult;
			
			Assert.AreEqual(400,actionResult.StatusCode);
		}
	
		[TestMethod]
		public async Task UploadToFolder_DefaultFlow()
		{
			var controller = new UploadController(_import, _appSettings,  
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = RequestWithFile(),
			};

			var toPlaceSubPath = "/yes01.jpg";
			
			controller.ControllerContext.HttpContext.Request.Headers["to"] = toPlaceSubPath; //Set header

			var actionResult = await controller.UploadToFolder()  as JsonResult;
			var list = actionResult.Value as List<ImportIndexItem>;

			Assert.AreEqual( ImportStatus.Ok, list.FirstOrDefault().Status);

			var fileSystemResult = _iStorage.ExistFile(toPlaceSubPath);
			Assert.IsTrue(fileSystemResult);

			var queryResult = _query.SingleItem(toPlaceSubPath);
			Assert.AreEqual("Sony",queryResult.FileIndexItem.Make);

			await _query.RemoveItemAsync(queryResult.FileIndexItem);
		}
		
		[TestMethod]
		public async Task UploadToFolder_DefaultFlow_ColorClass()
		{
			var controller = new UploadController(_import, _appSettings,  
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = RequestWithFile(CreateAnImageColorClass.Bytes),
			};

			var toPlaceSubPath = "/color-class01.jpg";
			
			controller.ControllerContext.HttpContext.Request.Headers["to"] = toPlaceSubPath; //Set header

			var actionResult = await controller.UploadToFolder()  as JsonResult;
			var list = actionResult.Value as List<ImportIndexItem>;

			Assert.AreEqual( ImportStatus.Ok, list.FirstOrDefault().Status);

			var fileSystemResult = _iStorage.ExistFile(toPlaceSubPath);
			Assert.IsTrue(fileSystemResult);

			var queryResult = _query.SingleItem(toPlaceSubPath);
			
			Assert.AreEqual("Sony",queryResult.FileIndexItem.Make);
			Assert.AreEqual(ColorClassParser.Color.Winner,queryResult.FileIndexItem.ColorClass);

			await _query.RemoveItemAsync(queryResult.FileIndexItem);
		}
		
		[TestMethod]
		public async Task UploadToFolder_DefaultFlow_ShouldNotOverWriteDatabase()
		{
			var controller = new UploadController(_import, _appSettings,  
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = RequestWithFile(),
			};

			var toPlaceSubPath = "/duplicate_upload/yes01.jpg";
			var toPlaceFolder = "/duplicate_upload";

			// add to db 
			await _query.AddItemAsync(new FileIndexItem(toPlaceSubPath));
			
			_iStorage.CreateDirectory(toPlaceFolder);
			
			controller.ControllerContext.HttpContext.Request.Headers["to"] = toPlaceSubPath; //Set header

			var actionResult = await controller.UploadToFolder() as JsonResult;
			if ( actionResult == null ) throw new ArgumentNullException(nameof(actionResult),
				"actionResult should not be null");
			var list = actionResult.Value as List<ImportIndexItem>;
			if ( list == null ) throw new ArgumentNullException(nameof(list),
				"result should not be null");

			Assert.AreEqual( ImportStatus.Ok, list[0].Status);

			var fileSystemResult = _iStorage.ExistFile(toPlaceSubPath);
			Assert.IsTrue(fileSystemResult);

			var getAllFiles = await _query.GetAllFilesAsync(toPlaceFolder);

			// Should not duplicate
			Assert.AreEqual(1,getAllFiles.Count);
			
			var queryResult = _query.SingleItem(toPlaceSubPath);
			Assert.AreEqual("Sony",queryResult.FileIndexItem.Make);

			await _query.RemoveItemAsync(queryResult.FileIndexItem);
		}
		
		[TestMethod]
		public async Task UploadToFolder_SidecarListShouldBeUpdated()
		{
			var controller = new UploadController(_import, _appSettings,  
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = RequestWithFile(),
			};

			var toPlaceSubPath = "/test_sidecar.dng";
			var toPlaceXmp = "/test_sidecar.xmp";

			await _iStorage.WriteStreamAsync(new MemoryStream(new byte[1]), toPlaceXmp);
			
			controller.ControllerContext.HttpContext.Request.Headers["to"] = toPlaceSubPath; //Set header

			await controller.UploadToFolder();

			var queryResult = _query.SingleItem(toPlaceSubPath);

			var sidecarExtList = queryResult.FileIndexItem.SidecarExtensionsList.ToList();
			Assert.AreEqual(1,sidecarExtList.Count);
			Assert.AreEqual("xmp",sidecarExtList[0]);

			await _query.RemoveItemAsync(queryResult.FileIndexItem);
		}
		
		[TestMethod]
		public async Task UploadToFolder_NotFound()
		{
			var controller =
				new UploadController(_import, _appSettings,  
					new FakeSelectorStorage(_iStorage), _query, 
					new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
					new FakeIMetaExifThumbnailService())
				{
					ControllerContext = RequestWithFile(),
				};
			controller.ControllerContext.HttpContext.Request.Headers["to"] = "/not-found"; //Set header

			var actionResult = await controller.UploadToFolder()as NotFoundObjectResult;
			
			Assert.AreEqual(404,actionResult.StatusCode);
		}
		
		[TestMethod]
		public async Task UploadToFolder_UnknownFailFlow()
		{
			var controller = new UploadController(_import, _appSettings, 
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = RequestWithFile(),
			};
			
			controller.ControllerContext.HttpContext.Request.Headers["to"] = "/"; //Set header

			var actionResult = await controller.UploadToFolder()  as JsonResult;
			var list = actionResult.Value as List<ImportIndexItem>;

			Assert.AreEqual( ImportStatus.FileError, list.FirstOrDefault().Status);
		}

		[TestMethod]
		public void GetParentDirectoryFromRequestHeader_InputToAsSubPath()
		{
			var controllerContext = RequestWithFile();
			controllerContext.HttpContext.Request.Headers.Add("to", "/test.jpg");
			
			var controller = new UploadController(_import, _appSettings, 
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = controllerContext
			};

			var result = controller.GetParentDirectoryFromRequestHeader();
			Assert.AreEqual("/", result);
		}
		
		[TestMethod]
		public void GetParentDirectoryFromRequestHeader_InputToAsSubPath_TestFolder()
		{
			var controllerContext = RequestWithFile();
			controllerContext.HttpContext.Request.Headers.Add("to", "/test/test.jpg");
			
			var controller = new UploadController(_import, _appSettings, 
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = controllerContext
			};

			var result = controller.GetParentDirectoryFromRequestHeader();
			Assert.AreEqual("/test", result);
		}
	
		[TestMethod]
		public void GetParentDirectoryFromRequestHeader_InputToAsSubPath_TestDirectFolder()
		{
			var controllerContext = RequestWithFile();
			controllerContext.HttpContext.Request.Headers.Add("to", "/test/");
			
			var controller = new UploadController(_import, _appSettings, 
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = controllerContext
			};

			var result = controller.GetParentDirectoryFromRequestHeader();
			Assert.AreEqual("/test", result);
		}
		
		[TestMethod]
		public void GetParentDirectoryFromRequestHeader_InputToAsSubPath_NonExistFolder()
		{
			var controllerContext = RequestWithFile();
			controllerContext.HttpContext.Request.Headers.Add("to", "/non-exist/test.jpg");

			var controller =
				new UploadController(_import, _appSettings, 
					new FakeSelectorStorage(_iStorage), _query, 
					new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
					new FakeIMetaExifThumbnailService())
				{
					ControllerContext = controllerContext
				};
			
			var result = controller.GetParentDirectoryFromRequestHeader();
			Assert.IsNull(result);
		}
		
		/// <summary>
		///  Add the file in the underlying request object.
		/// </summary>
		/// <returns>Controller Context with file</returns>
		private ControllerContext RequestWithSidecar()
		{
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers.Add("Content-Type", "application/octet-stream");
			httpContext.Request.Body = new MemoryStream(CreateAnXmp.Bytes);
	        
			var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
			return new ControllerContext(actionContext);
		}
		
		[TestMethod]
		public async Task UploadToFolderSidecarFile_DefaultFlow()
		{
			var controller = new UploadController(_import, _appSettings, 
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = RequestWithSidecar(),
			};

			var toPlaceSubPath = "/yes01.xmp";
			controller.ControllerContext.HttpContext.Request.Headers["to"] = toPlaceSubPath; //Set header

			var actionResult = await controller.UploadToFolderSidecarFile()  as JsonResult;
			var list = actionResult.Value as List<string>;

			Assert.AreEqual(toPlaceSubPath, list.FirstOrDefault());
		}
		
		[TestMethod]
		public async Task UploadToFolderSidecarFile_UpdateMainItemWithSidecarRef()
		{
			// it should add a reference to the main item
			var controller = new UploadController(_import, new AppSettings{UseDiskWatcher = false}, 
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = RequestWithSidecar(),
			};

			var dngSubPath = "/UploadToFolderSidecarFile.dng";
			await _query.AddItemAsync(
				new FileIndexItem(dngSubPath));
			
			var toPlaceSubPath = "/UploadToFolderSidecarFile.xmp";
			controller.ControllerContext.HttpContext.Request.Headers["to"] = toPlaceSubPath; //Set header

			await controller.UploadToFolderSidecarFile();

			var queryResult = await _query.GetObjectByFilePathAsync(dngSubPath);
			var sidecarExtList = queryResult.SidecarExtensionsList.ToList();
			Assert.AreEqual(1,sidecarExtList.Count);
			Assert.AreEqual("xmp",sidecarExtList[0]);
		}
				
		[TestMethod]
		public async Task UploadToFolderSidecarFile_NoXml_SoIgnore()
		{
			var controller = new UploadController(_import, _appSettings, 
				new FakeSelectorStorage(_iStorage), _query, 
				new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
				new FakeIMetaExifThumbnailService())
			{
				ControllerContext = RequestWithFile() // < - - - - - - this is not an xml
			};

			var toPlaceSubPath = "/yes01.xmp";
			controller.ControllerContext.HttpContext.Request.Headers["to"] = toPlaceSubPath; //Set header

			var actionResult = await controller.UploadToFolderSidecarFile()  as JsonResult;
			var list = actionResult.Value as List<string>;

			Assert.AreEqual(0, list.Count);
		}
		
		[TestMethod]
		public async Task UploadToFolderSidecarFile_NotFound()
		{
			var controller =
				new UploadController(_import, _appSettings,  
					new FakeSelectorStorage(_iStorage), _query, 
					new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
					new FakeIMetaExifThumbnailService())
				{
					ControllerContext = RequestWithFile(),
				};
			controller.ControllerContext.HttpContext.Request.Headers["to"] = "/not-found"; //Set header

			var actionResult = await controller.UploadToFolderSidecarFile()as NotFoundObjectResult;
			
			Assert.AreEqual(404,actionResult.StatusCode);
		}
		
		[TestMethod]
		public async Task UploadToFolderSidecarFile_NoToHeader_BadRequest()
		{
			var controller =
				new UploadController(_import, _appSettings, 
					new FakeSelectorStorage(new FakeIStorage()), _query, 
					new FakeIRealtimeConnectionsService(), new FakeIWebLogger(),
					new FakeIMetaExifThumbnailService())
				{
					ControllerContext = {HttpContext = new DefaultHttpContext()}
				};
			
			var actionResult = await controller.UploadToFolderSidecarFile()as BadRequestObjectResult;
			
			Assert.AreEqual(400,actionResult.StatusCode);
		}
	}
}

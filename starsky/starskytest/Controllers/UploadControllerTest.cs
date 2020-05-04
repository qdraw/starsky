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
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starskycore.Services;
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
		private SyncService _iSync;
		private readonly IReadMeta _readMeta;
		private readonly Import _import;

		public UploadControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase(nameof(ExportControllerTest));
			var options = builderDb.Options;
			var context = new ApplicationDbContext(options);
			_query = new Query(context, memoryCache);

			var services = new ServiceCollection();

			// Fake the readmeta output
			services.AddSingleton<IReadMeta, FakeReadMeta>();

			// Inject Config helper
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			// random config
			var createAnImage = new CreateAnImage();
			_appSettings = new AppSettings { 
				TempFolder = createAnImage.BasePath
			};

			_iStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{createAnImage.DbPath}, 
				new List<byte[]>{CreateAnImage.Bytes});
			
	        _readMeta = new ReadMeta(_iStorage,_appSettings);
                        
	        var selectorStorage = new FakeSelectorStorage(_iStorage);
	        _iSync = new SyncService(_query,_appSettings, selectorStorage);

			_import = new Import(selectorStorage, _appSettings, new FakeIImportQuery(null),
			 new FakeExifTool(_iStorage,_appSettings), _query, new ConsoleWrapper());

			// Start using dependency injection
			var builder = new ConfigurationBuilder();
			// Add random config to dependency injection
			// build config
			var configuration = builder.Build();
			// inject config as object to a service

			// Add Background services
			services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			
			_readMeta = serviceProvider.GetRequiredService<IReadMeta>();
			serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		/// <summary>
		///  Add the file in the underlying request object.
		/// </summary>
		/// <returns>Controller Context with file</returns>
		private ControllerContext RequestWithFile()
		{
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers.Add("Content-Type", "application/octet-stream");
			httpContext.Request.Body = new MemoryStream(CreateAnImage.Bytes);
	        
			var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
			return new ControllerContext(actionContext);
		}

		[TestMethod]
		public async Task UploadToFolder_NoToHeader_BadRequest()
		{
			var controller =
				new UploadController(_import, _appSettings, _iSync, new FakeSelectorStorage(new FakeIStorage()), _query)
				{
					ControllerContext = {HttpContext = new DefaultHttpContext()}
				};
			
			var actionResult = await controller.UploadToFolder()as BadRequestObjectResult;
			
			Assert.AreEqual(400,actionResult.StatusCode);
		}
	
		[TestMethod]
		public async Task UploadToFolder_DefaultFlow()
		{
			var controller = new UploadController(_import, _appSettings, _iSync,  new FakeSelectorStorage(_iStorage), _query)
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
		}
		
		[TestMethod]
		public async Task UploadToFolder_NotFound()
		{
			var controller =
				new UploadController(_import, _appSettings,  _iSync, new FakeSelectorStorage(_iStorage), _query)
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
			var controller = new UploadController(_import, _appSettings, _iSync, new FakeSelectorStorage(_iStorage), _query)
			{
				ControllerContext = RequestWithFile(),
			};
			
			controller.ControllerContext.HttpContext.Request.Headers["to"] = "/"; //Set header

			var actionResult = await controller.UploadToFolder()  as JsonResult;
			var list = actionResult.Value as List<ImportIndexItem>;

			Assert.AreEqual( ImportStatus.FileError, list.FirstOrDefault().Status);
		}

		
		
//		[TestMethod]
		public async Task UploadToFolder_StreamFileHasFailed_BadRequest()
		{
			var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
			httpContext.Request.Headers["to"] = "/"; //Set header
			
			var controller =
				new UploadController(_import, _appSettings,  _iSync, new FakeSelectorStorage(_iStorage), _query)
				{
					ControllerContext =
					{
						HttpContext = httpContext
					}
				};
			
			var actionResult = await controller.UploadToFolder() as BadRequestObjectResult;
			
			Assert.AreEqual(400,actionResult.StatusCode);
		}

	}
}

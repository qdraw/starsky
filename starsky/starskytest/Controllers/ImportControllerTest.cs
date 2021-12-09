using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.http.Services;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public class ImportControllerTest
	{
		private readonly IImport _import;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly AppSettings _appSettings;

		public ImportControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();

			var memoryCache = provider.GetService<IMemoryCache>();

			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("test");
			var options = builder.Options;
			var context = new ApplicationDbContext(options);

			var services = new ServiceCollection();

			_appSettings = new AppSettings();

			// Add Background services
			services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
			services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();

			var serviceProvider = services.BuildServiceProvider();

			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();

			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			_import = new FakeIImport(new FakeSelectorStorage(new FakeIStorage()));
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

			var actionContext = new ActionContext(httpContext, new RouteData(),
				new ControllerActionDescriptor());
			return new ControllerContext(actionContext);
		}

		[TestMethod]
		public async Task ImportController_WrongInputFlow()
		{
			var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());

			var importController = new ImportController(new FakeIImport(fakeStorageSelector),
				_appSettings,
				_bgTaskQueue, null, fakeStorageSelector, _scopeFactory, new FakeIWebLogger())
			{
				ControllerContext = RequestWithFile(),
			};

			var actionResult = await importController.IndexPost() as JsonResult;
			var list = actionResult.Value as List<ImportIndexItem>;

			Assert.AreEqual(ImportStatus.FileError, list.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task ImportPostBackgroundTask_NotFound()
		{

			var services = new ServiceCollection();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<IImportQuery, FakeIImportQuery>();
			services.AddSingleton<IExifTool, FakeExifTool>();
			services.AddSingleton<IQuery, FakeIQuery>();
			services.AddSingleton<IImport, FakeIImport>();
			services.AddSingleton<IConsole, FakeConsoleWrapper>();
			services.AddSingleton<IMetaExifThumbnailService, FakeIMetaExifThumbnailService>();

			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			var importController = new ImportController(null, new AppSettings(),
				null, null, new FakeSelectorStorage(),
				scopeFactory, new FakeIWebLogger());
			
			var result = await importController.ImportPostBackgroundTask(
				new List<string>{"/test"}, new ImportSettingsModel());

			Assert.AreEqual(1,result.Count );
			Assert.AreEqual(ImportStatus.NotFound, result[0].Status);
		}

		[TestMethod]
		public async Task ImportPostBackgroundTask_NotFound_Logger_Contain1()
		{
			var services = new ServiceCollection();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<IImportQuery, FakeIImportQuery>();
			services.AddSingleton<IExifTool, FakeExifTool>();
			services.AddSingleton<IQuery, FakeIQuery>();
			services.AddSingleton<IImport, FakeIImport>();
			services.AddSingleton<IConsole, FakeConsoleWrapper>();
			services.AddSingleton<IMetaExifThumbnailService, FakeIMetaExifThumbnailService>();

			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			var logger = new FakeIWebLogger();
			var importController = new ImportController(null, new AppSettings(),
				null, null, new FakeSelectorStorage(),
				scopeFactory, logger);
			
			await importController.ImportPostBackgroundTask(
				new List<string>{"/test"}, new ImportSettingsModel(), true);

			Assert.AreEqual(1,logger.TrackedInformation.Count );
		}

		[TestMethod]
		public async Task FromUrl_PathInjection()
		{
			var importController = new ImportController(_import, _appSettings,
				_bgTaskQueue, null, new FakeSelectorStorage(new FakeIStorage()), _scopeFactory, new FakeIWebLogger())
			{
				ControllerContext = RequestWithFile(),
			};
			var actionResult =
				await importController.FromUrl("", "../../path-injection.dll", null) as
					BadRequestResult;
			Assert.AreEqual(400, actionResult.StatusCode);
		}

		[TestMethod]
		public async Task FromUrl_RequestFromWhiteListedDomain_NotFound()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();

			var httpClientHelper = new HttpClientHelper(httpProvider,
				serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FakeIWebLogger());

			var importController = new ImportController(_import, _appSettings,
				_bgTaskQueue, httpClientHelper, new FakeSelectorStorage(new FakeIStorage()),
				_scopeFactory, new FakeIWebLogger()) {ControllerContext = RequestWithFile(),};
			// download.geoNames is in the FakeHttpMessageHandler always a 404
			var actionResult =
				await importController.FromUrl("https://download.geonames.org", "example.tiff",
					null) as NotFoundObjectResult;
			Assert.AreEqual(404, actionResult.StatusCode);
		}

		[TestMethod]
		public async Task FromUrl_RequestFromWhiteListedDomain_Ok()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var storageProvider = serviceProvider.GetRequiredService<IStorage>();

			var httpClientHelper = new HttpClientHelper(httpProvider,
				serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FakeIWebLogger());

			var importController = new ImportController(
				new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings,
				_bgTaskQueue, httpClientHelper, new FakeSelectorStorage(storageProvider),
				_scopeFactory, new FakeIWebLogger()) {ControllerContext = RequestWithFile(),};

			var actionResult =
				await importController.FromUrl("https://qdraw.nl", "example_image.tiff", null) as
					JsonResult;
			var list = actionResult.Value as List<ImportIndexItem>;

			Assert.IsTrue(list.FirstOrDefault().FilePath.Contains("example_image.tiff"));
		}

		[TestMethod]
		public async Task Import_Thumbnail_Ok()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var storageProvider = serviceProvider.GetRequiredService<IStorage>();
			var importController = new ImportController(
				new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings,
				_bgTaskQueue, null, new FakeSelectorStorage(storageProvider), 
				_scopeFactory, new FakeIWebLogger())
			{
				ControllerContext = RequestWithFile(),
			};
			importController.Request.Headers["filename"] =
				"01234567890123456789123456.jpg"; // len() 26

			var actionResult = await importController.Thumbnail() as JsonResult;
			var list = actionResult.Value as List<string>;
			var existFileInTempFolder =
				storageProvider.ExistFile(
					_appSettings.TempFolder + "01234567890123456789123456.jpg");

			Assert.AreEqual("01234567890123456789123456", list.FirstOrDefault());
			Assert.IsFalse(existFileInTempFolder);
		}
		
		[TestMethod]
		public async Task Import_Thumbnail_Ok_SmallSize()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var storageProvider = serviceProvider.GetRequiredService<IStorage>();
			var importController = new ImportController(
				new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings,
				_bgTaskQueue, null, new FakeSelectorStorage(storageProvider), 
				_scopeFactory, new FakeIWebLogger())
			{
				ControllerContext = RequestWithFile(),
			};
			importController.Request.Headers["filename"] =
				"01234567890123456789123456@300.jpg"; // len() 26

			var actionResult = await importController.Thumbnail() as JsonResult;
			var list = actionResult.Value as List<string>;
			var existFileInTempFolder =
				storageProvider.ExistFile(
					_appSettings.TempFolder + "01234567890123456789123456@300.jpg");

			Assert.AreEqual("01234567890123456789123456@300", list.FirstOrDefault());
			Assert.IsFalse(existFileInTempFolder);
		}

		[TestMethod]
		public async Task Import_Thumbnail_WrongInputName()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var storageProvider = serviceProvider.GetRequiredService<IStorage>();

			var importController = new ImportController(
				new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings,
				_bgTaskQueue, null, new FakeSelectorStorage(storageProvider),
				_scopeFactory, new FakeIWebLogger())
			{
				ControllerContext = RequestWithFile(),
			};
			importController.Request.Headers["filename"] = "123.jpg"; // len() 3

			var actionResult = await importController.Thumbnail() as JsonResult;
			var list = actionResult.Value as List<string>;

			Assert.AreEqual(0, list.Count);
		}
	}
}

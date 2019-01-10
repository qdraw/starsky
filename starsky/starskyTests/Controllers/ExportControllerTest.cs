using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using starsky.Controllers;
using starsky.Data;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;
using starskytests.FakeMocks;
using starskytests.Models;
using starskytests.Services;

namespace starskytests.Controllers
{
	[TestClass]
	public class ExportControllerTest
	{
		private readonly IQuery _query;
		private readonly IExiftool _exiftool;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly ApplicationDbContext _context;
		private readonly IReadMeta _readmeta;
		private readonly IServiceScopeFactory _scopeFactory;

		public ExportControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase("test1234");
			var options = builderDb.Options;
			_context = new ApplicationDbContext(options);
			_query = new Query(_context, memoryCache);

			// Inject Fake Exiftool; dependency injection
			var services = new ServiceCollection();
			services.AddSingleton<IExiftool, FakeExiftool>();

			// Fake the readmeta output
			services.AddSingleton<IReadMeta, FakeReadMeta>();

			// Inject Config helper
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			// random config
			_createAnImage = new CreateAnImage();
			var dict = new Dictionary<string, string>
			{
				{"App:StorageFolder", _createAnImage.BasePath},
				{"App:ThumbnailTempFolder", _createAnImage.BasePath},
				{"App:Verbose", "true"}
			};
			// Start using dependency injection
			var builder = new ConfigurationBuilder();
			// Add random config to dependency injection
			builder.AddInMemoryCollection(dict);
			// build config
			var configuration = builder.Build();
			// inject config as object to a service
			services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));

			// Add Background services
			services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			_appSettings = serviceProvider.GetRequiredService<AppSettings>();

			// inject fake exiftool
			_exiftool = serviceProvider.GetRequiredService<IExiftool>();

			_readmeta = serviceProvider.GetRequiredService<IReadMeta>();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();


			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
			;
		}

		private FileIndexItem InsertSearchData(bool delete = false)
		{

			var fileHashCode = FileHash.GetHashCode(_createAnImage.FullFilePath);
			if ( string.IsNullOrEmpty(_query.GetItemByHash(fileHashCode)) )
			{
				var isDelete = string.Empty;
				if ( delete ) isDelete = "!delete!";
				_query.AddItem(new FileIndexItem
				{
					FileName = _createAnImage.FileName,
					ParentDirectory = "/",
					FileHash = fileHashCode,
					ColorClass = FileIndexItem.Color.Winner, // 1
					Tags = isDelete
				});
			}
			return _query.GetObjectByFilePath(_createAnImage.DbPath);
		}

		[TestMethod]
		public void ExportController_CreateZipNotFound()
		{
			var controller = new ExportController(_query, _exiftool, _appSettings, _bgTaskQueue, _readmeta);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.CreateZip("/fail", true, false) as NotFoundObjectResult;
			Assert.AreEqual(404,actionResult.StatusCode);
		}

		[TestMethod]
		public void ExportController_TestZipping()
		{
			var createAnImage = InsertSearchData(true);
			_appSettings.DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase;
			var controller = new ExportController(_query, _exiftool, _appSettings, _bgTaskQueue, _readmeta);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			// to avoid skip of adding zip
			var zipFileFullPath = Path.Join(_createAnImage.BasePath, zipHash + ".zip");
			Files.DeleteFile(zipFileFullPath);

			var actionResult = controller.CreateZip(createAnImage.FilePath,true,false) as JsonResult;
			Assert.AreNotEqual(actionResult, null);
			var zipHash = actionResult.Value as string;

			Assert.AreEqual(zipHash.Contains("SR"),true);


			var actionResult2 = controller.Zip(zipHash) as JsonResult;
			if ( (string) actionResult2.Value == "Not ready" || ( string ) actionResult2.Value == "OK" )
			{
				throw new Exception(actionResult2.StatusCode.ToString());
			}

			// There is no check due async background process

		}

		[TestMethod]
		public void ExportController_ZipNotFound()
		{
			var controller = new ExportController(_query, _exiftool, _appSettings, _bgTaskQueue, _readmeta);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.Zip("____fail", true) as NotFoundObjectResult;
			Assert.AreEqual(404, actionResult.StatusCode);
		}


	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskycore.Data;
using starsky.Models;
using starsky.ViewModels;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskytests.FakeCreateAn;
using starskytests.FakeMocks;
using starskytests.Models;
using Query = starskycore.Services.Query;

namespace starskytests.Controllers
{
	[TestClass]
	public class SyncControllerTest
	{
		private readonly IQuery _query;
		private readonly IExiftool _exiftool;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly ApplicationDbContext _context;
		private readonly IReadMeta _readmeta;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ISync _isync;

		public SyncControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase("SyncControllerTest");
			var options = builderDb.Options;
			_context = new ApplicationDbContext(options);
			_query = new Query(_context, memoryCache);

			// Inject Fake Exiftool; dependency injection
			var services = new ServiceCollection();
			services.AddSingleton<IExiftool, FakeExiftool>();

			// Fake the readmeta output
			services.AddSingleton<IReadMeta, FakeReadMeta>();

			// Fake ISync
			services.AddSingleton<ISync, FakeISync>();


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

			_isync = serviceProvider.GetRequiredService<ISync>();

			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
			;
		}

		private FileIndexItem InsertSearchData()
		{

			var fileHashCode = FileHash.GetHashCode(_createAnImage.FullFilePath);
			if ( string.IsNullOrEmpty(_query.GetItemByHash(fileHashCode)) )
			{
				_query.AddItem(new FileIndexItem
				{
					FileName = "/",
					ParentDirectory = "/",
					IsDirectory = true
				});

				_query.AddItem(new FileIndexItem
				{
					FileName = _createAnImage.FileName,
					ParentDirectory = "/",
					FileHash = fileHashCode,
					ColorClass = FileIndexItem.Color.Winner, // 1
				});
			}

			return _query.GetObjectByFilePath(_createAnImage.DbPath);
		}

		[TestMethod]
		public void SyncControllerTest_Folder()
		{
			InsertSearchData();

			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			var controller = new SyncController(_isync, _bgTaskQueue, _query, _appSettings);
			controller.ControllerContext = context;

			var result = controller.SyncIndex("/") as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			var path = list.FirstOrDefault(p => p.FilePath == _createAnImage.DbPath).FilePath;
			Assert.AreEqual(_createAnImage.DbPath, path);
		}

		[TestMethod]
		public void SyncControllerTest_OneFile()
		{
			InsertSearchData();

			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			var controller = new SyncController(_isync, _bgTaskQueue, _query, _appSettings);
			controller.ControllerContext = context;

			var result = controller.SyncIndex(_createAnImage.DbPath) as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			var path = list.FirstOrDefault(p => p.FilePath == _createAnImage.DbPath).FilePath;

			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(_createAnImage.DbPath, path);
		}

		[TestMethod]
		public void SyncControllerTest_DeletedFile()
		{

			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			var controller = new SyncController(_isync, _bgTaskQueue, _query, _appSettings);
			controller.ControllerContext = context;

			var result = controller.SyncIndex("/404") as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, list[0].Status);
			
		}
	}
}

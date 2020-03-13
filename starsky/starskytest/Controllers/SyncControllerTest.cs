using System;
using System.Collections.Generic;
using System.Linq;
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
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;
using Query = starskycore.Services.Query;

namespace starskytest.Controllers
{
	[TestClass]
	public class SyncControllerTest
	{
		private readonly IQuery _query;
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly ApplicationDbContext _context;
		private readonly IReadMeta _readmeta;
		private readonly IServiceScopeFactory _scopeFactory;
		private IStorage _iStorage;
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
			services.AddSingleton<IExifTool, FakeExifTool>();

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
			_exifTool = new FakeExifTool(new FakeIStorage(),_appSettings );

			_readmeta = serviceProvider.GetRequiredService<IReadMeta>();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			_isync = serviceProvider.GetRequiredService<ISync>();

			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
			
		}

		private FileIndexItem InsertSearchData()
		{
			_iStorage = new FakeIStorage(new List<string> { "/" }, new List<string> { _createAnImage.DbPath });
			var fileHashCode = new FileHash(_iStorage).GetHashCode(_createAnImage.DbPath);
			
			if ( string.IsNullOrEmpty(_query.GetSubPathByHash(fileHashCode)) )
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

			var newImage = CreateAnImage.Bytes;
			var fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg"},new List<byte[]>{newImage});
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var controller = new SyncController(_isync, _bgTaskQueue, _query,storageSelector);
			controller.ControllerContext = context;

			var result = controller.SyncIndex("/") as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			var path = list.FirstOrDefault(p => p.FilePath == "/test.jpg").FilePath;
			Assert.AreEqual("/test.jpg", path);
		}

		[TestMethod]
		public void SyncControllerTest_OneFile()
		{
			InsertSearchData();

			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};
			
			var fakeStorage = new FakeIStorage();
			var storageSelector = new FakeSelectorStorage(fakeStorage);

			var controller = new SyncController(_isync, _bgTaskQueue, _query, storageSelector);
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
			
			var fakeStorage = new FakeIStorage();
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var controller = new SyncController(_isync, _bgTaskQueue, _query, storageSelector);
			controller.ControllerContext = context;

			var result = controller.SyncIndex("/404") as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, list[0].Status);
			
		}
		
		[TestMethod]
		public void SyncControllerTest_Rename_NotFoundInIndex()
		{

			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};
			var fakeStorage = new FakeIStorage();
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var controller = new SyncController(_isync, _bgTaskQueue, _query, storageSelector);
			controller.ControllerContext = context;

			var result = controller.Rename("/notfound-image.jpg", "/test.jpg") as NotFoundObjectResult;
			
			Assert.AreEqual(404,result.StatusCode);
		}
		
		[TestMethod]
		public void SyncControllerTest_Rename_Good()
		{
			InsertSearchData();

			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};
			
			var fakeStorage =  new FakeIStorage(new List<string> { "/" }, 
				new List<string> { _createAnImage.DbPath });
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var controller =
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector)
				{
					ControllerContext = context
				};
			
			var result = controller.Rename(_createAnImage.DbPath, "/test.jpg") as JsonResult;
			var list = result.Value as List<FileIndexItem>;

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,list.FirstOrDefault().Status);

		}

		[TestMethod]
		public void SyncControllerTest_Mkdir_Good()
		{
			InsertSearchData();
			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};
			
			var fakeStorage =  new FakeIStorage(new List<string> { "/" }, 
				new List<string> { _createAnImage.DbPath });
			var storageSelector = new FakeSelectorStorage(fakeStorage);

			var controller =
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector)
				{
					ControllerContext = context
				};
			
			var result = controller.Mkdir("/test_dir") as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,list.FirstOrDefault().Status);

		}
		
		[TestMethod]
		public void SyncControllerTest_Mkdir_Exist()
		{
			InsertSearchData();
			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};
			
			var fakeStorage =  new FakeIStorage(new List<string> { "/" ,"/test_dir" }, 
				new List<string> { _createAnImage.DbPath });
			var storageSelector = new FakeSelectorStorage(fakeStorage);

			var controller =
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector)
				{
					ControllerContext = context
				};
			
			var result = controller.Mkdir("/test_dir") as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,list.FirstOrDefault().Status);

		}
	}
}

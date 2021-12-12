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
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Interfaces;
using starskycore.ViewModels;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public class SyncControllerTest
	{
		private readonly IQuery _query;
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
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
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));

			// Add Background services
			services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
			services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			_appSettings = serviceProvider.GetRequiredService<AppSettings>();

			// inject fake exiftool
			_exifTool = new FakeExifTool(new FakeIStorage(),_appSettings );

			_readmeta = serviceProvider.GetRequiredService<IReadMeta>();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			_query = new Query(_context, new AppSettings(), _scopeFactory, new FakeIWebLogger(), memoryCache);

			_isync = serviceProvider.GetRequiredService<ISync>();

			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();
			
		}

		private FileIndexItem InsertSearchData()
		{
			_iStorage = new FakeIStorage(new List<string> { "/" }, 
				new List<string> { _createAnImage.DbPath });
			var fileHashCode = new FileHash(_iStorage).GetHashCode(_createAnImage.DbPath).Key;
			
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
					ColorClass = ColorClassParser.Color.Winner, // 1
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
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{newImage});
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var controller = new SyncController(_isync, _bgTaskQueue, _query,storageSelector, 
				new FakeIWebSocketConnectionsService());
			controller.ControllerContext = context;

#pragma warning disable 0618
			var result = controller.SyncIndex("/") as JsonResult;
#pragma warning restore 0618
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

			var controller = new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
				new FakeIWebSocketConnectionsService());
			controller.ControllerContext = context;

#pragma warning disable 0618
			var result = controller.SyncIndex(_createAnImage.DbPath) as JsonResult;
#pragma warning restore 0618
			
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
			
			var controller = new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
				new FakeIWebSocketConnectionsService());
			controller.ControllerContext = context;

#pragma warning disable 0618
			var result = controller.SyncIndex("/404") as JsonResult;
#pragma warning restore 0618
			
			var list = result.Value as List<SyncViewModel>;
			
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, list[0].Status);
			
		}
		
		[TestMethod]
		public async Task SyncControllerTest_Rename_NotFoundInIndex()
		{

			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};
			var fakeStorage = new FakeIStorage();
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var controller = new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
				new FakeIWebSocketConnectionsService());
			controller.ControllerContext = context;

			var result = await controller.Rename("/notfound-image.jpg", "/test.jpg") as NotFoundObjectResult;
			
			Assert.AreEqual(404,result.StatusCode);
		}
		
		[TestMethod]
		public async Task SyncControllerTest_Rename_Good()
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
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
					new FakeIWebSocketConnectionsService())
				{
					ControllerContext = context
				};
			
			var result = await controller.Rename(_createAnImage.DbPath, "/test.jpg") as JsonResult;
			var list = result.Value as List<FileIndexItem>;

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,list.FirstOrDefault().Status);

			await _query.RemoveItemAsync(await _query.GetObjectByFilePathAsync("/test.jpg"));
		}
		
		[TestMethod]
		public async Task SyncControllerTest_Rename_WithCurrentStatusDisabled()
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
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
					new FakeIWebSocketConnectionsService())
				{
					ControllerContext = context
				};
			
			var result = await controller.Rename(_createAnImage.DbPath, "/test.jpg", true, false) as JsonResult;
			var list = result.Value as List<FileIndexItem>;

			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,list[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,list[1].Status);

			await _query.RemoveItemAsync(await _query.GetObjectByFilePathAsync("/test.jpg"));
		}

		[TestMethod]
		public async Task SyncControllerTest_Rename_Good_SocketUpdate()
		{
			InsertSearchData();

			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};
			var socket = new FakeIWebSocketConnectionsService();

			var fakeStorage =  new FakeIStorage(new List<string> { "/" }, 
				new List<string> { _createAnImage.DbPath });
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var controller =
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
					socket)
				{
					ControllerContext = context
				};
			
			await controller.Rename(_createAnImage.DbPath, "/test.jpg");
			
			Assert.AreEqual(1,socket.FakeSendToAllAsync.Count);
			Assert.IsTrue(socket.FakeSendToAllAsync[0].Contains("/test.jpg"));
			
			await _query.RemoveItemAsync(await _query.GetObjectByFilePathAsync("/test.jpg"));
		}

		[TestMethod]
		public async Task SyncControllerTest_Mkdir_Good()
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
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
					new FakeIWebSocketConnectionsService())
				{
					ControllerContext = context
				};
			
			var result = await controller.Mkdir("/test_dir") as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,list.FirstOrDefault().Status);
		}
		
		[TestMethod]
		public async Task SyncControllerTest_Mkdir_Good_SocketUpdate()
		{
			InsertSearchData();
			var context = new ControllerContext
			{
				HttpContext = new DefaultHttpContext()
			};

			var socket = new FakeIWebSocketConnectionsService();
			var fakeStorage =  new FakeIStorage(new List<string> { "/" }, 
				new List<string> { _createAnImage.DbPath });
			var storageSelector = new FakeSelectorStorage(fakeStorage);

			var controller =
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
					socket)
				{
					ControllerContext = context
				};
			
			await controller.Mkdir("/test_dir");
			
			var value = socket.FakeSendToAllAsync.FirstOrDefault(p =>
				!p.StartsWith("[system]"));
			Assert.IsTrue(value.Contains("/test_dir"));
		}
		
		[TestMethod]
		public async Task SyncControllerTest_Mkdir_Exist()
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
				new SyncController(_isync, _bgTaskQueue, _query, storageSelector, 
					new FakeIWebSocketConnectionsService())
				{
					ControllerContext = context
				};
			
			var result = await controller.Mkdir("/test_dir") as JsonResult;
			var list = result.Value as List<SyncViewModel>;
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,list.FirstOrDefault().Status);
		}
	}
}

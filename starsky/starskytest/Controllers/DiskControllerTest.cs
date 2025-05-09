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
using starsky.project.web.ViewModels;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class DiskControllerTest
{
	private readonly CreateAnImage _createAnImage;
	private readonly Query _query;
	private IStorage? _iStorage;

	public DiskControllerTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetService<IMemoryCache>();

		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase("SyncControllerTest");
		var options = builderDb.Options;
		var context = new ApplicationDbContext(options);

		// Inject Fake Exiftool; dependency injection
		var services = new ServiceCollection();
		services.AddSingleton<IExifTool, FakeExifTool>();

		// Fake the readmeta output
		services.AddSingleton<IReadMeta, FakeReadMeta>();


		// Inject Config helper
		services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
		// random config
		_createAnImage = new CreateAnImage();
		var dict = new Dictionary<string, string?>
		{
			{ "App:StorageFolder", _createAnImage.BasePath },
			{ "App:ThumbnailTempFolder", _createAnImage.BasePath },
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
		var appSettings = serviceProvider.GetRequiredService<AppSettings>();

		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		_query = new Query(context, appSettings, scopeFactory, new FakeIWebLogger(),
			memoryCache);
	}

	private async Task InsertSearchData()
	{
		_iStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _createAnImage.DbPath });
		var fileHashCode =
			( await new FileHash(_iStorage, new FakeIWebLogger()).GetHashCodeAsync(_createAnImage
				.DbPath) ).Key;

		if ( string.IsNullOrEmpty(await _query.GetSubPathByHashAsync(fileHashCode)) )
		{
			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "/", ParentDirectory = "/", IsDirectory = true
			});

			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = _createAnImage.FileName,
				ParentDirectory = "/",
				FileHash = fileHashCode,
				ColorClass = ColorClassParser.Color.Winner // 1
			});
		}

		await _query.GetObjectByFilePathAsync(_createAnImage.DbPath);
	}


	[TestMethod]
	public async Task SyncControllerTest_Rename_NotFoundInIndex()
	{
		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };
		var fakeStorage = new FakeIStorage();
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller = new DiskController(_query, storageSelector,
			new FakeIWebSocketConnectionsService(), new FakeINotificationQuery());
		controller.ControllerContext = context;

		var result =
			await controller.Rename("/notfound-image.jpg", "/test.jpg") as NotFoundObjectResult;

		Assert.AreEqual(404, result?.StatusCode);
	}

	[TestMethod]
	public async Task Rename_BadRequest()
	{
		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };
		var fakeStorage = new FakeIStorage();
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller = new DiskController(_query, storageSelector,
			new FakeIWebSocketConnectionsService(), new FakeINotificationQuery());
		controller.ControllerContext = context;

		var result =
			await controller.Rename(string.Empty, "/test.jpg") as BadRequestObjectResult;

		Assert.AreEqual(400, result?.StatusCode);
	}

	[TestMethod]
	public async Task Rename_ReturnsBadRequest()
	{
		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };
		var fakeStorage = new FakeIStorage();
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller = new DiskController(_query, storageSelector,
			new FakeIWebSocketConnectionsService(), new FakeINotificationQuery());
		controller.ControllerContext = context;
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result =
			await controller.Rename(string.Empty, "/test.jpg") as BadRequestObjectResult;

		Assert.AreEqual(400, result?.StatusCode);
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task SyncControllerTest_Rename_Good()
	{
		await InsertSearchData();

		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _createAnImage.DbPath });
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller =
			new DiskController(_query, storageSelector,
				new FakeIWebSocketConnectionsService(), new FakeINotificationQuery())
			{
				ControllerContext = context
			};

		var result = await controller.Rename(_createAnImage.DbPath, "/test.jpg") as JsonResult;
		var list = result?.Value as List<FileIndexItem>;

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, list?.FirstOrDefault()?.Status);

		await _query.RemoveItemAsync(( await _query.GetObjectByFilePathAsync("/test.jpg") )!);
	}

	[TestMethod]
	public async Task SyncControllerTest_Rename_WithCurrentStatusDisabled()
	{
		await InsertSearchData();

		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _createAnImage.DbPath });
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller =
			new DiskController(_query, storageSelector,
				new FakeIWebSocketConnectionsService(), new FakeINotificationQuery())
			{
				ControllerContext = context
			};

		var result =
			await controller.Rename(_createAnImage.DbPath, "/test.jpg", true, false) as
				JsonResult;
		var list = result?.Value as List<FileIndexItem>;

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, list?[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, list?[1].Status);

		await _query.RemoveItemAsync(( await _query.GetObjectByFilePathAsync("/test.jpg") )!);
	}

	[TestMethod]
	public async Task SyncControllerTest_Rename_Good_SocketUpdate()
	{
		await InsertSearchData();

		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };
		var socket = new FakeIWebSocketConnectionsService();

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _createAnImage.DbPath });
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller =
			new DiskController(_query, storageSelector,
				socket, new FakeINotificationQuery()) { ControllerContext = context };

		await controller.Rename(_createAnImage.DbPath, "/test.jpg");

		Assert.AreEqual(1, socket.FakeSendToAllAsync.Count(p => !p.Contains("[system]")));
		Assert.IsTrue(socket.FakeSendToAllAsync[0].Contains("/test.jpg"));

		await _query.RemoveItemAsync(( await _query.GetObjectByFilePathAsync("/test.jpg") )!);
	}

	[TestMethod]
	public async Task SyncControllerTest_Mkdir_Good()
	{
		await InsertSearchData();
		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };

		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _createAnImage.DbPath });
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller =
			new DiskController(_query, storageSelector,
				new FakeIWebSocketConnectionsService(), new FakeINotificationQuery())
			{
				ControllerContext = context
			};

		var result = await controller.Mkdir("/test_dir") as JsonResult;
		var list = result?.Value as List<SyncViewModel>;
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, list?.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task Mkdir_ReturnsBadRequest()
	{
		// Arrange
		var controller =
			new DiskController(_query, new FakeSelectorStorage(),
				new FakeIWebSocketConnectionsService(), new FakeINotificationQuery());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		controller.ModelState.AddModelError("Key", "ErrorMessage");

		// Act
		var result = await controller.Mkdir(null!);

		// Assert
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task SyncControllerTest_Mkdir_Good_SocketUpdate()
	{
		await InsertSearchData();
		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };

		var socket = new FakeIWebSocketConnectionsService();
		var fakeStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { _createAnImage.DbPath });
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller =
			new DiskController(_query, storageSelector,
				socket, new FakeINotificationQuery()) { ControllerContext = context };

		await controller.Mkdir("/test_dir");

		var value = socket.FakeSendToAllAsync.Find(p =>
			!p.StartsWith("[system]"));

		Assert.IsNotNull(value);
		Assert.IsTrue(value.Contains("/test_dir"));
	}

	[TestMethod]
	public async Task SyncControllerTest_Mkdir_Exist()
	{
		await InsertSearchData();
		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };

		var fakeStorage = new FakeIStorage(new List<string> { "/", "/test_dir" },
			new List<string> { _createAnImage.DbPath });
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller =
			new DiskController(_query, storageSelector,
				new FakeIWebSocketConnectionsService(), new FakeINotificationQuery())
			{
				ControllerContext = context
			};

		var result = await controller.Mkdir("/test_dir") as JsonResult;
		var list = result?.Value as List<SyncViewModel>;
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,
			list?.FirstOrDefault()?.Status);
	}


	[TestMethod]
	public async Task Mkdir_BadRequest()
	{
		var context = new ControllerContext { HttpContext = new DefaultHttpContext() };

		var fakeStorage = new FakeIStorage(new List<string> { "/", "/test_dir" },
			new List<string> { _createAnImage.DbPath });
		var storageSelector = new FakeSelectorStorage(fakeStorage);

		var controller =
			new DiskController(_query, storageSelector,
				new FakeIWebSocketConnectionsService(), new FakeINotificationQuery())
			{
				ControllerContext = context
			};

		await controller.Mkdir(string.Empty);

		Assert.AreEqual(400, context.HttpContext.Response.StatusCode);
	}
}

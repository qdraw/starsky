using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
using starsky.feature.geolookup.Interfaces;
using starsky.feature.geolookup.Models;
using starsky.feature.geolookup.Services;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Metrics;
using starsky.foundation.worker.Services;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class GeoControllerTest
{
	private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
	private readonly IMemoryCache _memoryCache;
	private readonly IServiceScopeFactory _scopeFactory;

	public GeoControllerTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		_memoryCache = provider.GetRequiredService<IMemoryCache>();

		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase(nameof(ExportControllerTest));

		// Inject Fake Exiftool; dependency injection
		var services = new ServiceCollection();
		services.AddSingleton<IExifTool, FakeExifTool>();

		// Fake the readMeta output
		services.AddSingleton<IReadMeta, FakeReadMeta>();

		// Inject Config helper
		services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
		// random config
		var createAnImage = new CreateAnImage();
		var dict = new Dictionary<string, string?>
		{
			{ "App:StorageFolder", createAnImage.BasePath },
			{ "App:ThumbnailTempFolder", createAnImage.BasePath },
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

		// for in bg test
		services.AddSingleton<IGeoBackgroundTask, FakeIGeoBackgroundTask>();

		// metrics
		services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
		services.AddSingleton<UpdateBackgroundQueuedMetrics>();

		// build the service
		var serviceProvider = services.BuildServiceProvider();
		// get the service

		_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		// get the background helper
		_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();
	}


	[TestMethod]
	public async Task FolderExist()
	{
		var fakeIStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" });

		var controller = new GeoController(_bgTaskQueue,
			new FakeSelectorStorage(fakeIStorage), null!, new FakeIWebLogger(), _scopeFactory)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		var result = await controller.GeoSyncFolder() as JsonResult;
		Assert.AreEqual("job started", result?.Value);
	}

	[TestMethod]
	public async Task FolderNotExist()
	{
		var fakeIStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" });

		var controller = new GeoController(_bgTaskQueue, new FakeSelectorStorage(fakeIStorage),
			_memoryCache, new FakeIWebLogger(), _scopeFactory)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		var result = await controller.GeoSyncFolder("/not-found") as NotFoundObjectResult;
		Assert.AreEqual(404, result?.StatusCode);
	}

	[TestMethod]
	public async Task GeoSyncFolder_BadRequestObjectResult()
	{
		var storage = new FakeIStorage();
		var controller = new GeoController(_bgTaskQueue,
			new FakeSelectorStorage(storage), null!, new FakeIWebLogger(), _scopeFactory)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = await
			controller.GeoSyncFolder(null!);
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void StatusCheck_CachedItemExist()
	{
		// set startup status aka 50%
		new GeoCacheStatusService(_memoryCache).StatusUpdate("/StatusCheck_CachedItemExist",
			1, StatusType.Current);
		new GeoCacheStatusService(_memoryCache).StatusUpdate("/StatusCheck_CachedItemExist",
			2, StatusType.Total);

		var storage = new FakeIStorage();

		var controller = new GeoController(_bgTaskQueue, new FakeSelectorStorage(storage),
			_memoryCache, new FakeIWebLogger(), _scopeFactory)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};

		var statusJson = controller.Status("/StatusCheck_CachedItemExist") as JsonResult;
		var status = statusJson!.Value as GeoCacheStatus;
		Assert.AreEqual(1, status?.Current);
		Assert.AreEqual(2, status?.Total);
	}

	[TestMethod]
	public void StatusCheck_CacheServiceMissing_ItemNotExist()
	{
		var storage = new FakeIStorage();
		var controller = new GeoController(_bgTaskQueue,
			new FakeSelectorStorage(storage), null!, new FakeIWebLogger(), _scopeFactory)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};

		var status =
			controller.Status("/StatusCheck_CachedItemNotExist") as NotFoundObjectResult;
		Assert.AreEqual(404, status?.StatusCode);
	}

	[TestMethod]
	public void StatusCheck_BadRequestObjectResult()
	{
		var storage = new FakeIStorage();
		var controller = new GeoController(_bgTaskQueue,
			new FakeSelectorStorage(storage), null!, new FakeIWebLogger(), _scopeFactory)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result =
			controller.Status(null!);
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task QueueBackgroundWorkItemAsync()
	{
		// reset
		var geoBackgroundTaskBefore = _scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<IGeoBackgroundTask>() as FakeIGeoBackgroundTask;
		Assert.IsNotNull(geoBackgroundTaskBefore);
		geoBackgroundTaskBefore.Count = 0;
		// end reset

		var storage = new FakeIStorage(new List<string> { "/" });
		var controller = new GeoController(new FakeIUpdateBackgroundTaskQueue(),
			new FakeSelectorStorage(storage), null!, new FakeIWebLogger(), _scopeFactory)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};

		await controller.GeoSyncFolder();

		var geoBackgroundTask = _scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<IGeoBackgroundTask>() as FakeIGeoBackgroundTask;
		Assert.IsNotNull(geoBackgroundTask);
		Assert.AreEqual(1, geoBackgroundTask.Count);
	}
}

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
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;


namespace starskytest.Controllers
{
	[TestClass]
	public sealed class CacheIndexControllerTest
	{
		private readonly Query _query;
		private readonly AppSettings _appSettings;
		private readonly ApplicationDbContext _context;

		public CacheIndexControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase("test1234");
			var options = builderDb.Options;
			_context = new ApplicationDbContext(options);
			_query = new Query(_context, new AppSettings(), null!, new FakeIWebLogger(),
				memoryCache);

			// Inject Fake ExifTool; dependency injection
			var services = new ServiceCollection();

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

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			_appSettings = serviceProvider.GetRequiredService<AppSettings>();
		}

		[TestMethod]
		public async Task CacheIndexController_CheckIfCacheIsRemoved_CleanCache()
		{
			// Act
			var controller = new CacheIndexController(_query, _appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "cacheDeleteTest", ParentDirectory = "/", IsDirectory = true
			});

			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "file.jpg", ParentDirectory = "/cacheDeleteTest", IsDirectory = false
			});

			Assert.IsTrue(_query.DisplayFileFolders("/cacheDeleteTest").Any());

			// Ask the cache
			_query.DisplayFileFolders("/cacheDeleteTest");

			// Don't notify the cache that there is an update
			var newItem = new FileIndexItem
			{
				FileName = "file2.jpg",
				ParentDirectory = "/cacheDeleteTest",
				IsDirectory = false
			};
			_context.FileIndex.Add(newItem);
			_context.SaveChanges();
			// Write changes to database

			// Check if there is one item in the cache
			var beforeQuery = _query.DisplayFileFolders("/cacheDeleteTest");
			Assert.AreEqual(1, beforeQuery.Count());

			// Act, remove content from cache
			var actionResult = controller.RemoveCache("/cacheDeleteTest") as JsonResult;
			Assert.AreEqual("cache successful cleared", actionResult?.Value);

			// Check if there are now two items in the cache
			var newQuery = _query.DisplayFileFolders("/cacheDeleteTest");
			Assert.AreEqual(2, newQuery.Count());
		}

		[TestMethod]
		public async Task RemoveCache_CacheDidNotExist()
		{
			// Act
			var controller = new CacheIndexController(_query, _appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "cacheDeleteTest2", ParentDirectory = "/", IsDirectory = true
			});

			// Act, remove content from cache
			var actionResult = controller.RemoveCache("/cacheDeleteTest2") as JsonResult;
			Assert.AreEqual("cache did not exist", actionResult?.Value);
		}

		[TestMethod]
		public void CacheIndexController_NonExistingCacheRemove()
		{
			// Act
			var controller = new CacheIndexController(_query, _appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.RemoveCache("/404page") as BadRequestObjectResult;
			Assert.AreEqual(400, actionResult?.StatusCode);
		}

		[TestMethod]
		public void CacheIndexController_CacheDisabled()
		{
			var controller =
				new CacheIndexController(_query, new AppSettings { AddMemoryCache = false });
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.RemoveCache("/404page") as JsonResult;
			Assert.AreEqual("cache disabled in config", actionResult?.Value);
		}

		[TestMethod]
		public void ListCache_CacheDidNotExist()
		{
			// Act
			var controller = new CacheIndexController(_query, _appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			// Act, remove content from cache
			var actionResult = controller.ListCache("/cacheDeleteTest2") as BadRequestObjectResult;
			Assert.AreEqual("ignored, please check if the 'f' path " +
			                "exist or use a folder string to get the cache", actionResult?.Value);
		}

		[TestMethod]
		public void ListCache_CacheDisabled()
		{
			var controller =
				new CacheIndexController(_query, new AppSettings { AddMemoryCache = false });
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.ListCache("/404page") as JsonResult;
			Assert.AreEqual("cache disabled in config", actionResult?.Value);
		}

		[TestMethod]
		public void ListCache_GetCache()
		{
			// Act
			var controller = new CacheIndexController(_query, _appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			_query.AddCacheParentItem("/list-cache",
				new List<FileIndexItem>
				{
					new FileIndexItem
					{
						FileName = "cacheDeleteTest2",
						ParentDirectory = "/list-cache",
						IsDirectory = true
					}
				});

			// Act, remove content from cache
			var actionResult = controller.ListCache("/list-cache") as JsonResult;

			Assert.IsNotNull(actionResult);
			Assert.IsNotNull(actionResult.Value);
		}
	}
}

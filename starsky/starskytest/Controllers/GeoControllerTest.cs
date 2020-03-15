using System.Collections.Generic;
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
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starsky.foundation.geo.Models;
using starsky.foundation.geo.Services;
using starsky.foundation.query.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;
using Query = starsky.foundation.query.Services.Query;

namespace starskytest.Controllers
{
	[TestClass]
	public class GeoControllerTest
	{
		private readonly IQuery _query;
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IReadMeta _readmeta;
		private readonly IServiceScopeFactory _scopeFactory;
		private IMemoryCache _memoryCache;

		public GeoControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase(nameof(ExportControllerTest));
			var options = builderDb.Options;
			var context = new ApplicationDbContext(options);
			_query = new Query(context, _memoryCache);

			// Inject Fake Exiftool; dependency injection
			var services = new ServiceCollection();
			services.AddSingleton<IExifTool, FakeExifTool>();

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
			_exifTool = new FakeExifTool(new FakeIStorage(),_appSettings );

			_readmeta = serviceProvider.GetRequiredService<IReadMeta>();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
		}
		

		[TestMethod]
		public void FolderExist()
		{
			var istorage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});

			var controller = new GeoController(_exifTool, _appSettings, _bgTaskQueue, _readmeta,new FakeSelectorStorage(istorage))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			var result = controller.SyncFolder("/") as JsonResult;
			Assert.AreEqual("event fired",result.Value);
		}
		
		[TestMethod]
		public void FolderNotExist()
		{
			var istorage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});

			var controller = new GeoController(_exifTool, _appSettings, _bgTaskQueue, _readmeta,new FakeSelectorStorage(istorage))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			var result = controller.SyncFolder("/not-found") as NotFoundObjectResult;
			Assert.AreEqual(404,result.StatusCode);
		}

		[TestMethod]
		public void StatusCheck_CachedItemExist()
		{
			// set startup status aka 50%
			new GeoCacheStatusService(_memoryCache).Update("/StatusCheck_CachedItemExist",1, StatusType.Current);
			new GeoCacheStatusService(_memoryCache).Update("/StatusCheck_CachedItemExist",2, StatusType.Total);

			var storage = new FakeIStorage();
			var storageSelector = new FakeSelectorStorage(storage);
			
			var controller = new GeoController(_exifTool, _appSettings, _bgTaskQueue, _readmeta,storageSelector,_memoryCache)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			
			var statusJson = controller.Status("/StatusCheck_CachedItemExist") as JsonResult;
			var status = statusJson.Value as GeoCacheStatus;
			Assert.AreEqual(1,status.Current);
			Assert.AreEqual(2,status.Total);
		}
		
		[TestMethod]
		public void StatusCheck_CachedItemNotExist()
		{
			var storage = new FakeIStorage();
			var storageSelector = new FakeSelectorStorage(storage);
			var controller = new GeoController(_exifTool, _appSettings, _bgTaskQueue, _readmeta,storageSelector)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			
			var status = controller.Status("/StatusCheck_CachedItemNotExist") as NotFoundObjectResult;
			Assert.AreEqual(404,status.StatusCode);
		}
	}
}

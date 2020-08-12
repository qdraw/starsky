using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
using starsky.feature.export.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public class PublishControllerTest
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IReadMeta _readmeta;

		public PublishControllerTest()
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
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));

			// Add Background services
			services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			_appSettings = serviceProvider.GetRequiredService<AppSettings>();

			// get the background helper
			_bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
			
		}

		private FileIndexItem InsertSearchData(bool delete = false)
		{
			var iStorage = new StorageSubPathFilesystem(_appSettings);
			var fileHashCode = new FileHash(iStorage).GetHashCode(_createAnImage.DbPath).Key;
			if ( string.IsNullOrEmpty(_query.GetSubPathByHash(fileHashCode)) )
			{
				var isDelete = string.Empty;
				if ( delete ) isDelete = "!delete!";
				_query.AddItem(new FileIndexItem
				{
					FileName = _createAnImage.FileName,
					ParentDirectory = "/",
					FileHash = fileHashCode,
					ColorClass = ColorClassParser.Color.Winner, // 1
					Tags = isDelete
				});
			}
			return _query.GetObjectByFilePath(_createAnImage.DbPath);
		}

		[TestMethod]
		public void PublishGet_List()
		{
			var controller = new PublishController(new AppSettings(), new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(), new FakeIMetaInfo(), new FakeSelectorStorage(),
				_bgTaskQueue);
			
			var actionResult = controller.PublishGet() as JsonResult;
			var result = actionResult.Value as IEnumerable<string>;

			Assert.AreEqual("test", result.FirstOrDefault());
		}

		[TestMethod]
		public async Task PublishCreate_newItem()
		{
			var controller = new PublishController(new AppSettings(), new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(), new FakeIMetaInfo(), new FakeSelectorStorage(),
				_bgTaskQueue);
			
			var actionResult = await controller.PublishCreate("/test.jpg", 
				"test", "test", true) as JsonResult;
			var result = actionResult.Value as string;
			
			Assert.AreEqual("test", result);
		}
		
		[TestMethod]
		public async Task PublishCreate_existItem_NoForce()
		{
			var appSettings = new AppSettings{TempFolder = "/"};
			var storage = new FakeIStorage(new List<string> { Path.DirectorySeparatorChar + "test" },
				new List<string> { Path.DirectorySeparatorChar + "test.zip" });

			var controller = new PublishController(appSettings, new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(), new FakeIMetaInfo(), new FakeSelectorStorage(storage),
				_bgTaskQueue);
			
			var actionResult = await controller.PublishCreate("/test.jpg", 
				"test", "test", false) as ConflictObjectResult;
			var result = actionResult.Value as string;
			
			Assert.AreEqual("name test exist", result);
		}

		[TestMethod]
		public async Task PublishCreate_existItem_Force()
		{
			var appSettings = new AppSettings {TempFolder = "/"};
			var storage = new FakeIStorage(new List<string> {Path.DirectorySeparatorChar + "test"}, 
				new List<string>{ Path.DirectorySeparatorChar + "test.zip" });

		var controller = new PublishController(appSettings, new FakeIPublishPreflight(),
				new FakeIWebHtmlPublishService(), new FakeIMetaInfo(), new FakeSelectorStorage(storage),
				_bgTaskQueue);
			
			var actionResult = await controller.PublishCreate("/test.jpg", 
				"test", "test", true) as JsonResult;
			var result = actionResult.Value as string;
			
			Assert.AreEqual("test", result);
			Assert.IsFalse(storage.ExistFolder(Path.DirectorySeparatorChar + "test"));
			Assert.IsFalse(storage.ExistFile(Path.DirectorySeparatorChar + "test.zip"));
		}
		
	}
}

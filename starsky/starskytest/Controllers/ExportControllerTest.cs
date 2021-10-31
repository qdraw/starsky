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
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.webtelemetry.Interfaces;
using starsky.foundation.worker.Services;
using starsky.foundation.writemeta.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public class ExportControllerTest
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly ServiceProvider _serviceProvider;

		public ExportControllerTest()
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
			services.AddSingleton<IWebLogger, FakeIWebLogger>();

			// Fake the readMeta output
			services.AddSingleton<IReadMeta, FakeReadMeta>();

			_bgTaskQueue = new BackgroundTaskQueue();
			
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

			services.AddSingleton<ISelectorStorage, SelectorStorage>();
			services.AddSingleton<IStorage, StorageSubPathFilesystem>();
			services.AddSingleton<IStorage, StorageHostFullPathFilesystem>();
			services.AddSingleton<IStorage, StorageThumbnailFilesystem>();

			// build the service
			_serviceProvider = services.BuildServiceProvider();
			// get the service
			_appSettings = _serviceProvider.GetRequiredService<AppSettings>();
			
		}

		[TestMethod]
		public void ExportController_CreateZipNotFound()
		{
			var iStorage = new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger());
			var storageSelector = new FakeSelectorStorage(iStorage);
			var export = new ExportService(_query,_appSettings,storageSelector, new FakeIWebLogger());
			var controller = new ExportController( _bgTaskQueue, storageSelector, export);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.CreateZip("/fail", true, false) as NotFoundObjectResult;
			Assert.AreEqual(404,actionResult.StatusCode);
		}

		[TestMethod]
		public async Task ExportController_TestZipping() {
			
			// to avoid skip of adding zip
			var zipFilesList = Directory.GetFiles(_createAnImage.BasePath, 
					"*.*", SearchOption.AllDirectories)
				.Where(p => ".zip" == Path.GetExtension(p) );
			
			foreach ( var toDelPath in zipFilesList )
			{
				new StorageHostFullPathFilesystem().FileDelete(toDelPath);
			}
			
			IServiceCollection services = new ServiceCollection();
			services.AddHostedService<BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<ITelemetryService, FakeTelemetryService>();
			var serviceProvider = services.BuildServiceProvider();

			var service = serviceProvider.GetService<IHostedService>() as BackgroundQueuedHostedService;

			var backgroundQueue = serviceProvider.GetService<IBackgroundTaskQueue>();

			if ( service == null ) throw new Exception("service should not be null");
			await service.StartAsync(CancellationToken.None);

			// the test
			_appSettings.DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase;

			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{_createAnImage.DbPath},new List<byte[]>{CreateAnImage.Bytes});
			
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var fakeQuery = new FakeIQuery(new List<FileIndexItem>{new FileIndexItem
			{
				FileName = _createAnImage.DbPath,
				ParentDirectory = "/",
				FileHash = "file-hash",
				ColorClass = ColorClassParser.Color.Winner, // 1
			}});

			var appSettings = new AppSettings {TempFolder = _createAnImage.BasePath, Verbose = true};
			
			var export = new ExportService(fakeQuery,appSettings,storageSelector, new FakeIWebLogger());
			var controller = new ExportController(
				backgroundQueue, storageSelector, export)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};


			var actionResult = controller.CreateZip(_createAnImage.DbPath,
				true,false) as JsonResult;
			
			Assert.AreNotEqual(actionResult, null);
			var zipHash = actionResult.Value as string;

			Assert.AreEqual(true,zipHash.Contains("SR"));

			await Task.Delay(150);

			// Get from real fs in to fake memory
			var sourceFullPath = Path.Join(appSettings.TempFolder,zipHash) + ".zip";
			await fakeStorage.WriteStreamAsync(new StorageHostFullPathFilesystem().ReadStream(sourceFullPath), sourceFullPath);

			var actionResult2zip = controller.Status(zipHash,true) as JsonResult;
			Assert.AreNotEqual(actionResult2zip, null);

			var resultValue = ( string ) actionResult2zip.Value;
			
			if ( resultValue != "OK" && resultValue != "Not Ready" )
			{
				throw new Exception(actionResult2zip.StatusCode.ToString());
			}

			// Don't check if file exist due async
			await service.StopAsync(CancellationToken.None);
		}


		[TestMethod]
		public async Task ExportControllerTest__ThumbTrue_CreateListToExport()
		{
			var selectorStorage = _serviceProvider.GetRequiredService<ISelectorStorage>();
			
			var export = new ExportService(_query,_appSettings,selectorStorage, new FakeIWebLogger());

			var item = new FileIndexItem
			{
				FileName = "testFile.jpg",
				ParentDirectory = "/",
				FileHash = _createAnImage.FileName,
				Status = FileIndexItem.ExifStatus.Ok
			};

			await _query.AddItemAsync(item);

			var fileIndexResultsList = new List<FileIndexItem> { item };

			var filePaths = await export.CreateListToExport(fileIndexResultsList, true);

			Assert.AreEqual(true,filePaths.FirstOrDefault().Contains(item.FileHash));
		}
		
		[TestMethod]
		public async Task ExportControllerTest__ThumbFalse_AddXmpFile_CreateListToExport()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, new List<string>
			{
				_appSettings.DatabasePathToFilePath("/test.dng", false), 
				_appSettings.DatabasePathToFilePath("/test.xmp", false),
				"/test.dng",
				"/test.xmp"
			});
			
			var selectorStorage = new FakeSelectorStorage(storage);

			var fileIndexResultsList = new List<FileIndexItem>
			{
				new FileIndexItem
				{
					FileName = "test.dng",
					ParentDirectory = "/",
					FileHash = "FileHash",
					Status = FileIndexItem.ExifStatus.Ok
				}
			};
			var fakeQuery = new FakeIQuery(fileIndexResultsList);
			
			var export = new ExportService(fakeQuery,_appSettings,selectorStorage, new FakeIWebLogger());

			var filePaths = await export.CreateListToExport(fileIndexResultsList, false);

			Assert.AreEqual(true,filePaths[0].Contains("test.dng"));
			Assert.AreEqual(true,filePaths[1].Contains("test.xmp"));
		}

		
		[TestMethod]
		public async Task ExportControllerTest__ThumbFalse_CreateListToExport()
		{
			var selectorStorage = _serviceProvider.GetRequiredService<ISelectorStorage>();
			var hostFileSystemStorage =
				selectorStorage.Get(SelectorStorage.StorageServices
					.HostFilesystem);
			
			var export = new ExportService(_query,_appSettings,selectorStorage, new FakeIWebLogger());

			var createAnImageNoExif = new CreateAnImageNoExif();

			var item = new FileIndexItem
			{
				FileName = createAnImageNoExif.FileName,
				ParentDirectory = "/",
				FileHash = createAnImageNoExif.FileName.Replace(".jpg", "-test"),
				Status = FileIndexItem.ExifStatus.Ok
			};

			await _query.AddItemAsync(item);

			var fileIndexResultsList = new List<FileIndexItem> { item };
			
			var filePaths = await export.CreateListToExport(fileIndexResultsList,false);

			Assert.AreEqual(true, filePaths.FirstOrDefault().Contains(item.FileName));

			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,
				hostFileSystemStorage.IsFolderOrFile(filePaths.FirstOrDefault()));

			hostFileSystemStorage.FileDelete(createAnImageNoExif.FullFilePathWithDate);
		}

		[TestMethod]
		public void ExportControllerTest__ThumbFalse__FilePathToFileName()
		{
			var storage = new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger());
			var selectorStorage = new FakeSelectorStorage(storage);
			var export = new ExportService(_query,_appSettings,selectorStorage, new FakeIWebLogger());

			var filePaths = new List<string>
			{
				Path.Combine("test","file.jpg")
			};
			var fileNames = export.FilePathToFileName(filePaths, false);
			Assert.AreEqual("file.jpg",fileNames.FirstOrDefault());
		}

		[TestMethod]
		public void ExportControllerTest__ThumbTrue__FilePathToFileName()
		{
			var storage = new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger());
			var selectorStorage = new FakeSelectorStorage(storage);
			var export = new ExportService(_query,_appSettings,selectorStorage, new FakeIWebLogger());
			var filePaths = new List<string>
			{
				Path.Combine("test","thumb.jpg")
			};

			_query.AddItem(new FileIndexItem
			{
				FileName = "file.jpg",
				ParentDirectory = "/test",
				FileHash = "thumb"
			});
			
			var fileNames = export.FilePathToFileName(filePaths, true);
			Assert.AreEqual("file.jpg",fileNames.FirstOrDefault());
			
			// This is a strange one: 
			// We use thumb as base32 fileHashes but export 
			// as file.jpg or the nice original name
		}

		[TestMethod]
		public void ExportController_ZipNotFound()
		{
			var storage = new StorageSubPathFilesystem(_appSettings, new FakeIWebLogger());
			var selectorStorage = new FakeSelectorStorage(storage);
			var export = new ExportService(_query,_appSettings,selectorStorage, new FakeIWebLogger());
			var controller = new ExportController( _bgTaskQueue, selectorStorage, export);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.Status("____fail", true) as NotFoundObjectResult;
			Assert.AreEqual(404, actionResult.StatusCode);
		}
	}
}

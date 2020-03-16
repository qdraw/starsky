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
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.query.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;
using Query = starsky.foundation.query.Services.Query;

namespace starskytest.Controllers
{
	[TestClass]
	public class ExportControllerTest
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IReadMeta _readmeta;

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
			new FakeExifTool(new FakeIStorage(),_appSettings );

			_readmeta = serviceProvider.GetRequiredService<IReadMeta>();
			serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		private FileIndexItem InsertSearchData(bool delete = false)
		{
			var iStorage = new StorageSubPathFilesystem(_appSettings);
			var fileHashCode = new FileHash(iStorage).GetHashCode(_createAnImage.DbPath);
			if ( string.IsNullOrEmpty(_query.GetSubPathByHash(fileHashCode)) )
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
		public async Task ExportController_CreateZipNotFound()
		{
			var iStorage = new StorageSubPathFilesystem(_appSettings);
			var storageSelector = new FakeSelectorStorage(iStorage);
			var controller = new ExportController(_query, _appSettings, _bgTaskQueue, storageSelector);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.CreateZip("/fail", true, false) as NotFoundObjectResult;
			Assert.AreEqual(404,actionResult.StatusCode);
		}

		[TestMethod]
		public async Task ExportController_TestZipping() {
			
			// to avoid skip of adding zip
			var zipFilesList = Directory.GetFiles(_createAnImage.BasePath, "*.*", SearchOption.AllDirectories)
				.Where(p => ".zip" == Path.GetExtension(p) );
			
			foreach ( var toDelPath in zipFilesList )
			{
				new StorageHostFullPathFilesystem().FileDelete(toDelPath);
			}
			
			IServiceCollection services = new ServiceCollection();
			services.AddHostedService<BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
			var serviceProvider = services.BuildServiceProvider();

			var service = serviceProvider.GetService<IHostedService>() as BackgroundQueuedHostedService;

			var backgroundQueue = serviceProvider.GetService<IBackgroundTaskQueue>();

			await service.StartAsync(CancellationToken.None);

			// the test
			var createAnImage = InsertSearchData(true);
			_appSettings.DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase;

			var newImage = CreateAnImage.Bytes;
			var fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{createAnImage.FilePath},new List<byte[]>{newImage});
			var storageSelector = new FakeSelectorStorage(fakeStorage);
			
			var controller = new ExportController(_query, _appSettings, backgroundQueue, storageSelector);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.CreateZip(createAnImage.FilePath,true,false) as JsonResult;
			Assert.AreNotEqual(actionResult, null);
			var zipHash = actionResult.Value as string;

			Assert.AreEqual(zipHash.Contains("SR"),true);

			await Task.Delay(100);

			// Get from real fs in to fake memory
			var sourceFullPath = Path.Join(_appSettings.TempFolder,zipHash) + ".zip";
			await fakeStorage.WriteStreamAsync(new StorageHostFullPathFilesystem().ReadStream(sourceFullPath), sourceFullPath);

			var actionResult2zip = await controller.Zip(zipHash,true) as JsonResult;
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
		public void ExportControllerTest__ThumbTrue_CreateListToExport()
		{
			var storage = new StorageSubPathFilesystem(_appSettings);
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ExportController(_query, _appSettings, _bgTaskQueue, selectorStorage);

			var item = new FileIndexItem
			{
				FileName = "testFile.jpg",
				ParentDirectory = "/",
				FileHash = "FileHash",
				Status = FileIndexItem.ExifStatus.Ok
			};

			_query.AddItem(item);

			var fileIndexResultsList = new List<FileIndexItem> { item };

			var filePaths = controller.CreateListToExport(fileIndexResultsList, true);

			Assert.AreEqual(true,filePaths.FirstOrDefault().Contains(item.FileHash));
		}

		
		[TestMethod]
		public void ExportControllerTest__ThumbFalse_CreateListToExport()
		{
			var storage = new StorageSubPathFilesystem(_appSettings);
			var hostFileSystemStorage = new StorageHostFullPathFilesystem();
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ExportController(_query, _appSettings, _bgTaskQueue, selectorStorage);

			var createAnImageNoExif = new CreateAnImageNoExif();

			var item = new FileIndexItem
			{
				FileName = createAnImageNoExif.FileName,
				ParentDirectory = "/",
				FileHash = createAnImageNoExif.FileName.Replace(".jpg", "-test"),
				Status = FileIndexItem.ExifStatus.Ok
			};

			_query.AddItem(item);

			var fileIndexResultsList = new List<FileIndexItem> { item };

			var filePaths = controller.CreateListToExport(fileIndexResultsList,false);

			Assert.AreEqual(true, filePaths.FirstOrDefault().Contains(item.FileName));

			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,
				hostFileSystemStorage.IsFolderOrFile(filePaths.FirstOrDefault()));

			hostFileSystemStorage.FileDelete(createAnImageNoExif.FullFilePathWithDate);
		}

		[TestMethod]
		public void ExportControllerTest__ThumbFalse__FilePathToFileName()
		{
			var storage = new StorageSubPathFilesystem(_appSettings);
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ExportController(_query, _appSettings, _bgTaskQueue, selectorStorage);
			var filePaths = new List<string>
			{
				Path.Combine("test","file.jpg")
			};
			var fileNames = controller.FilePathToFileName(filePaths, false);
			Assert.AreEqual("file.jpg",fileNames.FirstOrDefault());
		}

		[TestMethod]
		public void ExportControllerTest__ThumbTrue__FilePathToFileName()
		{
			var storage = new StorageSubPathFilesystem(_appSettings);
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ExportController(_query, _appSettings, _bgTaskQueue, selectorStorage);
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
			
			var fileNames = controller.FilePathToFileName(filePaths, true);
			Assert.AreEqual("file.jpg",fileNames.FirstOrDefault());
			
			// This is a strange one: 
			// We use thumb as base32 filehashes but export 
			// as file.jpg or the nice orginal name
		}

		[TestMethod]
		public async Task ExportController_ZipNotFound()
		{
			var storage = new StorageSubPathFilesystem(_appSettings);
			var selectorStorage = new FakeSelectorStorage(storage);
			var controller = new ExportController(_query, _appSettings, _bgTaskQueue, selectorStorage);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.Zip("____fail", true) as NotFoundObjectResult;
			Assert.AreEqual(404, actionResult.StatusCode);
		}
	}
}

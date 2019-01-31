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
using starskycore.Data;
using starskytests.FakeMocks;
using starskytests.Models;
using starsky.Controllers;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskytests.FakeCreateAn;
using Query = starskycore.Services.Query;

namespace starskytests.Controllers
{
	[TestClass]
	public class ExportControllerTest
	{
		private readonly IQuery _query;
		private readonly IExiftool _exiftool;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly ApplicationDbContext _context;
		private readonly IReadMeta _readmeta;
		private readonly IServiceScopeFactory _scopeFactory;

		public ExportControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase(nameof(ExportControllerTest));
			var options = builderDb.Options;
			_context = new ApplicationDbContext(options);
			_query = new Query(_context, memoryCache);

			// Inject Fake Exiftool; dependency injection
			var services = new ServiceCollection();
			services.AddSingleton<IExiftool, FakeExiftool>();

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
			_exiftool = serviceProvider.GetRequiredService<IExiftool>();

			_readmeta = serviceProvider.GetRequiredService<IReadMeta>();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

	
		}

		private FileIndexItem InsertSearchData(bool delete = false)
		{

			var fileHashCode = FileHash.GetHashCode(_createAnImage.FullFilePath);
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
			var controller = new ExportController(_query, _exiftool, _appSettings, _bgTaskQueue);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.CreateZip("/fail", true, false) as NotFoundObjectResult;
			Assert.AreEqual(404,actionResult.StatusCode);
		}

		[TestMethod]
		public async Task ExportController_TestZipping() {
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
			var controller = new ExportController(_query, _exiftool, _appSettings, backgroundQueue);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			// to avoid skip of adding zip
			var zipFilesList = Directory.GetFiles(_createAnImage.BasePath, "*.*", SearchOption.AllDirectories)
				.Where(p => ".zip" == Path.GetExtension(p) );
			Files.DeleteFile(zipFilesList);
			
			
			backgroundQueue.QueueBackgroundWorkItem(async token =>
			{
				Console.WriteLine("kdlsf");
			});

			var actionResult = await controller.CreateZip(createAnImage.FilePath,true,false) as JsonResult;
			Assert.AreNotEqual(actionResult, null);
			var zipHash = actionResult.Value as string;

			Assert.AreEqual(zipHash.Contains("SR"),true);

			await Task.Delay(100);

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
		public async Task ExportController_TestExportRaws()
		{

			IServiceCollection services = new ServiceCollection();
			services.AddHostedService<BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
			var serviceProvider = services.BuildServiceProvider();
			var backgroundQueue = serviceProvider.GetService<IBackgroundTaskQueue>();


			var controller = new ExportController(_query, _exiftool, _appSettings, backgroundQueue);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var createAnImageNoExif = new CreateAnImageNoExif();
			new PlainTextFileHelper().WriteFile(createAnImageNoExif.FullFilePathWithDate.Replace(".jpg",".xmp"),"test");

			_query.AddItem(new FileIndexItem
			{
				FileName = createAnImageNoExif.FileName,
				ParentDirectory = "/",
				ImageFormat = Files.ImageFormat.tiff //<== not true but for test
			});

			var all = _query.GetAllFiles("/");

			var t = await controller.CreateZip($"/{createAnImageNoExif.FileName}", true, false) as JsonResult;

			// todo: cleanup files

		}

		[TestMethod]
		public void ExportControllerTest__ThumbTrue_CreateListToExport()
		{
			var controller = new ExportController(_query, _exiftool, _appSettings, _bgTaskQueue);

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
			var controller = new ExportController(_query, _exiftool, _appSettings, _bgTaskQueue);

			var item = new FileIndexItem
			{
				FileName = "testFile.jpg",
				ParentDirectory = "/",
				FileHash = "FileHash",
				Status = FileIndexItem.ExifStatus.Ok
			};

			_query.AddItem(item);

			var fileIndexResultsList = new List<FileIndexItem> { item };

			// change to false
			var filePaths = controller.CreateListToExport(fileIndexResultsList, false);

			Assert.AreEqual(true, filePaths.FirstOrDefault().Contains(item.FileName));

		}

		//[TestMethod]
		//public void ExportControllerTest__ThumbFalse_CreateListToExport()
		//{
		//	var controller = new ExportController(_query, _exiftool, _appSettings, _bgTaskQueue);

		//	var createAnImageNoExif = new CreateAnImageNoExif();

		//	var item = new FileIndexItem
		//	{
		//		FileName = createAnImageNoExif.FileName,
		//		ParentDirectory = "/",
		//		FileHash = createAnImageNoExif.FileName.Replace(".jpg", "-test"),
		//		Status = FileIndexItem.ExifStatus.Ok
		//	};

		//	_query.AddItem(item);

		//	var fileIndexResultsList = new List<FileIndexItem> { item };

		//	var filePaths = controller.CreateListToExport(fileIndexResultsList,false);

		//	Assert.AreEqual(true, filePaths.FirstOrDefault().Contains(item.FileName));

		//	Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,
		//		Files.IsFolderOrFile(filePaths.FirstOrDefault()));

		//	Files.DeleteFile(createAnImageNoExif.FullFilePathWithDate);
		//}



		[TestMethod]
		public async Task ExportController_ZipNotFound()
		{
			var controller = new ExportController(_query, _exiftool, _appSettings, _bgTaskQueue);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.Zip("____fail", true) as NotFoundObjectResult;
			Assert.AreEqual(404, actionResult.StatusCode);
		}


	}
}

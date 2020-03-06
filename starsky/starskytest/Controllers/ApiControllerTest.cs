using System;
using System.Collections.Generic;
using System.IO;
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
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;
using Query = starskycore.Services.Query;

namespace starskytest.Controllers
{
    [TestClass]
    public class ApiControllerTest
    {
        private readonly IQuery _query;
        private readonly IExifTool _exifTool;
        private readonly AppSettings _appSettings;
        private readonly CreateAnImage _createAnImage;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
        private readonly ApplicationDbContext _context;
        private readonly IReadMeta _readmeta;
        private readonly IServiceScopeFactory _scopeFactory;
	    private readonly IStorage _iStorage;

	    public ApiControllerTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            
            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
            builderDb.UseInMemoryDatabase("test1234");
            var options = builderDb.Options;
            _context = new ApplicationDbContext(options);
            _query = new Query(_context,memoryCache);
            
            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();

            // Fake the readmeta output
            services.AddSingleton<IReadMeta, FakeReadMeta>();    
            
            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // random config
            _createAnImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", _createAnImage.BasePath },
                { "App:ThumbnailTempFolder",_createAnImage.BasePath },
                { "App:Verbose", "true" }
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
            _exifTool = new FakeExifTool(_iStorage,_appSettings);
            
            _readmeta = serviceProvider.GetRequiredService<IReadMeta>();
            _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            
            // get the background helper
            _bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
	        
			_iStorage = new StorageSubPathFilesystem(_appSettings);

        }
        
        private FileIndexItem InsertSearchData(bool delete = false)
        {
            var fileHashCode = new FileHash(_iStorage).GetHashCode(_createAnImage.DbPath);
	        
            if (string.IsNullOrEmpty(_query.GetSubPathByHash(fileHashCode)))
            {
                var isDelete = string.Empty;
                if (delete) isDelete = "!delete!";
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
        public void ApiController_Delete_API_HappyFlow_Test()
        {
            var createAnImage = InsertSearchData(true);
            _appSettings.DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase;
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue, new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));

            Console.WriteLine("createAnImage.FilePath");
            Console.WriteLine(createAnImage.FilePath);

            var actionResult = controller.Delete(createAnImage.FilePath) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var jsonCollection = actionResult.Value as List<FileIndexItem>;
            Assert.AreEqual(createAnImage.FilePath,jsonCollection.FirstOrDefault().FilePath);
            new CreateAnImage(); //restore afterwards
        }

        [TestMethod]
        public void ApiController_Delete_API_RemoveNotAllowedFile_Test()
        {

	        
	        // re add data
            var createAnImage = InsertSearchData();
	        
	        // Clean existing items to avoid errors
	        var itemByHash = _query.SingleItem(createAnImage.FilePath);
	        itemByHash.FileIndexItem.Tags = string.Empty;
	        _query.UpdateItem(itemByHash.FileIndexItem);
	        
            _appSettings.DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase;
	        
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var notFoundResult = controller.Delete(createAnImage.FilePath) as NotFoundObjectResult;
            Assert.AreEqual(404,notFoundResult.StatusCode);
            var jsonCollection = notFoundResult.Value as List<FileIndexItem>;

            Assert.AreEqual(FileIndexItem.ExifStatus.Unauthorized,jsonCollection.FirstOrDefault().Status);

            _query.RemoveItem(_query.SingleItem(createAnImage.FilePath).FileIndexItem);
        }

        [TestMethod]
        public void ApiController_Thumbnail_HappyFlowDisplayJson_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            
            new Thumbnail(_iStorage).CreateThumb(createAnImage.FilePath, createAnImage.FileHash);
            
            var actionResult = controller.Thumbnail(createAnImage.FileHash,true,true) as JsonResult;
            Assert.AreNotEqual(actionResult,null);
            var thumbnailAnswer = actionResult.Value as string;
            Assert.AreEqual("OK",thumbnailAnswer);

            var thumbnewImg = new CreateAnImage().BasePath + 
                              Path.DirectorySeparatorChar + createAnImage.FileHash + ".jpg";
            File.Delete(thumbnewImg);
        }

        [TestMethod]
        public void ApiController_Thumbnail_InputBadRequest()
        {
	        var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
	        var actionResult = controller.Thumbnail("../") as BadRequestResult;
			Assert.AreEqual(400,actionResult.StatusCode);
        }

        [TestMethod]
        public void ApiController_Thumbnail_HappyFlowFileStreamResult_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            
            new Thumbnail(new StorageSubPathFilesystem(_appSettings) )
	            .CreateThumb(createAnImage.FilePath,createAnImage.FileHash);

            var actionResult = controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
            var thumbnailAnswer = actionResult.ContentType;
            Assert.AreEqual("image/jpeg",thumbnailAnswer);
            actionResult.FileStream.Dispose(); // for windows

            var thumbnewImg = new CreateAnImage().BasePath + 
                              Path.DirectorySeparatorChar + createAnImage.FileHash + ".jpg";
            File.Delete(thumbnewImg);
        }

        [TestMethod]
        public void ApiController_Thumbnail_ShowOrginalImage_API_Test()
        {
            var createAnImage = InsertSearchData();
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
	        controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var actionResult = controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
            var thumbnailAnswer = actionResult.ContentType;
            Assert.AreEqual("image/jpeg",thumbnailAnswer);
            actionResult.FileStream.Dispose(); // for windows

        }

        [TestMethod]
        public void ApiController_ThumbIsMissing_ButOrginalExist_butNoIsSingleItemFlag_API_Test()
        {
            // Photo exist in database but " + "isSingleItem flag is Missing
            var createAnImage = InsertSearchData();

            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var actionResult = controller.Thumbnail(createAnImage.FileHash, false, true) as JsonResult;
            var thumbnailAnswer = actionResult.StatusCode; // always null for some reason ?!
            Assert.AreEqual("Thumbnail is not ready yet",actionResult.Value);
        }

        [TestMethod]
        public void ApiController_FloatingDatabaseFileTest_API_Test()
        {
            var item = _query.AddItem(new FileIndexItem
            {
                //FilePath = "/fakeImage/fake.jpg",
                ParentDirectory = "/fakeImage/",
                FileName = "fake.jpg",
                FileHash = "0986524678765456786543",
				Id= 788,
            });
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var actionResult = controller.Thumbnail(item.FileHash, false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
            _query.RemoveItem(item);
        }

        [TestMethod]
        public void ApiController_Thumbnail1_NonExistingFile_API_Test()
        {
	        var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
        }

        [TestMethod]
        public void ApiController_ENV_starskyTestEnv()
        {
	        var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.Env();
        }
        
        [TestMethod]
        public void ApiController_Update_AllDataIncluded_WithFakeExiftool()
        {
            var createAnImage = new CreateAnImage();
            InsertSearchData();
            
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var input = new FileIndexItem
            {
                Tags = "test"
            };
            var jsonResult = controller.Update(input, createAnImage.DbPath,false,false) as JsonResult;
            var fileModel = jsonResult.Value as List<FileIndexItem>;
            //you could not test because exiftool is an external dependency
            Assert.AreNotEqual(null,fileModel.FirstOrDefault().Tags);
        }
        
        [TestMethod]
        public void ApiController_Update_SourceImageMissingOnDisk_WithFakeExiftool()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "345678765434567.jpg",
                ParentDirectory = "/",
                FileHash = "345678765434567"
            });

            var controller =
                new ApiController(_query, _exifTool, _appSettings, _bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)))
                {
                    ControllerContext = {HttpContext = new DefaultHttpContext()}
                };

            var testElement = new FileIndexItem();
            var notFoundResult = controller.Update(testElement, "/345678765434567.jpg",false,false) as NotFoundObjectResult;
            Assert.AreEqual(404,notFoundResult.StatusCode);

            _query.RemoveItem(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
        }
        
        [TestMethod]
        public void ApiController_Info_AllDataIncluded_WithFakeExiftool()
        {
            // Using Fake exiftool
            // Uses FAKE readMeta
            var createAnImage = new CreateAnImage();
            InsertSearchData();
            
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var jsonResult = controller.Info(createAnImage.DbPath,false) as JsonResult;
            var exiftoolModel = jsonResult.Value as List<FileIndexItem>;
            Assert.AreEqual("test, sion",exiftoolModel.FirstOrDefault().Tags);            
        }

        [TestMethod]
        public void ApiController_Info_SourceImageMissingOnDisk_WithFakeExiftool()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "345678765434567.jpg",
                ParentDirectory = "/",
                FileHash = "345678765434567"
            });
            
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var notFoundResult = controller.Info("/345678765434567.jpg") as NotFoundObjectResult;
            Assert.AreEqual(404,notFoundResult.StatusCode);
            
            _query.RemoveItem(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
        }
        
        [TestMethod]
        public void ApiController_Delete_SourceImageMissingOnDisk_WithFakeExiftool()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "345678765434567.jpg",
                ParentDirectory = "/",
                FileHash = "345678765434567"
            });
            
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var notFoundResult = controller.Delete("/345678765434567.jpg") as NotFoundObjectResult;
            Assert.AreEqual(404,notFoundResult.StatusCode);

            _query.RemoveItem(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
        }
        
        [TestMethod]
        public void ApiController_Thumbnail_NonExistingFile_API_Test()
        {
	        var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
        }

        [TestMethod]
        public void ApiController_Thumbnail_CorruptImage_NoContentResult_Test()
        {
            // Arrange
            var thumbHash = "ApiController_Thumbnail_CorruptImage_Test";
            var path = _createAnImage.BasePath + Path.DirectorySeparatorChar + thumbHash + ".jpg";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path)) 
                {
                    sw.WriteLine("CorruptImage");
                } 
            }
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "ApiController_Thumbnail_CorruptImage_Test.jpg",
                ParentDirectory = "/",
                FileHash = thumbHash
            });
            
            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var actionResult = controller.Thumbnail(thumbHash, false, true) as NoContentResult;
            Assert.AreEqual(204,actionResult.StatusCode);
               
            // remove files + database item
            _query.RemoveItem(_query.GetObjectByFilePath("/" + thumbHash + ".jpg"));
            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }

        [TestMethod]
        public void ApiController_DownloadPhoto_isThumbnailTrue_CreateThumb_ReturnFileStream_Test()
        {
            // Arange
            var fileIndexItem = InsertSearchData();

            // Remove thumb if exist
            var thumbPath = new CreateAnImage().BasePath + 
                            Path.DirectorySeparatorChar + fileIndexItem.FileHash +
                            ".jpg";
            
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
            
            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var actionResult =  controller.DownloadPhoto(fileIndexItem.FilePath)  as FileStreamResult;
            Assert.AreNotEqual(null,actionResult);

            actionResult.FileStream.Dispose();

            // Clean
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
        }
        
        [TestMethod]
        public void ApiController_DownloadPhoto_isThumbnailFalse_ReturnFileStream_Test()
        {
            // Arange
            var fileIndexItem = InsertSearchData();

            // Remove thumb if exist
            var thumbPath = new CreateAnImage().BasePath + 
                            Path.DirectorySeparatorChar + 
                            fileIndexItem.FileHash + ".jpg";
            

            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
            
            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var actionResult =  controller.DownloadPhoto(fileIndexItem.FilePath,false)  as FileStreamResult;
            Assert.AreNotEqual(null,actionResult);

            actionResult.FileStream.Dispose();


            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
        }
        
        [TestMethod]
        public void ApiController_DownloadPhoto_isThumbnailTrue_ReturnAThumb_ReturnFileStream_Test()
        {
            // Arange
            var fileIndexItem = InsertSearchData();

            // Remove thumb if exist
            var thumbPath = new CreateAnImage().BasePath + 
                            Path.DirectorySeparatorChar + fileIndexItem.FileHash +
                            ".jpg";
            
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
            
            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            // Run once
            var actionResult1 = controller.DownloadPhoto(fileIndexItem.FilePath) as FileStreamResult;
            actionResult1.FileStream.Dispose();


            // Run twice
            var actionResult2 =  controller.DownloadPhoto(fileIndexItem.FilePath)  as FileStreamResult;
            Assert.AreNotEqual(null,actionResult2);

            actionResult2.FileStream.Dispose();

            // Clean
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }
        }

        [TestMethod]
        public void ApiController_DownloadPhoto_SourceImageIsMissing_Test()
        {
            var thumbHash = "ApiController_Thumbnail_CorruptImage_Test";
            var path = _createAnImage.BasePath + Path.DirectorySeparatorChar + thumbHash + ".jpg";
           

            _query.AddItem(new FileIndexItem
            {
                FileName = "ApiController_Thumbnail_CorruptImage_Test.jpg",
                ParentDirectory = "/",
                FileHash = thumbHash
            });

            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            var actionResult =  controller.DownloadPhoto("/" + thumbHash)  as NotFoundObjectResult;
            Assert.AreNotEqual(null,actionResult);
            Assert.AreEqual(404,actionResult.StatusCode);

            
            // remove files + database item
            _query.RemoveItem(_query.GetObjectByFilePath("/" + thumbHash + ".jpg"));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
        }
        
        
        [TestMethod]
        public void ApiController_DownloadPhoto_Thumb_base_folder_not_found_Test()
        {
            // this works
            
            // Arange
            var fileIndexItem = InsertSearchData();
            
            var appSettingsthumbtest = _appSettings;
            appSettingsthumbtest.ThumbnailTempFolder = null;
            
            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            // Run once
            var actionResult = controller.DownloadPhoto(fileIndexItem.FilePath) as NotFoundObjectResult;
            Assert.AreNotEqual(null,actionResult);
            Assert.AreEqual(404,actionResult.StatusCode);

        }

        [TestMethod]
        public void ApiController_CheckIfCacheIsRemoved_CleanCache()
        {
            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            _query.AddItem(new FileIndexItem
            {
                FileName = "cacheDeleteTest",
                ParentDirectory = "/",
                IsDirectory = true
            });
            
            _query.AddItem(new FileIndexItem
            {
                FileName = "file.jpg",
                ParentDirectory = "/cacheDeleteTest",
                IsDirectory = false
            });

            Assert.AreEqual(true,_query.DisplayFileFolders("/cacheDeleteTest").Any());
            
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
            Assert.AreEqual("cache successful cleared", actionResult.Value);
            
            // Check if there are now two items in the cache
            var newQuery = _query.DisplayFileFolders("/cacheDeleteTest");
            Assert.AreEqual(2, newQuery.Count());
        }

        [TestMethod]
        public void ApiController_NonExistingCacheRemove()
        {
            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)));
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            var actionResult = controller.RemoveCache("/404page") as BadRequestObjectResult;
            Assert.AreEqual(400,actionResult.StatusCode);
        }

        [TestMethod]
        public void ApiController_CacheDisabled()
        {
            var appsettings = new AppSettings {AddMemoryCache = false};

            var controller =
                new ApiController(_query, _exifTool, appsettings, _bgTaskQueue,new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings)))
                {
                    ControllerContext = {HttpContext = new DefaultHttpContext()}
                };
            var actionResult = controller.RemoveCache("/404page") as JsonResult;
            Assert.AreEqual("cache disabled in config",actionResult.Value);
        }
    }
}

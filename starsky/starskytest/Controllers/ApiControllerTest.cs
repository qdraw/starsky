using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Middleware;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskytest.Models;

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
            
            // Inject Fake ExifTool; dependency injection
            var services = new ServiceCollection();

            // Fake the readMeta output
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
            services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
            
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
            var fileHashCode = new FileHash(_iStorage).GetHashCode(_createAnImage.DbPath).Key;
	        
            if (string.IsNullOrEmpty(_query.GetSubPathByHash(fileHashCode)))
            {
                var isDelete = string.Empty;
                if (delete) isDelete = "!delete!";
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
        public void ApiController_CheckIfCacheIsRemoved_CleanCache()
        {
	        var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));

            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,selectorStorage,null);
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
	        var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));

            // Act
            var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,selectorStorage,null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
            var actionResult = controller.RemoveCache("/404page") as BadRequestObjectResult;
            Assert.AreEqual(400,actionResult.StatusCode);
        }

        [TestMethod]
        public void ApiController_CacheDisabled()
        {
	        var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));

            var appsettings = new AppSettings {AddMemoryCache = false};

            var controller =
                new ApiController(_query, _exifTool, appsettings, _bgTaskQueue,selectorStorage,null)
                {
                    ControllerContext = {HttpContext = new DefaultHttpContext()}
                };
            var actionResult = controller.RemoveCache("/404page") as JsonResult;
            Assert.AreEqual("cache disabled in config",actionResult.Value);
        }
    }
}

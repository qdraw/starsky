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
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
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
	public class MetaUpdateControllerTest
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

	    public MetaUpdateControllerTest()
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
        public async Task ApiController_Update_AllDataIncluded_WithFakeExifTool()
        {
	        var createAnImage = new CreateAnImage();
	        InsertSearchData();
            
	        var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));
	        
	        var metaPreflight = new MetaPreflight(_query,_appSettings,selectorStorage);
	        var metaUpdateService = new MetaUpdateService(_query,_exifTool,new FakeReadMeta(), selectorStorage, metaPreflight, new FakeConsoleWrapper());
	        var metaReplaceService = new MetaReplaceService(_query,_appSettings,selectorStorage);
	        var controller = new MetaUpdateController(metaPreflight,metaUpdateService, metaReplaceService, _bgTaskQueue);

	        var input = new FileIndexItem
	        {
		        Tags = "test"
	        };
	        var jsonResult = await controller.UpdateAsync(input, createAnImage.DbPath,false,false) as JsonResult;
	        var fileModel = jsonResult.Value as List<FileIndexItem>;
	        //you could not test because exiftool is an external dependency
	        Assert.AreNotEqual(null,fileModel.FirstOrDefault().Tags);
        }
        
        [TestMethod]
        public async Task ApiController_Update_SourceImageMissingOnDisk_WithFakeExifTool()
        {
	        _query.AddItem(new FileIndexItem
	        {
		        FileName = "345678765434567.jpg",
		        ParentDirectory = "/",
		        FileHash = "345678765434567"
	        });
	        var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));

	        var metaPreflight = new MetaPreflight(_query,_appSettings,selectorStorage);
	        var metaUpdateService = new MetaUpdateService(_query,_exifTool,new FakeReadMeta(), selectorStorage, metaPreflight, new FakeConsoleWrapper());
	        var metaReplaceService = new MetaReplaceService(_query,_appSettings,selectorStorage);
	        
	        var controller = new MetaUpdateController(metaPreflight,metaUpdateService, metaReplaceService, _bgTaskQueue)
		        {
			        ControllerContext = {HttpContext = new DefaultHttpContext()}
		        };

	        var testElement = new FileIndexItem();
	        var notFoundResult = await controller.UpdateAsync(testElement, "/345678765434567.jpg",false,false) as NotFoundObjectResult;
	        Assert.AreEqual(404,notFoundResult.StatusCode);

	        _query.RemoveItem(_query.SingleItem("/345678765434567.jpg").FileIndexItem);
        }
	}
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starskycore.Interfaces;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{

    [TestClass]
    public class ImportControllerTest
    {
        private readonly IImport _import;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppSettings _appSettings;

        public ImportControllerTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            
            var memoryCache = provider.GetService<IMemoryCache>();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            
            var services = new ServiceCollection();
            
            _appSettings = new AppSettings();
            
            // Add Background services
            services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // get the background helper
            _bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
            
	        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
	        _import = new FakeIImport(new FakeSelectorStorage(new FakeIStorage()));

        }

        /// <summary>
        ///  Add the file in the underlying request object.
        /// </summary>
        /// <returns>Controller Context with file</returns>
        private ControllerContext RequestWithFile()
        {
	        var httpContext = new DefaultHttpContext();
	        httpContext.Request.Headers.Add("Content-Type", "application/octet-stream");
	        httpContext.Request.Body = new MemoryStream(CreateAnImage.Bytes);
	        
	        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
	        return new ControllerContext(actionContext);
	    }

        [TestMethod]
        public async Task ImportController_WrongInputFlow()
        {
	        var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());

	        var importController = new ImportController(new FakeIImport(fakeStorageSelector), _appSettings, 
		        _bgTaskQueue, null, fakeStorageSelector)
	        {
		        ControllerContext = RequestWithFile(),
	        };

	        var actionResult = await importController.IndexPost()  as JsonResult;
	        var list = actionResult.Value as List<ImportIndexItem>;

	        Assert.AreEqual( ImportStatus.FileError, list.FirstOrDefault().Status);
        }

        [TestMethod]
        public async Task FromUrl_PathInjection()
        {
	        var importController = new ImportController(_import, _appSettings, 
		        _bgTaskQueue, null, new FakeSelectorStorage(new FakeIStorage()))
	        {
		        ControllerContext = RequestWithFile(),
	        };
	        var actionResult = await importController.FromUrl("","../../path-injection.dll",null)  as BadRequestResult;
	        Assert.AreEqual(400, actionResult.StatusCode);
        }

        [TestMethod]
        public async Task FromUrl_RequestFromWhiteListedDomain_NotFound()
        {
	        var fakeHttpMessageHandler = new FakeHttpMessageHandler();
	        var httpClient = new HttpClient(fakeHttpMessageHandler);
	        var httpProvider = new HttpProvider(httpClient);
	        
	        var services = new ServiceCollection();
	        services.AddSingleton<IStorage, FakeIStorage>();
	        services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
	        var serviceProvider = services.BuildServiceProvider();

	        var httpClientHelper = new HttpClientHelper(httpProvider, serviceProvider.GetRequiredService<IServiceScopeFactory>());
	        
	        var importController = new ImportController(_import, _appSettings, 
		        _bgTaskQueue, httpClientHelper, new FakeSelectorStorage(new FakeIStorage()))
	        {
		        ControllerContext = RequestWithFile(),
	        };
	        // download.geoNames is in the FakeHttpMessageHandler always a 404
	        var actionResult = await importController.FromUrl("https://download.geonames.org","example.tiff",null)  as NotFoundObjectResult;
	        Assert.AreEqual(404, actionResult.StatusCode);
        }
        
        [TestMethod]
        public async Task FromUrl_RequestFromWhiteListedDomain_Ok()
        {
	        var fakeHttpMessageHandler = new FakeHttpMessageHandler();
	        var httpClient = new HttpClient(fakeHttpMessageHandler);
	        var httpProvider = new HttpProvider(httpClient);

	        var services = new ServiceCollection();
	        services.AddSingleton<IStorage, FakeIStorage>();
	        services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
	        var serviceProvider = services.BuildServiceProvider();
	        var storageProvider = serviceProvider.GetRequiredService<IStorage>();

	        var httpClientHelper = new HttpClientHelper(httpProvider, serviceProvider.GetRequiredService<IServiceScopeFactory>());
	        
	        var importController = new ImportController(new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings, 
		        _bgTaskQueue, httpClientHelper, new FakeSelectorStorage(storageProvider))
	        {
		        ControllerContext = RequestWithFile(),
	        };

	        var actionResult = await importController.FromUrl("https://qdraw.nl","example_image.tiff",null) as JsonResult;
	        var list = actionResult.Value as List<string>;

	        Assert.IsTrue(list.FirstOrDefault().Contains("example_image.tiff"));
        }
        
        [TestMethod]
        public async Task Import_Thumbnail_Ok()
        {
	        var services = new ServiceCollection();
	        services.AddSingleton<IStorage, FakeIStorage>();
	        services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
	        var serviceProvider = services.BuildServiceProvider();
	        var storageProvider = serviceProvider.GetRequiredService<IStorage>();
	        
	        var importController = new ImportController(new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings, 
		        _bgTaskQueue, null, new FakeSelectorStorage(storageProvider))
	        {
		        ControllerContext = RequestWithFile(),
	        };
	        importController.Request.Headers["filename"] = "01234567890123456789123456.jpg"; // len() 26
	        
	        var actionResult = await importController.Thumbnail() as JsonResult;
	        var list = actionResult.Value as List<string>;

	        Assert.AreEqual( "01234567890123456789123456", list.FirstOrDefault());
        }
        
        [TestMethod]
        public async Task Import_Thumbnail_WrongInputName()
        {
	        var services = new ServiceCollection();
	        services.AddSingleton<IStorage, FakeIStorage>();
	        services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
	        var serviceProvider = services.BuildServiceProvider();
	        var storageProvider = serviceProvider.GetRequiredService<IStorage>();
	        
	        var importController = new ImportController(new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings, 
		        _bgTaskQueue, null, new FakeSelectorStorage(storageProvider))
	        {
		        ControllerContext = RequestWithFile(),
	        };
	        importController.Request.Headers["filename"] = "123.jpg"; // len() 3
	        
	        var actionResult = await importController.Thumbnail() as JsonResult;
	        var list = actionResult.Value as List<string>;

	        Assert.AreEqual( 0, list.Count);
        }
    }
}

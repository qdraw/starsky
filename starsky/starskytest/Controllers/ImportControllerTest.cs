using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Controllers;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
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

	        var importController = new ImportController(new FakeIImport(fakeStorageSelector), _appSettings, _scopeFactory,
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
	        var importController = new ImportController(_import, _appSettings, _scopeFactory,
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

	        var httpClientHelper = new HttpClientHelper(httpProvider, new FakeSelectorStorage(new FakeIStorage()));
	        
	        var importController = new ImportController(_import, _appSettings, _scopeFactory,
		        _bgTaskQueue, httpClientHelper, new FakeSelectorStorage(new FakeIStorage()))
	        {
		        ControllerContext = RequestWithFile(),
	        };
	        // download.geonames is in the FakeHttpmessageHandler always a 404
	        var actionResult = await importController.FromUrl("https://download.geonames.org","example.tiff",null)  as NotFoundObjectResult;
	        Assert.AreEqual(404, actionResult.StatusCode);
        }
        
        [TestMethod]
        public async Task FromUrl_RequestFromWhiteListedDomain_Ok()
        {
	        var fakeHttpMessageHandler = new FakeHttpMessageHandler();
	        var httpClient = new HttpClient(fakeHttpMessageHandler);
	        var httpProvider = new HttpProvider(httpClient);

	        var storageProvider = new FakeIStorage();
	        var httpClientHelper = new HttpClientHelper(httpProvider, new FakeSelectorStorage(storageProvider));
	        
	        var importController = new ImportController(new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings, _scopeFactory,
		        _bgTaskQueue, httpClientHelper, new FakeSelectorStorage(storageProvider))
	        {
		        ControllerContext = RequestWithFile(),
	        };

	        var actionResult = await importController.FromUrl("https://qdraw.nl","example_image.tiff",null) as JsonResult;
	        var list = actionResult.Value as List<string>;

	        Assert.IsTrue(list.FirstOrDefault().Contains("example_image.tiff"));
        }
    }
}

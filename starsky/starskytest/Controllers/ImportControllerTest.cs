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
            
            _import = new FakeIImport();
            // IImport import, AppSettings appSettings, 
//            IServiceScopeFactory scopeFactory, IBackgroundTaskQueue queue, 
//            HttpClientHelper httpClientHelper, IStorage iStorage
	        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

//            _importController = new ImportController(_import, new AppSettings(), _scopeFactory,_bgTaskQueue,null,new FakeIStorage())
//            {
//	            ControllerContext = {HttpContext = new DefaultHttpContext()}
//            };
        }


        // Add the file in the underlying request object.
        private ControllerContext RequestWithFileFromData()
        {
	        var httpContext = new DefaultHttpContext();
	        httpContext.Request.Headers.Add("Content-Type", "multipart/form-data; boundary=\"--9051914041544843365972754266\"");


//	        var text = "--9051914041544843365972754266\n" +
//	                   "Content-Disposition: form-data; name=\"files\"; filename=\"anp-52220411.jpg\"\n" +
//	                   "Content-Type: image/jpeg\n" +
//	                   "yolo\n" +
//	                   "--9051914041544843365972754266--";
	        
	        var text = "--9051914041544843365972754266\r\n" +
	                   "Content-Disposition: form-data; name=\"text\"\r\n" +
	                   "\r\n" +
	                   "text default\r\n" +
	                   "--9051914041544843365972754266--\r\n";

//	        var text = "skdfnlsdflksd\n-----------------------------70143061614066247291641834127--";
//	        httpContext.Request.Headers.Add("Content-Length", text.Length.ToString());
	        
//	        var tempStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
//	        
//	        var formFile = new FormFile(tempStream, 0, text.Length, "files", "dummy.txt");
//	        
//	        var formCollection = new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection { formFile });
//
//	        var formFileContent = formFile.ToString();
////	        file.NewFile()

	        
	        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(text));
	        
//	        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

//	        DisableFormValueModelBindingAttribute.OnResourceExecuting(context)
		        
//	        httpContext.
	        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
	        
//	        // copy from net filters
//	        var filters = new List<IFilterMetadata>
//	        {
//		        
//	        };
//	        var values = new List<IValueProviderFactory>();
//
//	        var context = new ResourceExecutingContext(actionContext, filters, values);
//
//	        //Run
//	        new DisableFormValueModelBindingAttribute().OnResourceExecuting(context);
	        
	        return new ControllerContext(actionContext);
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
	        var importController = new ImportController(_import, _appSettings, _scopeFactory,
		        _bgTaskQueue, null, new FakeIStorage())
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
		        _bgTaskQueue, null, new FakeIStorage())
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

	        var httpClientHelper = new HttpClientHelper(httpProvider);
	        
	        var importController = new ImportController(_import, _appSettings, _scopeFactory,
		        _bgTaskQueue, httpClientHelper, new FakeIStorage())
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

	        var httpClientHelper = new HttpClientHelper(httpProvider);
	        
	        var importController = new ImportController(_import, _appSettings, _scopeFactory,
		        _bgTaskQueue, httpClientHelper, new FakeIStorage())
	        {
		        ControllerContext = RequestWithFile(),
	        };

	        var actionResult = await importController.FromUrl("https://qdraw.nl","example.tiff",null) as JsonResult;
	        var list = actionResult.Value as List<string>;

	        Assert.AreEqual("/example.tiff", list.FirstOrDefault());
        }
    }
}

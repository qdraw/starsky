using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
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

namespace starskytest.Controllers
{
	[TestClass]
	public class UploadControllerTest
	{
		private readonly IQuery _query;
		private readonly IStorage _iStorage;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _createAnImage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IReadMeta _readmeta;
		private readonly IServiceScopeFactory _scopeFactory;
		private ImportService _import;
		private SyncService _isync;

		public UploadControllerTest()
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
			


			var services = new ServiceCollection();

			// Fake the readmeta output
			services.AddSingleton<IReadMeta, FakeReadMeta>();

			// Inject Config helper
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			// random config
			_createAnImage = new CreateAnImage();
			
			_iStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{_createAnImage.DbPath}, 
				new List<byte[]>{CreateAnImage.Bytes}, 
				new List<string>{null});
			
	        _readmeta = new ReadMeta(_iStorage,_appSettings);

            _isync = new SyncService(_query,_appSettings,_readmeta,_iStorage);
                        
			_import = new ImportService(context,_isync,new FakeExifTool(_iStorage,_appSettings), _appSettings,null,_iStorage);

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

			_readmeta = serviceProvider.GetRequiredService<IReadMeta>();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task UploadToFolder_BadRequest()
		{
			var controller =
				new UploadController(_import, _appSettings, _isync, _iStorage, _query)
				{
					ControllerContext = {HttpContext = new DefaultHttpContext()}
				};
			
			var actionResult = await controller.UploadToFolder()as BadRequestObjectResult;
			
			Assert.AreEqual(400,actionResult.StatusCode);
		}
		
//		[TestMethod]
		public async Task UploadToFolder_BadReque1111st()
		{
			var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
			httpContext.Request.Headers["to"] = "/"; //Set header
			
			var controller =
				new UploadController(_import, _appSettings, _isync, _iStorage, _query)
				{
					ControllerContext =
					{
						HttpContext = httpContext
					}
				};
			
			var actionResult = await controller.UploadToFolder();
			
//			Assert.AreEqual(400,actionResult.StatusCode);
		}

	}
}

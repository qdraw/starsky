using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.http.Services;
using starsky.foundation.import.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Metrics;
using starsky.foundation.worker.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class ImportControllerTest
{
	private readonly AppSettings _appSettings;
	private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
	private readonly IImport _import;
	private readonly IServiceScopeFactory _scopeFactory;

	public ImportControllerTest()
	{
		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase("test");

		var services = new ServiceCollection();

		_appSettings = new AppSettings();

		// Add Background services
		services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
		services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();
		// metrics
		services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
		services.AddSingleton<UpdateBackgroundQueuedMetrics>();

		var serviceProvider = services.BuildServiceProvider();

		// get the background helper
		_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();

		_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		_import = new FakeIImport(new FakeSelectorStorage(new FakeIStorage()));
	}

	/// <summary>
	///     Add the file in the underlying request object.
	/// </summary>
	/// <returns>Controller Context with file</returns>
	private static ControllerContext RequestWithFile()
	{
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Headers.Append("Content-Type", "application/octet-stream");
		httpContext.Request.Body = new MemoryStream(CreateAnImage.Bytes.ToArray());

		var actionContext = new ActionContext(httpContext, new RouteData(),
			new ControllerActionDescriptor());
		return new ControllerContext(actionContext);
	}

	[TestMethod]
	public async Task ImportController_WrongInputFlow()
	{
		var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());

		var importController = new ImportController(new FakeIImport(fakeStorageSelector),
			_appSettings,
			_bgTaskQueue, null!, fakeStorageSelector, _scopeFactory, new FakeIWebLogger())
		{
			ControllerContext = RequestWithFile()
		};

		var actionResult = await importController.IndexPost() as JsonResult;
		var list = actionResult?.Value as List<ImportIndexItem>;

		Assert.AreEqual(ImportStatus.FileError, list?.FirstOrDefault()?.Status);
	}


	[TestMethod]
	public async Task FromUrl_PathInjection()
	{
		var importController = new ImportController(_import, _appSettings,
			_bgTaskQueue, null!, new FakeSelectorStorage(new FakeIStorage()), _scopeFactory,
			new FakeIWebLogger()) { ControllerContext = RequestWithFile() };
		var actionResult =
			await importController.FromUrl("", "../../path-injection.dll", null!) as
				BadRequestResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task FromUrl_BadRequest()
	{
		var importController = new ImportController(_import, _appSettings,
			_bgTaskQueue, null!, new FakeSelectorStorage(new FakeIStorage()), _scopeFactory,
			new FakeIWebLogger()) { ControllerContext = RequestWithFile() };
		importController.ModelState.AddModelError("Key", "ErrorMessage");

		var result =
			await importController.FromUrl(null!, null!, null!);

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
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

		var httpClientHelper = new HttpClientHelper(httpProvider,
			serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FakeIWebLogger());

		var importController = new ImportController(_import, _appSettings,
			_bgTaskQueue, httpClientHelper, new FakeSelectorStorage(new FakeIStorage()),
			_scopeFactory, new FakeIWebLogger()) { ControllerContext = RequestWithFile() };
		// download.geoNames is in the FakeHttpMessageHandler always a 404
		var actionResult =
			await importController.FromUrl("https://download.geonames.org", "example.tiff",
				null!) as NotFoundObjectResult;
		Assert.AreEqual(404, actionResult?.StatusCode);
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

		var httpClientHelper = new HttpClientHelper(httpProvider,
			serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FakeIWebLogger());

		var importController = new ImportController(
			new FakeIImport(new FakeSelectorStorage(storageProvider)), _appSettings,
			_bgTaskQueue, httpClientHelper, new FakeSelectorStorage(storageProvider),
			_scopeFactory, new FakeIWebLogger()) { ControllerContext = RequestWithFile() };

		var actionResult =
			await importController.FromUrl("https://qdraw.nl", "example_image.tiff", null!) as
				JsonResult;
		var list = actionResult?.Value as List<ImportIndexItem>;

		Assert.IsTrue(list?.FirstOrDefault()?.FilePath?.Contains("example_image.tiff"));
	}
}

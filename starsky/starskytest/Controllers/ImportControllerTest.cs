using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starsky.foundation.http.Services;
using starsky.foundation.import.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.worker.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class ImportControllerTest
{
	private readonly AppSettings _appSettings;
	private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
	private readonly IImport _import;

	public ImportControllerTest()
	{
		_appSettings = new AppSettings();
		_bgTaskQueue = new FakeIUpdateBackgroundTaskQueue();

		_import = new FakeIImport(new FakeSelectorStorage(new FakeIStorage()));
	}

	/// <summary>
	///     Add the file in the underlying request object.
	/// </summary>
	/// <returns>Controller Context with file</returns>
	private static ControllerContext RequestWithFile(ImmutableArray<byte>? fileBytes = null)
	{
		fileBytes ??= CreateAnImage.Bytes;
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Headers.Append("Content-Type", "application/octet-stream");
		httpContext.Request.Body = new MemoryStream([.. fileBytes.Value]);

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
			_bgTaskQueue, null!, fakeStorageSelector, new FakeIWebLogger())
		{
			ControllerContext = RequestWithFile()
		};

		var actionResult = await importController.IndexPost() as JsonResult;
		var list = actionResult?.Value as List<ImportIndexItem>;

		Assert.AreEqual(ImportStatus.FileError, list?.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public async Task IndexPost_WrongInput_Sets415AndLogs()
	{
		var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());
		var fakeLogger = new FakeIWebLogger();

		// Use the FakeIImport which returns FileError items in Preflight
		var importController = new ImportController(new FakeIImport(fakeStorageSelector),
			_appSettings,
			_bgTaskQueue, null!, fakeStorageSelector, fakeLogger)
		{
			ControllerContext = RequestWithFile(CreateAnExifToolTar.Bytes)
		};

		await importController.IndexPost();
		
		// Controller should set 415 and log a debug message
		Assert.AreEqual(415, importController.Response.StatusCode);
		Assert.Contains(t => (t.Item2 ?? string.Empty).Contains("Wrong input"), fakeLogger.TrackedDebug, "Logger should contain 'Wrong input' debug entry");
	}

	[TestMethod]
	public async Task IndexPost_AllItemsAlreadyImported_Returns206()
	{
		var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());
		// Use a FakeIImport implementation that returns an empty Preflight list
		var fakeImport = new FakeIImportForImportTest();

		var importController = new ImportController(fakeImport,
			_appSettings,
			_bgTaskQueue, null!, fakeStorageSelector, new FakeIWebLogger())
		{
			ControllerContext = RequestWithFile()
		};

		// Call the action
		var actionResult = await importController.IndexPost() as JsonResult;
		var list = actionResult?.Value as List<ImportIndexItem>;

		// Preflight returns empty list => JSON result list should be empty
		Assert.IsNotNull(list);
		Assert.IsEmpty(list);

		// When Preflight returns an empty list (no Ok items) and IndexMode is true,
		// the controller should set Response.StatusCode to 206
		Assert.AreEqual(206, importController.Response.StatusCode);
	}

	[TestMethod]
	public async Task IndexPost_AllItemsAlreadyImported_Returns206_EmptyPreflight()
	{
		var fakeStorageSelector = new FakeSelectorStorage(new FakeIStorage());
		var fakeImport = new FakeIImportForImportTest();
		var fakeQueue = new FakeIUpdateBackgroundTaskQueue();

		var importController = new ImportController(fakeImport,
			_appSettings,
			fakeQueue, null!, fakeStorageSelector, new FakeIWebLogger())
		{
			ControllerContext = RequestWithFile()
		};

		// Act
		var actionResult = await importController.IndexPost() as JsonResult;
		var list = actionResult?.Value as List<ImportIndexItem>;

		// Assert
		Assert.IsNotNull(list);
		Assert.IsEmpty(list);
		Assert.AreEqual(206, importController.Response.StatusCode);
		Assert.IsFalse(fakeQueue.QueueBackgroundWorkItemCalled,
			"Empty preflight should not enqueue any background import job.");
	}

	[TestMethod]
	public async Task FromUrl_PathInjection()
	{
		var importController = new ImportController(_import, _appSettings,
			_bgTaskQueue, null!, new FakeSelectorStorage(new FakeIStorage()),
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
			_bgTaskQueue, null!, new FakeSelectorStorage(new FakeIStorage()),
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
			_bgTaskQueue, httpClientHelper, new FakeSelectorStorage(new FakeIStorage()), new FakeIWebLogger()) { ControllerContext = RequestWithFile() };
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
			_bgTaskQueue, httpClientHelper, new FakeSelectorStorage(storageProvider), new FakeIWebLogger()) { ControllerContext = RequestWithFile() };

		var actionResult =
			await importController.FromUrl("https://qdraw.nl", "example_image.tiff", null!) as
				JsonResult;
		var list = actionResult?.Value as List<ImportIndexItem>;

		Assert.IsTrue(list?.FirstOrDefault()?.FilePath?.Contains("example_image.tiff"));
	}
}

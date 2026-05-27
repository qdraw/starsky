using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.import.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class ImportIndexJsonControllerTest
{
	private readonly string _tempFolder = Path.Combine(Path.GetTempPath(),
		$"starsky-import-index-json-controller-{Guid.NewGuid():N}");

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_tempFolder) )
		{
			Directory.Delete(_tempFolder, true);
		}
	}

	[TestMethod]
	public async Task Import_InvalidModelState_ReturnsBadRequest()
	{
		var (_, controller) = CreateSut();
		controller.ModelState.AddModelError("json", "invalid");
		using var payloadDocument = JsonDocument.Parse("{}");

		var result = await controller.Import(payloadDocument.RootElement) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
	}

	[TestMethod]
	public async Task Import_NullJson_ReturnsBadRequest()
	{
		var (_, controller) = CreateSut();
		using var payloadDocument = JsonDocument.Parse("null");

		var result = await controller.Import(payloadDocument.RootElement) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
	}

	[TestMethod]
	public async Task Import_UndefinedJson_ReturnsBadRequest()
	{
		var fakeService = new FakeIImportIndexJsonService();
		var (_, controller) = CreateSut();

		var result = await controller.Import(default) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
		Assert.AreEqual(string.Empty, fakeService.ImportPath);
	}

	[TestMethod]
	public async Task Import_ValidJson_ReturnsJsonResult_AndDeletesTempFile()
	{
		var (service, controller) = CreateSut();
		using var payloadDocument = JsonDocument.Parse("{\"id\":1}");

		var result = await controller.Import(payloadDocument.RootElement) as JsonResult;

		Assert.IsNotNull(result);

		await service.ExportAsync("test.json");
		
			
		Assert.AreEqual("{\"id\":1}", );
		Assert.IsFalse(string.IsNullOrWhiteSpace(service.ImportPath));
		Assert.IsTrue(service.ImportPath.StartsWith(_tempFolder, StringComparison.Ordinal));
		Assert.IsFalse(File.Exists(service.ImportPath));
		Assert.AreSame(service.ImportResult, result.Value);
	}

	[TestMethod]
	public async Task Export_WhenServiceDoesNotWriteFile_ReturnsNotFound()
	{
		var fakeService = new FakeIImportIndexJsonService();
		var controller = CreateSut(fakeService);

		var result = await controller.Export() as NotFoundObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(404, result.StatusCode);
	}

	[TestMethod]
	public async Task Export_WhenServiceCreatesFile_ReturnsJsonContent_AndDeletesTempFile()
	{
		const string expectedPayload = "{\"items\":[]}";
		var controller = CreateSut(service);

		var result = await controller.Export() as ContentResult;

		Assert.IsNotNull(result);
		Assert.AreEqual("application/json", result.ContentType);
		Assert.AreEqual(expectedPayload, result.Content);
		Assert.IsFalse(string.IsNullOrWhiteSpace(service.ExportPath));
		Assert.IsTrue(service.ExportPath.StartsWith(_tempFolder, StringComparison.Ordinal));
		Assert.IsFalse(File.Exists(service.ExportPath));
	}

	[TestMethod]
	public void Controller_HasAdministratorAuthorizeAttribute()
	{
		var authorizeAttribute = typeof(ImportIndexJsonController)
			.GetCustomAttributes(typeof(AuthorizeAttribute), true);

		Assert.HasCount(1, authorizeAttribute);
		Assert.AreEqual(nameof(AccountRoles.AppAccountRoles.Administrator),
			( ( AuthorizeAttribute ) authorizeAttribute[0] ).Roles);
	}

	private (ImportIndexJsonService, ImportIndexJsonController) CreateSut()
	{
		var appSettings = new AppSettings { TempFolder = _tempFolder };
		var storage = new StorageTemporaryFilesystem(new AppSettings { TempFolder = _tempFolder },
			new FakeIWebLogger());
		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new ImportIndexJsonService(
			new FakeIImportQuery([])
			, appSettings, selectorStorage);
		var controller =
			new ImportIndexJsonController(
				appSettings,
				service,
				selectorStorage) { ControllerContext = { HttpContext = new DefaultHttpContext() } };

		return ( service, controller );
	}
}

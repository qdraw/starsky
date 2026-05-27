using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starsky.foundation.import.Interfaces;
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
		var fakeService = new FakeIImportIndexJsonService();
		var controller = CreateController(fakeService);
		controller.ModelState.AddModelError("json", "invalid");
		using var payloadDocument = JsonDocument.Parse("{}");

		var result = await controller.Import(payloadDocument.RootElement) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
		Assert.AreEqual(string.Empty, fakeService.ImportPath);
	}

	[TestMethod]
	public async Task Import_NullJson_ReturnsBadRequest()
	{
		var fakeService = new FakeIImportIndexJsonService();
		var controller = CreateController(fakeService);
		using var payloadDocument = JsonDocument.Parse("null");

		var result = await controller.Import(payloadDocument.RootElement) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
		Assert.AreEqual(string.Empty, fakeService.ImportPath);
	}

	[TestMethod]
	public async Task Import_UndefinedJson_ReturnsBadRequest()
	{
		var fakeService = new FakeIImportIndexJsonService();
		var controller = CreateController(fakeService);

		var result = await controller.Import(default) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
		Assert.AreEqual(string.Empty, fakeService.ImportPath);
	}

	[TestMethod]
	public async Task Import_ValidJson_ReturnsJsonResult_AndDeletesTempFile()
	{
		var service = new CapturingImportIndexJsonService
		{
			ImportResult =
			[
				new ImportIndexItem { FilePath = "/example.jpg" }
			]
		};
		var controller = CreateController(service);
		using var payloadDocument = JsonDocument.Parse("{\"id\":1}");

		var result = await controller.Import(payloadDocument.RootElement) as JsonResult;

		Assert.IsNotNull(result);
		Assert.AreEqual("{\"id\":1}", service.ImportPayload);
		Assert.IsFalse(string.IsNullOrWhiteSpace(service.ImportPath));
		Assert.IsTrue(service.ImportPath.StartsWith(_tempFolder, StringComparison.Ordinal));
		Assert.IsFalse(File.Exists(service.ImportPath));
		Assert.AreSame(service.ImportResult, result.Value);
	}

	[TestMethod]
	public async Task Export_WhenServiceDoesNotWriteFile_ReturnsNotFound()
	{
		var fakeService = new FakeIImportIndexJsonService();
		var controller = CreateController(fakeService);

		var result = await controller.Export() as NotFoundObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(404, result.StatusCode);
	}

	[TestMethod]
	public async Task Export_WhenServiceCreatesFile_ReturnsJsonContent_AndDeletesTempFile()
	{
		const string expectedPayload = "{\"items\":[]}";
		var service = new CapturingImportIndexJsonService { ExportPayload = expectedPayload };
		var controller = CreateController(service);

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

	private ImportIndexJsonController CreateController(IImportIndexJsonService service)
	{
		var storage = new StorageTemporaryFilesystem(new AppSettings { TempFolder = _tempFolder },
			new FakeIWebLogger());
		var selectorStorage = new FakeSelectorStorage(storage);
		var controller =
			new ImportIndexJsonController(
				new AppSettings { TempFolder = _tempFolder },
				service,
				selectorStorage) { ControllerContext = { HttpContext = new DefaultHttpContext() } };

		return controller;
	}

	private sealed class CapturingImportIndexJsonService : IImportIndexJsonService
	{
		public string ExportPath { get; private set; } = string.Empty;
		public string ImportPath { get; private set; } = string.Empty;
		public string ImportPayload { get; private set; } = string.Empty;
		public string ExportPayload { get; init; } = string.Empty;
		public List<ImportIndexItem> ImportResult { get; init; } = [];

		public async Task<string> ExportAsync(string outputJsonPath)
		{
			ExportPath = outputJsonPath;
			await File.WriteAllTextAsync(outputJsonPath, ExportPayload);
			return outputJsonPath;
		}

		public async Task<List<ImportIndexItem>> ImportAsync(string inputJsonPath)
		{
			ImportPath = inputJsonPath;
			ImportPayload = await File.ReadAllTextAsync(inputJsonPath);
			return ImportResult;
		}
	}
}

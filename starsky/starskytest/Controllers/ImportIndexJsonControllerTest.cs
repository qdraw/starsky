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
		var controller = CreateSut();
		controller.ModelState.AddModelError("json", "invalid");
		using var payloadDocument = JsonDocument.Parse("{}");

		var result = await controller.Import(payloadDocument.RootElement) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
	}

	[TestMethod]
	public async Task Import_NullJson_ReturnsBadRequest()
	{
		var controller = CreateSut();
		using var payloadDocument = JsonDocument.Parse("null");

		var result = await controller.Import(payloadDocument.RootElement) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
	}

	[TestMethod]
	public async Task Import_UndefinedJson_ReturnsBadRequest()
	{
		var fakeService = new FakeIImportIndexJsonService();
		var controller = CreateSut(fakeService);

		var result = await controller.Import(default) as BadRequestObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
		Assert.AreEqual(string.Empty, fakeService.ImportPath);
	}

	[TestMethod]
	public async Task Import_ValidJson_ReturnsJsonResult_AndDeletesTempFile()
	{
		var fakeService = new FakeIImportIndexJsonService
		{
			ImportResult = [new ImportIndexItem { FileHash = "test" }]
		};
		var controller = CreateSut(fakeService);
		using var payloadDocument = JsonDocument.Parse("{\"id\":1}");

		var result = await controller.Import(payloadDocument.RootElement) as JsonResult;

		Assert.IsNotNull(result);
		Assert.IsFalse(string.IsNullOrWhiteSpace(fakeService.ImportPath));
		Assert.IsTrue(fakeService.ImportPath.StartsWith(Path.DirectorySeparatorChar));
		var tempFilePath = Path.Combine(_tempFolder, fakeService.ImportPath.TrimStart(Path.DirectorySeparatorChar));
		Assert.IsFalse(File.Exists(tempFilePath));
		Assert.AreSame(fakeService.ImportResult, result.Value);
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
		var fakeService = new WritingImportIndexJsonService(_tempFolder, expectedPayload);
		var controller = CreateSut(fakeService);

		var result = await controller.Export() as ContentResult;

		Assert.IsNotNull(result);
		Assert.AreEqual("application/json", result.ContentType);
		Assert.AreEqual(expectedPayload, result.Content);
		Assert.IsFalse(string.IsNullOrWhiteSpace(fakeService.ExportPath));
		Assert.IsTrue(fakeService.ExportPath.StartsWith(_tempFolder, StringComparison.Ordinal));
		Assert.IsFalse(File.Exists(fakeService.ExportPath));
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

	private ImportIndexJsonController CreateSut(IImportIndexJsonService service)
	{
		var appSettings = new AppSettings { TempFolder = _tempFolder };
		var storage = new StorageTemporaryFilesystem(new AppSettings { TempFolder = _tempFolder },
			new FakeIWebLogger());
		var selectorStorage = new FakeSelectorStorage(storage);

		return new ImportIndexJsonController(
			appSettings,
			service,
			selectorStorage)
		{ ControllerContext = { HttpContext = new DefaultHttpContext() } };
	}

	private ImportIndexJsonController CreateSut()
	{
		var appSettings = new AppSettings { TempFolder = _tempFolder };
		var storage = new StorageTemporaryFilesystem(new AppSettings { TempFolder = _tempFolder },
			new FakeIWebLogger());
		var selectorStorage = new FakeSelectorStorage(storage);

		var service = new ImportIndexJsonService(
			new FakeIImportQuery([])
			, appSettings, selectorStorage);
		return CreateSut(service);
	}

	private sealed class WritingImportIndexJsonService(string baseFolder, string payload)
		: IImportIndexJsonService
	{
		public string ExportPath { get; private set; } = string.Empty;

		public Task<string> ExportAsync(string outputJsonPath)
		{
			ExportPath = Path.Combine(baseFolder, outputJsonPath.TrimStart(Path.DirectorySeparatorChar));
			var directory = Path.GetDirectoryName(ExportPath);
			if ( !string.IsNullOrWhiteSpace(directory) )
			{
				Directory.CreateDirectory(directory);
			}

			File.WriteAllText(ExportPath, payload);
			return Task.FromResult(outputJsonPath);
		}

		public Task<List<ImportIndexItem>> ImportAsync(string inputJsonPath)
		{
			return Task.FromResult(new List<ImportIndexItem>());
		}
	}
}

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Models;
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
	public async Task Export_WhenServiceDoesNotWriteFile_ReturnsNotFound()
	{
		var fakeService = new FakeIImportIndexJsonService();
		var controller = CreateController(fakeService);

		var result = await controller.Export() as NotFoundObjectResult;

		Assert.IsNotNull(result);
		Assert.AreEqual(404, result.StatusCode);
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

	private ImportIndexJsonController CreateController(FakeIImportIndexJsonService service)
	{
		var controller =
			new ImportIndexJsonController(new AppSettings { TempFolder = _tempFolder }, service)
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};

		return controller;
	}
}

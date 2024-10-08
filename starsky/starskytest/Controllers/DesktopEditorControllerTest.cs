using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.desktop.Models;
using starsky.feature.desktop.Service;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class DesktopEditorControllerTest
{
	[TestMethod]
	public void OpenAmountConfirmationChecker_FeatureToggleEnabled()
	{
		var controller = new DesktopEditorController(
			new OpenEditorDesktopService(new AppSettings(),
				new FakeIOpenApplicationNativeService(new List<string>(), "test"),
				new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>())));

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};

		var result = controller.OpenAmountConfirmationChecker("/test.jpg;/test2.jpg");

		var castedResult = ( JsonResult ) result;
		var boolValue = ( bool? ) castedResult.Value;
		// mock is always true

		Assert.IsTrue(boolValue);
	}

	[TestMethod]
	public void OpenAmountConfirmationChecker_ReturnsBadRequest()
	{
		// Arrange
		var controller = new DesktopEditorController(new OpenEditorDesktopService(new AppSettings(),
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>())));

		controller.ModelState.AddModelError("Key", "ErrorMessage");

		// Act
		var result = controller.OpenAmountConfirmationChecker(null!);

		// Assert
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task OpenAsync_FeatureToggleDisabled()
	{
		var controller = new DesktopEditorController(new OpenEditorDesktopService(new AppSettings(),
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>())));

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};

		var result = await controller.OpenAsync("/test.jpg;/test2.jpg");
		var castedResult = ( BadRequestObjectResult ) result;

		Assert.AreEqual(400, castedResult.StatusCode);
	}

	[TestMethod]
	public async Task OpenAsync_ReturnsBadRequest()
	{
		// Arrange
		var controller = new DesktopEditorController(new OpenEditorDesktopService(new AppSettings(),
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>())));

		controller.ModelState.AddModelError("Key", "ErrorMessage");

		// Act
		var result = await controller.OpenAsync(null!);

		// Assert
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task OpenAsync_NoResultsBack()
	{
		var controller = new DesktopEditorController(new OpenEditorDesktopService(
			new AppSettings { UseLocalDesktop = true },
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>())));

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};

		var result = await controller.OpenAsync("/test.jpg;/test2.jpg");
		Assert.AreEqual(206, controller.HttpContext.Response.StatusCode);

		var castedResult = ( JsonResult ) result;
		var arrayValues = ( List<PathImageFormatExistsAppPathModel>? ) castedResult.Value;

		Assert.AreEqual(0, arrayValues?.Count);
	}

	[TestMethod]
	public async Task OpenAsync_HappyFlow()
	{
		var preflight = new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>
		{
			new()
			{
				AppPath = "test",
				Status = FileIndexItem.ExifStatus.Ok,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
				SubPath = "/test.jpg",
				FullFilePath = "/test.jpg"
			}
		});
		var controller = new DesktopEditorController(new OpenEditorDesktopService(
			new AppSettings { UseLocalDesktop = true },
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			preflight));

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};

		var result = await controller.OpenAsync("/test.jpg;/test2.jpg");
		Assert.AreEqual(200, controller.HttpContext.Response.StatusCode);

		var castedResult = ( JsonResult ) result;
		var arrayValues = ( List<PathImageFormatExistsAppPathModel>? ) castedResult.Value;

		Assert.AreEqual(1, arrayValues?.Count);
	}
}

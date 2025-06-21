using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class RedirectControllerTest
{
	private readonly FakeSelectorStorage _fakeSelectorStorage = new();

	private static AppSettings CreateAppSettings()
	{
		return new AppSettings
		{
			Structure = new AppSettingsStructureModel("/yyyyMMdd/{filenamebase}.ext")
		};
	}

	[TestMethod]
	public void RedirectControllerTest_SubPathRelative()
	{
		var appSettings = CreateAppSettings();
		var controller = new RedirectController(_fakeSelectorStorage, appSettings, new FakeIWebLogger());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var result = controller.SubPathRelative(0) as JsonResult;

		var today = "/" + DateTime.Now.ToString("yyyyMMdd");
		Assert.AreEqual(today, result?.Value);
	}

	[TestMethod]
	public void RedirectControllerTest_SubPathRelative_ModelState()
	{
		var appSettings = CreateAppSettings();
		var controller = new RedirectController(_fakeSelectorStorage, appSettings, new FakeIWebLogger());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = controller.SubPathRelative(0);

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void RedirectControllerTest_SubPathRelativeMinusValue()
	{
		var appSettings = CreateAppSettings();
		var controller = new RedirectController(_fakeSelectorStorage, appSettings, new FakeIWebLogger());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var result = controller.SubPathRelative(1) as JsonResult;

		var today = "/" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
		Assert.AreEqual(today, result?.Value);
	}

	[TestMethod]
	public void RedirectControllerTest_LargeInt()
	{
		var appSettings = CreateAppSettings();
		var controller = new RedirectController(_fakeSelectorStorage, appSettings, new FakeIWebLogger())
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		var result = controller.SubPathRelative(201801020) as JsonResult;
		// 201801020= not a date but a large number ==> fallback to today
		var today = "/" + DateTime.Now.ToString("yyyyMMdd");
		Assert.AreEqual(today, result?.Value);
	}

	[TestMethod]
	public void RedirectControllerTest_SubPathRelativeRedirectToAction()
	{
		var appSettings = CreateAppSettings();
		var controller = new RedirectController(_fakeSelectorStorage, appSettings, new FakeIWebLogger())
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		var result = controller.SubPathRelative(0, false) as RedirectToActionResult;

		var today = "/" + DateTime.Now.ToString("yyyyMMdd");

		Assert.AreEqual(today, result?.RouteValues?.Values.FirstOrDefault());
	}
}

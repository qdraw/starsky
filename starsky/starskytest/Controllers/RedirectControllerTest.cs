using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Models;
using starskycore.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public sealed class RedirectControllerTest
	{
		private readonly FakeSelectorStorage _fakeSelectorStorage;
		public RedirectControllerTest()
		{
			_fakeSelectorStorage = new FakeSelectorStorage();
		}
		
		[TestMethod]
		public void RedirectControllerTest_SubpathRelative()
		{
			var appSettings = new AppSettings();
			appSettings.Structure = "/yyyyMMdd/{filenamebase}.ext";
			var controller = new RedirectController(_fakeSelectorStorage, appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var result = controller.SubPathRelative(0, true) as JsonResult;

			var today = "/" + DateTime.Now.ToString("yyyyMMdd");
			Assert.AreEqual(today,result.Value);
		}

		[TestMethod]
		public void RedirectControllerTest_SubpathRelativeMinusValue()
		{
			var appSettings = new AppSettings {Structure = "/yyyyMMdd/{filenamebase}.ext"};
			var controller = new RedirectController(_fakeSelectorStorage, appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var result = controller.SubPathRelative(1, true) as JsonResult;

			var today = "/" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
			Assert.AreEqual(today, result.Value);
		}
		
		[TestMethod]
		public void RedirectControllerTest_LargeInt()
		{
			var appSettings = new AppSettings {Structure = "/yyyyMMdd/{filenamebase}.ext"};
			var controller = new RedirectController(_fakeSelectorStorage, appSettings)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			var result = controller.SubPathRelative(201801020, true) as JsonResult;
			// 201801020= not a date but a large number ==> fallback to today
			var today = "/" + DateTime.Now.ToString("yyyyMMdd");
			Assert.AreEqual(today, result.Value);
		}

		[TestMethod]
		public void RedirectControllerTest_SubpathRelativeRedirectToAction()
		{
			var appSettings = new AppSettings {Structure = "/yyyyMMdd/{filenamebase}.ext"};
			var controller = new RedirectController(_fakeSelectorStorage, appSettings)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			var result = controller.SubPathRelative(0, false) as RedirectToActionResult;

			var today = "/" + DateTime.Now.ToString("yyyyMMdd");

			Assert.AreEqual(today, result.RouteValues.Values.FirstOrDefault());
		}
	}
}

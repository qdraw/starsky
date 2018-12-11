using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace starskytests.Controllers
{
	[TestClass]
	public class RedirectControllerTest
	{
		[TestMethod]
		public void RedirectControllerTest_SubpathRelative()
		{
			var appSettings = new AppSettings();
			appSettings.Structure = "/yyyyMMdd/{filenamebase}.ext";
			var controller = new RedirectController(appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var result = controller.SubpathRelative(0, true) as JsonResult;

			var today = "/" + DateTime.Now.ToString("yyyyMMdd") + "/";
			Assert.AreEqual(today,result.Value);
		}

		[TestMethod]
		public void RedirectControllerTest_SubpathRelativeMinusValue()
		{
			var appSettings = new AppSettings();
			appSettings.Structure = "/yyyyMMdd/{filenamebase}.ext";
			var controller = new RedirectController(appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var result = controller.SubpathRelative(1, true) as JsonResult;

			var today = "/" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "/";
			Assert.AreEqual(today, result.Value);
		}

		[TestMethod]
		public void RedirectControllerTest_SubpathRelativeRedirectToAction()
		{
			var appSettings = new AppSettings();
			appSettings.Structure = "/yyyyMMdd/{filenamebase}.ext";
			var controller = new RedirectController(appSettings);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var result = controller.SubpathRelative(0, false) as RedirectToActionResult;

			var today = "/" + DateTime.Now.ToString("yyyyMMdd") + "/";

			Assert.AreEqual(today, result.RouteValues.Values.FirstOrDefault());
		}

		

	}
}

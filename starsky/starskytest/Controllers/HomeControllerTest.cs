using System;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;

namespace starskytest.Controllers
{
	[TestClass]
	public class HomeControllerTest
	{
		[TestMethod]
		public void HomeController_ReturnFixedCaseSensitiveUrl()
		{
			var controller = new HomeController
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/Search");
			controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?T=1");
			var caseSensitive =  controller.CaseSensitiveRedirect(controller.ControllerContext.HttpContext.Request);
			Assert.AreEqual("/search?T=1",caseSensitive);
		}
		
		[TestMethod]
		public void HomeController_ReturnNotFixedUrl()
		{
			var controller = new HomeController
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/search");
			controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?T=1");
			var caseSensitive =  controller.CaseSensitiveRedirect(controller.ControllerContext.HttpContext.Request);
			Assert.AreEqual(string.Empty,caseSensitive);
		}
	}
}

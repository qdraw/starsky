using System;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class HomeControllerTest
	{
		private readonly IAntiforgery _antiForgery;
		private readonly HttpContext _httpContext;

		public HomeControllerTest()
		{
			_antiForgery = new FakeAntiforgery();
			_httpContext = new DefaultHttpContext();
		}
		
		[TestMethod]
		public void HomeController_Index()
		{
			var controller = new HomeController(_antiForgery)
			{
				ControllerContext = {HttpContext = _httpContext}
			};
			var actionResult = controller.Index() as PhysicalFileResult;
			Assert.AreEqual("text/html", actionResult.ContentType);
		}
		
		[TestMethod]
		public void HomeController_IsCaseSensitiveRedirect_true()
		{
			var controller = new HomeController(_antiForgery);
			var caseSensitive =  controller.IsCaseSensitiveRedirect("/Search","/search");
			Assert.IsTrue(caseSensitive);
		}
		
		[TestMethod]
		public void HomeController_IsCaseSensitiveRedirect_false()
		{
			var controller = new HomeController(_antiForgery);
			var caseSensitive =  controller.IsCaseSensitiveRedirect("/search","/search");
			Assert.IsFalse(caseSensitive);
		}
		
		[TestMethod]
		public void HomeController_SearchPost_Controller_CaseSensitive_Redirect()
		{
			var controller = new HomeController(_antiForgery)
			{
				ControllerContext = {HttpContext = _httpContext}
			};
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/Search");
			controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?T=1");
			var actionResult = controller.SearchPost("1") as RedirectResult;
			Assert.AreEqual("/search?t=1&p=0", actionResult.Url);
		}
		
		[TestMethod]
		public void HomeController_SearchGet_Controller_CaseSensitive_Redirect()
		{
			var controller = new HomeController(_antiForgery)
			{
				ControllerContext = {HttpContext = _httpContext}
			};
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/Search");
			controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?T=1");
			var actionResult = controller.Search("1") as RedirectResult;
			Assert.AreEqual("/search?t=1&p=0", actionResult.Url);
		}

		
		[TestMethod]
		public void HomeController_Trash_Controller_CaseSensitive_Redirect()
		{
			var controller = new HomeController(_antiForgery)
			{
				ControllerContext = {HttpContext = _httpContext}
			};
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/Trash");
			var actionResult = controller.Trash() as RedirectResult;
			Assert.AreEqual("/trash?p=0", actionResult.Url);
		}
		
		[TestMethod]
		public void HomeController_Import_Controller_CaseSensitive_Redirect()
		{
			var controller = new HomeController(_antiForgery)
			{
				ControllerContext = {HttpContext = _httpContext}
			};
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/Import");
			var actionResult = controller.Import() as RedirectResult;
			Assert.AreEqual("/import", actionResult.Url);
		}
		
		[TestMethod]
		public void AccountController_RegisterGet()
		{
			var controller = new HomeController(_antiForgery)
			{
				ControllerContext =
				{
					HttpContext = new DefaultHttpContext()
				}
			};
			controller.Register();
		}
		
		[TestMethod]
		public void Preferences()
		{
			var controller = new HomeController(_antiForgery)
			{
				ControllerContext = {HttpContext = _httpContext}
			};
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/preferences");

			var actionResult = controller.Preferences() as PhysicalFileResult;
			Assert.AreEqual("text/html", actionResult.ContentType);
		}
		
				
		[TestMethod]
		public void Preferences_Expect_Capital()
		{
			var controller = new HomeController(_antiForgery)
			{
				ControllerContext = {HttpContext = _httpContext}
			};
			controller.ControllerContext.HttpContext.Request.Path = new PathString("/Preferences");

			var actionResult = controller.Preferences() as RedirectResult;
			Assert.AreEqual("/preferences", actionResult.Url);
		}

	}
}

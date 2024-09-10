using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class HomeControllerTest
{
	private readonly IAntiforgery _antiForgery;
	private readonly HomeController _controller;
	private readonly HttpContext _httpContext;

	public HomeControllerTest()
	{
		_antiForgery = new FakeAntiforgery();
		_httpContext = new DefaultHttpContext();
		_controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
	}

	[TestMethod]
	public void HomeController_Index()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		var actionResult = controller.Index() as PhysicalFileResult;
		Assert.AreEqual("text/html", actionResult?.ContentType);
	}

	[TestMethod]
	public void HomeController_BadRequest()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = controller.Index();

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void HomeController_IsCaseSensitiveRedirect_true()
	{
		var caseSensitive = HomeController.IsCaseSensitiveRedirect(
			"/Search", "/search");
		Assert.IsTrue(caseSensitive);
	}

	[TestMethod]
	public void HomeController_IsCaseSensitiveRedirect_false()
	{
		var caseSensitive = HomeController.IsCaseSensitiveRedirect(
			"/search", "/search");
		Assert.IsFalse(caseSensitive);
	}

	[TestMethod]
	public void HomeController_SearchPost_Controller_CaseSensitive_Redirect()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ControllerContext.HttpContext.Request.Path = new PathString("/Search");
		controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?T=1");
		var actionResult = controller.SearchPost("1") as RedirectResult;
		Assert.AreEqual("/search?t=1&p=0", actionResult?.Url);
	}

	[TestMethod]
	public void HomeController_SearchGet_Controller_CaseSensitive_Redirect()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ControllerContext.HttpContext.Request.Path = new PathString("/Search");
		controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?T=1");
		var actionResult = controller.Search("1") as RedirectResult;
		Assert.AreEqual("/search?t=1&p=0", actionResult?.Url);
	}


	[TestMethod]
	public void HomeController_Trash_Controller_CaseSensitive_Redirect()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ControllerContext.HttpContext.Request.Path = new PathString("/Trash");
		var actionResult = controller.Trash() as RedirectResult;
		Assert.AreEqual("/trash?p=0", actionResult?.Url);
	}

	[TestMethod]
	public void HomeController_Import_Controller_CaseSensitive_Redirect()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ControllerContext.HttpContext.Request.Path = new PathString("/Import");
		var actionResult = controller.Import() as RedirectResult;
		Assert.AreEqual("/import", actionResult?.Url);
	}

	[TestMethod]
	public void AccountController_RegisterGet()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		var result = controller.Register();
		Assert.IsNotNull(result);
	}
	
	[TestMethod]
	public void Register_BadRequest()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = controller.Register();

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void Preferences()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ControllerContext.HttpContext.Request.Path = new PathString("/preferences");

		var actionResult = controller.Preferences() as PhysicalFileResult;
		Assert.AreEqual("text/html", actionResult?.ContentType);
	}


	[TestMethod]
	public void Preferences_Expect_Capital()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ControllerContext.HttpContext.Request.Path = new PathString("/Preferences");

		var actionResult = controller.Preferences() as RedirectResult;
		Assert.AreEqual("/preferences", actionResult?.Url);
	}

	[TestMethod]
	public void SearchPost_ReturnsRedirectToSearch_WhenTIsEmpty()
	{
		// Arrange
		var t = "";
		var p = 0;

		// Act
		var result = _controller.SearchPost(t, p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(RedirectResult));
		var redirectResult = ( RedirectResult ) result;
		Assert.AreEqual("/search", redirectResult.Url);
	}

	[TestMethod]
	public void SearchPost_ReturnsBadRequest_WhenTContainsInvalidCharacters()
	{
		// Arrange
		var t = "abc$";
		var p = 0;

		// Act
		var result = _controller.SearchPost(t, p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public void SearchPost_ReturnsRedirectToSearch_WhenTIsValid()
	{
		// Arrange
		var t = "abc";
		var p = 0;

		// Act
		var result = _controller.SearchPost(t, p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(RedirectResult));
		var redirectResult = ( RedirectResult ) result;
		Assert.AreEqual($"/search?t={t}&p={p}", redirectResult.Url);
	}

	[TestMethod]
	public void SearchPost_BadRequest()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = controller.SearchPost();

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void Search_ReturnsPhysicalFile_WhenRequestPathIsNotCaseSensitiveRedirect()
	{
		// Arrange
		var t = "";
		var p = 0;
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Path = "/search";
		httpContext.Request.Method = "GET";
		_controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

		// Act
		var result = _controller.Search(t, p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(PhysicalFileResult));
		var physicalFileResult = ( PhysicalFileResult ) result;
		Assert.AreEqual("text/html", physicalFileResult.ContentType);
	}

	[TestMethod]
	public void Search_ReturnsRedirectToSearch_WhenTIsEmpty()
	{
		// Arrange
		var t = "";
		var p = 0;

		// Act
		var result = _controller.Search(t, p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(RedirectResult));
		var redirectResult = ( RedirectResult ) result;
		Assert.AreEqual("/search", redirectResult.Url);
	}

	[TestMethod]
	public void Search_ReturnsBadRequest_WhenTContainsInvalidCharacters()
	{
		// Arrange
		var t = "abc$";
		var p = 0;

		// Act
		var result = _controller.Search(t, p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public void Search_ReturnsRedirectToSearch_WhenTIsValid()
	{
		// Arrange
		var t = "abc";
		var p = 0;

		// Act
		var result = _controller.Search(t, p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(RedirectResult));
		var redirectResult = ( RedirectResult ) result;
		Assert.AreEqual($"/search?t={t}&p={p}", redirectResult.Url);
	}
	
	[TestMethod]
	public void Search_BadRequest()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = controller.Search();

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void Trash_ReturnsPhysicalFile()
	{
		// Arrange
		var p = 0;

		// Act
		_controller.Request.Path = "/trash";
		var result = _controller.Trash(p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(PhysicalFileResult));
		var physicalFileResult = ( PhysicalFileResult ) result;
		Assert.AreEqual("text/html", physicalFileResult.ContentType);
	}

	[TestMethod]
	public void Trash_RedirectsToLowerCaseUrl_WhenRequestedUrlIsCaseSensitive()
	{
		// Arrange
		var p = 0;
		var request = new DefaultHttpContext().Request;
		request.Scheme = "http";
		request.Host = new HostString("localhost");
		request.Path = "/Trash";
		request.PathBase = "/";

		_controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};

		// Act
		var result = _controller.Trash(p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(RedirectResult));
		var redirectResult = ( RedirectResult ) result;
		Assert.AreEqual("/trash?p=0", redirectResult.Url);
	}

	[TestMethod]
	public void Trash_ReturnsPhysicalFile_WhenRequestedUrlIsNotCaseSensitive()
	{
		// Arrange
		var p = 0;
		// difference is trash vs import
		_controller.Request.Path = "/trash";

		// Act
		var result = _controller.Trash(p);

		// Assert
		Assert.IsInstanceOfType(result, typeof(PhysicalFileResult));
		var physicalFileResult = ( PhysicalFileResult ) result;
		Assert.AreEqual("text/html", physicalFileResult.ContentType);
	}
	
	[TestMethod]
	public void Trash_BadRequest()
	{
		var controller = new HomeController(new AppSettings(), _antiForgery)
		{
			ControllerContext = { HttpContext = _httpContext }
		};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = controller.Trash();

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void Import_ReturnsPhysicalFile()
	{
		// Act
		_controller.Request.Path = "/import";
		var result = _controller.Import();

		// Assert
		Assert.IsInstanceOfType(result, typeof(PhysicalFileResult));
		var physicalFileResult = ( PhysicalFileResult ) result;
		Assert.AreEqual("text/html", physicalFileResult.ContentType);
	}

	[TestMethod]
	public void Import_ReturnsPhysicalFile_WhenRequestedUrlIsNotCaseSensitive()
	{
		// Arrange
		_controller.Request.Path = "/import";

		// Act
		var result = _controller.Import();

		// Assert
		Assert.IsInstanceOfType(result, typeof(PhysicalFileResult));
		var physicalFileResult = ( PhysicalFileResult ) result;
		Assert.AreEqual("text/html", physicalFileResult.ContentType);
	}

	[TestMethod]
	public void AppendPathBasePrefix_1()
	{
		var result = HomeController.AppendPathBasePrefix("", "/search");
		Assert.AreEqual("/search", result);
	}

	[TestMethod]
	public void AppendPathBasePrefix_2()
	{
		var result = HomeController.AppendPathBasePrefix("test", "/search");
		Assert.AreEqual("/search", result);
	}

	[TestMethod]
	public void AppendPathBasePrefix_3()
	{
		var result = HomeController.AppendPathBasePrefix("/starsky", "/search");
		Assert.AreEqual("/starsky/search", result);
	}
}

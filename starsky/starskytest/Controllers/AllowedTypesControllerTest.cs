using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;

namespace starskytest.Controllers;

[TestClass]
public sealed class AllowedTypesControllerTest
{
	private readonly HttpContext _httpContext = new DefaultHttpContext();

	[TestMethod]
	public void AllowedTypesController_MimetypeSync()
	{
		var jsonResult = new AllowedTypesController().AllowedTypesMimetypeSync() as JsonResult;
		var allowedApiResult = jsonResult?.Value as HashSet<string>;
		Assert.IsTrue(allowedApiResult?.Contains("image/jpeg"));
	}

	[TestMethod]
	public void AllowedTypesController_MimetypeSyncThumb()
	{
		var jsonResult = new AllowedTypesController().AllowedTypesMimetypeSyncThumb() as JsonResult;
		var allowedApiResult = jsonResult?.Value as HashSet<string>;
		Assert.IsTrue(allowedApiResult?.Contains("image/jpeg"));
	}

	/// <summary>
	///     The list is built from the same extension list on every request, calling the endpoint
	///     more than once should return the same result
	/// </summary>
	[TestMethod]
	public void AllowedTypesController_MimetypeSyncThumb_CalledTwice()
	{
		var controller = new AllowedTypesController();

		var first = controller.AllowedTypesMimetypeSyncThumb() as JsonResult;
		var second = controller.AllowedTypesMimetypeSyncThumb() as JsonResult;

		var firstList = first?.Value as HashSet<string>;
		var secondList = second?.Value as HashSet<string>;

		Assert.IsNotNull(firstList);
		Assert.IsNotNull(secondList);
		Assert.Contains("image/webp", firstList);
		Assert.AreSequenceEqual(firstList.ToList(), secondList.ToList());
	}

	[TestMethod]
	public void AllowedTypesController_AllowedTypesThumb_NoInput()
	{
		var jsonResult =
			new AllowedTypesController { ControllerContext = { HttpContext = _httpContext } }
				.AllowedTypesThumb("") as JsonResult;
		var allowedApiResult = bool.Parse(jsonResult?.Value?.ToString()!);
		Assert.IsFalse(allowedApiResult);
	}

	[TestMethod]
	public void AllowedTypesController_AllowedTypesThumb_Example()
	{
		var jsonResult =
			new AllowedTypesController { ControllerContext = { HttpContext = _httpContext } }
				.AllowedTypesThumb("test.jpg") as JsonResult;
		var allowedApiResult = bool.Parse(jsonResult?.Value?.ToString()!);
		Assert.IsTrue(allowedApiResult);
	}

	[TestMethod]
	public void AllowedTypesController_AllowedTypesThumb_ReturnsBadRequest()
	{
		// Arrange
		var controller =
			new AllowedTypesController { ControllerContext = { HttpContext = _httpContext } };

		controller.ModelState.AddModelError("Key", "ErrorMessage");

		// Act
		var result = controller.AllowedTypesThumb("test.jpg");

		// Assert
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}
}

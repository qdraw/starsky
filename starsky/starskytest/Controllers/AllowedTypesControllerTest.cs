using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;

namespace starskytest.Controllers
{
	[TestClass]
	public class AllowedTypesControllerTest
	{
		private readonly HttpContext _httpContext = new DefaultHttpContext();

		[TestMethod]
		public void AllowedTypesController_MimetypeSync()
		{
			var jsonResult = new AllowedTypesController().AllowedTypesMimetypeSync() as JsonResult;
			var allowedApiResult = jsonResult.Value as HashSet<string>;
			Assert.IsTrue(allowedApiResult.Contains("image/jpeg"));
		}
		
		[TestMethod]
		public void AllowedTypesController_MimetypeSyncThumb()
		{
			var jsonResult = new AllowedTypesController().AllowedTypesMimetypeSyncThumb() as JsonResult;
			var allowedApiResult = jsonResult.Value as HashSet<string>;
			Assert.IsTrue(allowedApiResult.Contains("image/jpeg"));
		}
		
		[TestMethod]
		public void AllowedTypesController_AllowedTypesThumb_NoInput()
		{
			var jsonResult = new AllowedTypesController{ ControllerContext = {HttpContext = _httpContext}}.AllowedTypesThumb("") as JsonResult;
			var allowedApiResult = bool.Parse(jsonResult.Value.ToString());
			Assert.IsFalse(allowedApiResult);
		}
		
		[TestMethod]
		public void AllowedTypesController_AllowedTypesThumb_Example()
		{
			var jsonResult = new AllowedTypesController{ ControllerContext = {HttpContext = _httpContext}}.AllowedTypesThumb("test.jpg") as JsonResult;
			var allowedApiResult = bool.Parse(jsonResult.Value.ToString());
			Assert.IsTrue(allowedApiResult);
		}
	}
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public class ErrorControllerTest
	{
		
		[TestMethod]
		public void ErrorControllerTest_Error()
		{
			var controller = new ErrorController(new AppSettings())
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = controller.Error(404) as PhysicalFileResult;
			Assert.AreEqual("text/html",actionResult.ContentType);
		}
	}
}

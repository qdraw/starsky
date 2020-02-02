using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;

namespace starskytest.Controllers
{
	[TestClass]
	public class ErrorControllerTest
	{
		
		[TestMethod]
		public void ErrorControllerTest_Error()
		{
			var controller = new ErrorController
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = controller.Error(404) as PhysicalFileResult;
			Assert.AreEqual("text/html",actionResult.ContentType);
		}
	}
}

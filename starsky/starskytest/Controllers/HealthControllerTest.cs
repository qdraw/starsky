using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskycore.ViewModels;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class HealthControllerTest
	{
		
		[TestMethod]
		public void HealthControllerTest_1()
		{
			var fakeHealthCheckService = new FakeHealthCheckService();
			var controller = new HealthController(fakeHealthCheckService)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = controller.Index() as JsonResult;
			var castedResult = actionResult.Value as HealthView;

			Assert.IsTrue(castedResult.IsHealthy);
			Assert.IsTrue(castedResult.Entries.FirstOrDefault().IsHealthy);
		}
	}
}

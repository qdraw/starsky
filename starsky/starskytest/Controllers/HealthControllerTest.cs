using System.Linq;
using System.Threading.Tasks;
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
		public async Task HealthControllerTest_Details()
		{
			var fakeHealthCheckService = new FakeHealthCheckService(true);
			var controller = new HealthController(fakeHealthCheckService)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = await controller.Details() as JsonResult;
			var castedResult = actionResult.Value as HealthView;

			Assert.IsTrue(castedResult.IsHealthy);
			Assert.IsTrue(castedResult.Entries.FirstOrDefault().IsHealthy);
		}
		
		[TestMethod]
		public async Task HealthControllerTest_Index()
		{
			var fakeHealthCheckService = new FakeHealthCheckService(true);
			var controller = new HealthController(fakeHealthCheckService)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = await controller.Index() as ContentResult;
			
			Assert.AreEqual("Healthy",actionResult.Content);
		}
	}
}

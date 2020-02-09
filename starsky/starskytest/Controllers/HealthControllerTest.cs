using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskycore.Helpers;
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
			Assert.AreEqual("test", castedResult.Entries.FirstOrDefault().Name );
			Assert.IsTrue(castedResult.Entries.FirstOrDefault().Duration == TimeSpan.Zero);
			Assert.IsTrue(castedResult.Entries.Any());
			Assert.IsTrue(castedResult.TotalDuration == TimeSpan.Zero);
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
		
		[TestMethod]
		public void HealthControllerTest_ApplicationInsights()
		{
			var controller = new HealthController(null, new ApplicationInsightsJsHelper(null,null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = controller.ApplicationInsights() as ContentResult;
			Assert.AreEqual("/* ApplicationInsights JavaScriptSnippet disabled */",actionResult.Content);
		}
	}
}

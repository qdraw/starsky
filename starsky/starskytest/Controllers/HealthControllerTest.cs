using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.webtelemetry.Services;
using starskycore.ViewModels;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public sealed class HealthControllerTest
	{
		
		[TestMethod]
		public async Task HealthControllerTest_Details_True()
		{
			var fakeHealthCheckService = new FakeHealthCheckService(true);
			var controller = new HealthController(fakeHealthCheckService, new FakeTelemetryService())
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = await controller.Details() as JsonResult;
			var castedResult = actionResult?.Value as HealthView;

			Assert.IsTrue(castedResult?.IsHealthy);
			Assert.IsTrue(castedResult?.Entries.FirstOrDefault()?.IsHealthy);
			Assert.AreEqual("test", castedResult?.Entries.FirstOrDefault()?.Name );
			Assert.IsTrue(castedResult?.Entries.FirstOrDefault()?.Duration == TimeSpan.Zero);
			Assert.IsTrue(castedResult.Entries.Any());
			Assert.IsTrue(castedResult.TotalDuration == TimeSpan.Zero);
		}
		
		[TestMethod]
		public async Task HealthControllerTest_Details_False()
		{
			var fakeHealthCheckService = new FakeHealthCheckService(false);
			var controller = new HealthController(fakeHealthCheckService, new FakeTelemetryService())
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = await controller.Details() as JsonResult;
			var castedResult = actionResult?.Value as HealthView;

			Assert.IsFalse(castedResult?.IsHealthy);
			Assert.IsFalse(castedResult?.Entries.FirstOrDefault()?.IsHealthy);
			Assert.AreEqual("test", castedResult?.Entries?.FirstOrDefault()?.Name );
			Assert.IsTrue(castedResult?.Entries?.FirstOrDefault()?.Duration == TimeSpan.Zero);
			Assert.IsTrue(castedResult.Entries.Count != 0);
			Assert.IsTrue(castedResult.TotalDuration == TimeSpan.Zero);
		}
		
		[TestMethod]
		public async Task HealthControllerTest_Index_True()
		{
			var fakeHealthCheckService = new FakeHealthCheckService(true);
			var controller = new HealthController(fakeHealthCheckService,new FakeTelemetryService())
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = await controller.Index() as ContentResult;
			
			Assert.AreEqual("Healthy",actionResult?.Content);
		}
		
				
		[TestMethod]
		public async Task HealthControllerTest_Index_False()
		{
			var fakeHealthCheckService = new FakeHealthCheckService(false);
			var controller = new HealthController(fakeHealthCheckService,new FakeTelemetryService())
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = await controller.Index() as ContentResult;

			Assert.AreEqual("Unhealthy",actionResult?.Content);
		}
		
		[TestMethod]
		public void HealthControllerTest_ApplicationInsights()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			var actionResult = controller.ApplicationInsights() as ContentResult;
			Assert.AreEqual("/* ApplicationInsights JavaScriptSnippet disabled */",
				actionResult?.Content);
		}

		[TestMethod]
		public void Version_NoVersion()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			var noVersion = controller.Version() as BadRequestObjectResult;
			Assert.AreEqual(400, noVersion?.StatusCode);
		}
		
		[TestMethod]
		public void Version_Version_newer()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = "1.0";
			var noVersion = controller.Version() as OkObjectResult;
			Assert.AreEqual(200, noVersion?.StatusCode);
		}

		[TestMethod]
		public void Version_Version_older()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = "0.1";
			var noVersion = controller.Version() as ObjectResult;
			Assert.AreEqual(202, noVersion?.StatusCode);
		}
		
		[TestMethod]
		public void Version_Version_AsParam_older()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			var noVersion = controller.Version("0.1") as ObjectResult;
			Assert.AreEqual(202, noVersion?.StatusCode);
		}

		[TestMethod]
		public void Version_Version_beta1_isBefore()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			const string beta = HealthController.MinimumVersion + "-beta.1";
			// the beta is before the 0.3 release
			controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = beta;
			var noVersion = controller.Version() as ObjectResult;
			Assert.AreEqual(202, noVersion?.StatusCode);
		}
		
		[TestMethod]
		public void Version_Version_eq()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};

			controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = 
				HealthController.MinimumVersion;
			var noVersion = controller.Version() as OkObjectResult;
			Assert.AreEqual(200, noVersion?.StatusCode);
		}

		[TestMethod]
		public void Version_Version_NonValidInput()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = "0.bad-input";
			var noVersion = controller.Version() as ObjectResult;
			Assert.AreEqual(400, noVersion?.StatusCode);
		}
		

		[TestMethod]
		public void Version_Version_0()
		{
			var controller = new HealthController(null, new FakeTelemetryService(), 
				new ApplicationInsightsJsHelper(null))
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = "0";
			var noVersion = controller.Version() as ObjectResult;
			Assert.AreEqual(202, noVersion?.StatusCode);
		}

		[TestMethod]
		public async Task CheckHealthAsyncWithTimeout_ShouldTimeout()
		{
			var result = await new HealthController(
					new FakeHealthCheckService(true), null)
				.CheckHealthAsyncWithTimeout(-1);
			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}
		
		[TestMethod]
		public async Task CheckHealthAsyncWithTimeout_ShouldSucceed()
		{
			var result = await new HealthController(new FakeHealthCheckService(true), 
					null)
				.CheckHealthAsyncWithTimeout();
			Assert.AreEqual(HealthStatus.Healthy, result.Status);
		}

		[TestMethod]
		public async Task CheckHealthAsyncWithTimeout_IgnoreCachedUnHealthyInput()
		{
			var entry = new HealthReportEntry(
				HealthStatus.Unhealthy,
				"timeout",
				TimeSpan.FromMilliseconds(1),
				null,
				null);
			var cachedItem = new Dictionary<string, object> {{"health", new HealthReport(
				new Dictionary<string, HealthReportEntry>{{"timeout",entry}}, 
				TimeSpan.FromMilliseconds(0))}};
			
			var result = await new HealthController(
					new FakeHealthCheckService(true),
					null, null, new FakeMemoryCache(cachedItem))
				.CheckHealthAsyncWithTimeout();
			Assert.AreEqual(HealthStatus.Healthy, result.Status);
		}
		
		[TestMethod]
		public async Task CheckHealthAsyncWithTimeout_IgnoreCheckIfCachedInputIsHealthy()
		{
			var entry = new HealthReportEntry(
				HealthStatus.Healthy,
				"timeout",
				TimeSpan.FromMilliseconds(1),
				null,
				null);
			var cachedItem = new Dictionary<string, object> {{"health", new HealthReport(
				new Dictionary<string, HealthReportEntry>{{"timeout",entry}}, 
				TimeSpan.FromMilliseconds(0))}};
			
			var result = await new HealthController(
					new FakeHealthCheckService(false),
					null, null, new FakeMemoryCache(cachedItem))
				.CheckHealthAsyncWithTimeout();
			Assert.AreEqual(HealthStatus.Healthy, result.Status);
		}
	}
}

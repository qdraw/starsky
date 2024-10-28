using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.project.web.ViewModels;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class HealthControllerTest
{
	[TestMethod]
	public async Task HealthControllerTest_Details_True()
	{
		var fakeHealthCheckService = new FakeHealthCheckService(true);
		var controller =
			new HealthController(
				new FakeICheckHealthService(new FakeIWebLogger(), fakeHealthCheckService))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};

		var actionResult = await controller.Details() as JsonResult;
		var castedResult = actionResult?.Value as HealthView;

		Assert.IsTrue(castedResult?.IsHealthy);
		Assert.IsTrue(castedResult?.Entries.FirstOrDefault()?.IsHealthy);
		Assert.AreEqual("test", castedResult?.Entries.FirstOrDefault()?.Name);
		Assert.IsTrue(castedResult?.Entries.FirstOrDefault()?.Duration == TimeSpan.Zero);
		Assert.IsTrue(castedResult.Entries.Count != 0);
		Assert.IsTrue(castedResult.TotalDuration == TimeSpan.Zero);
	}

	[TestMethod]
	public async Task HealthControllerTest_Details_False()
	{
		var fakeHealthCheckService = new FakeHealthCheckService(false);
		var controller =
			new HealthController(
				new FakeICheckHealthService(new FakeIWebLogger(), fakeHealthCheckService))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};

		var actionResult = await controller.Details() as JsonResult;
		var castedResult = actionResult?.Value as HealthView;

		Assert.IsFalse(castedResult?.IsHealthy);
		Assert.IsFalse(castedResult?.Entries.FirstOrDefault()?.IsHealthy);
		Assert.AreEqual("test", castedResult?.Entries?.FirstOrDefault()?.Name);
		Assert.IsTrue(castedResult?.Entries?.FirstOrDefault()?.Duration == TimeSpan.Zero);
		Assert.IsTrue(castedResult.Entries.Count != 0);
		Assert.IsTrue(castedResult.TotalDuration == TimeSpan.Zero);
	}

	[TestMethod]
	public async Task HealthControllerTest_Details_False_Logging()
	{
		var fakeHealthCheckService = new FakeHealthCheckService(false);
		var logger = new FakeIWebLogger();
		var controller =
			new HealthController(new FakeICheckHealthService(logger, fakeHealthCheckService))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};

		await controller.Details();

		Assert.IsTrue(logger.TrackedExceptions.LastOrDefault().Item2
			?.Contains("HealthCheck test failed"));
	}

	[TestMethod]
	public async Task HealthControllerTest_Index_True()
	{
		var fakeHealthCheckService = new FakeHealthCheckService(true);
		var controller =
			new HealthController(
				new FakeICheckHealthService(new FakeIWebLogger(), fakeHealthCheckService))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};

		var actionResult = await controller.Index() as ContentResult;

		Assert.AreEqual("Healthy", actionResult?.Content);
	}


	[TestMethod]
	public async Task HealthControllerTest_Index_False()
	{
		var fakeHealthCheckService = new FakeHealthCheckService(false);
		var controller =
			new HealthController(
				new FakeICheckHealthService(new FakeIWebLogger(), fakeHealthCheckService))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};

		var actionResult = await controller.Index() as ContentResult;

		Assert.AreEqual("Unhealthy", actionResult?.Content);
	}

	[TestMethod]
	public void Version_NoVersion()
	{
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		var noVersion = controller.Version() as BadRequestObjectResult;
		Assert.AreEqual(400, noVersion?.StatusCode);
	}

	[TestMethod]
	public void Version_NoVersion_BadRequest()
	{
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = controller.Version() as BadRequestObjectResult;
		Assert.AreEqual(400, result?.StatusCode);
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public void Version_Version_newer()
	{
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = "1.0";
		var noVersion = controller.Version() as OkObjectResult;
		Assert.AreEqual(200, noVersion?.StatusCode);
	}

	[TestMethod]
	public void Version_Version_older()
	{
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = "0.1";
		var noVersion = controller.Version() as ObjectResult;
		Assert.AreEqual(202, noVersion?.StatusCode);
	}

	[TestMethod]
	public void Version_Version_AsParam_older()
	{
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		var noVersion = controller.Version("0.1") as ObjectResult;
		Assert.AreEqual(202, noVersion?.StatusCode);
	}

	[TestMethod]
	public void Version_Version_beta1_isBefore()
	{
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
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
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};

		controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] =
			HealthController.MinimumVersion;
		var noVersion = controller.Version() as OkObjectResult;
		Assert.AreEqual(200, noVersion?.StatusCode);
	}

	[TestMethod]
	public void Version_Version_NonValidInput()
	{
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] =
			"0.bad-input";
		var noVersion = controller.Version() as ObjectResult;
		Assert.AreEqual(400, noVersion?.StatusCode);
	}

	[TestMethod]
	public void Version_Version_0()
	{
		var controller =
			new HealthController(new FakeICheckHealthService(new FakeIWebLogger(), null))
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		controller.ControllerContext.HttpContext.Request.Headers["x-api-version"] = "0";
		var noVersion = controller.Version() as ObjectResult;
		Assert.AreEqual(202, noVersion?.StatusCode);
	}
}

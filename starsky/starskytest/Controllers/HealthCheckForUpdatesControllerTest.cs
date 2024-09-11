using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.health.UpdateCheck.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class HealthCheckForUpdatesControllerTest
{
	// or CheckForUpdatesTest

	// Disabled,
	// HttpError,
	// NoReleasesFound,
	// NeedToUpdate,
	// CurrentVersionIsLatest

	[TestMethod]
	public async Task CheckForUpdates_Disabled()
	{
		var fakeService = new FakeICheckForUpdates(
			new KeyValuePair<UpdateStatus, string?>(UpdateStatus.Disabled, string.Empty));

		var actionResult =
			await new HealthCheckForUpdatesController(fakeService,
				new FakeISpecificVersionReleaseInfo()).CheckForUpdates() as ObjectResult;
		Assert.AreEqual(208, actionResult?.StatusCode);
	}


	[TestMethod]
	public async Task CheckForUpdates_NotSupportedException()
	{
		var input = new TestOverWriteEnumModel { Value = UpdateStatus.Disabled };

		// Use reflection to set the updateStatus field to UpdateAvailable
		// overwrite enum value
		var propertyInfo = input.GetType().GetProperty("Value");
		Assert.IsNotNull(propertyInfo);
		propertyInfo.SetValue(input, 44, null); // <-- this could not happen

		var fakeService = new FakeICheckForUpdates(
			new KeyValuePair<UpdateStatus, string?>(input.Value, string.Empty));

		Assert.IsNotNull(input.Value);

		var sut = new HealthCheckForUpdatesController(fakeService,
			new FakeISpecificVersionReleaseInfo());
		await Assert.ThrowsExceptionAsync<NotSupportedException>(async () =>
			await sut.CheckForUpdates());
	}

	[TestMethod]
	public async Task CheckForUpdates_HttpError()
	{
		var fakeService = new FakeICheckForUpdates(
			new KeyValuePair<UpdateStatus, string?>(UpdateStatus.HttpError, string.Empty));

		var actionResult =
			await new HealthCheckForUpdatesController(fakeService,
				new FakeISpecificVersionReleaseInfo()).CheckForUpdates() as ObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task CheckForUpdates_InputNotValid()
	{
		var fakeService = new FakeICheckForUpdates(
			new KeyValuePair<UpdateStatus, string?>(UpdateStatus.InputNotValid, string.Empty));

		var service2 = new FakeISpecificVersionReleaseInfo();
		var actionResult =
			await new HealthCheckForUpdatesController(fakeService, service2)
					.CheckForUpdates() as
				ObjectResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task CheckForUpdates_NoReleasesFound()
	{
		var fakeService = new FakeICheckForUpdates(
			new KeyValuePair<UpdateStatus, string?>(UpdateStatus.NoReleasesFound,
				string.Empty));

		var service2 = new FakeISpecificVersionReleaseInfo();

		var actionResult =
			await new HealthCheckForUpdatesController(fakeService, service2)
					.CheckForUpdates() as
				ObjectResult;
		Assert.AreEqual(206, actionResult?.StatusCode);
	}

	// NeedToUpdate
	[TestMethod]
	public async Task CheckForUpdates_NeedToUpdate()
	{
		var service2 = new FakeISpecificVersionReleaseInfo();
		var fakeService = new FakeICheckForUpdates(
			new KeyValuePair<UpdateStatus, string?>(UpdateStatus.NeedToUpdate, string.Empty));

		var actionResult =
			await new HealthCheckForUpdatesController(fakeService, service2)
					.CheckForUpdates() as
				ObjectResult;
		Assert.AreEqual(202, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task CheckForUpdates_CurrentVersionIsLatest()
	{
		var fakeService = new FakeICheckForUpdates(
			new KeyValuePair<UpdateStatus, string?>(UpdateStatus.CurrentVersionIsLatest,
				string.Empty));
		var service2 = new FakeISpecificVersionReleaseInfo();

		var actionResult =
			await new HealthCheckForUpdatesController(fakeService, service2)
					.CheckForUpdates() as
				ObjectResult;
		Assert.AreEqual(200, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task CheckForUpdates_BadRequest()
	{
		var fakeService = new FakeICheckForUpdates(
			new KeyValuePair<UpdateStatus, string?>(UpdateStatus.CurrentVersionIsLatest,
				string.Empty));
		var service2 = new FakeISpecificVersionReleaseInfo();

		var controller =
			new HealthCheckForUpdatesController(fakeService, service2);
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		// Act
		var result = await controller.CheckForUpdates();

		// Assert
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task SpecificVersionReleaseInfo_GivesResult()
	{
		var fakeService = new FakeICheckForUpdates(new KeyValuePair<UpdateStatus, string?>());
		var service2 = new FakeISpecificVersionReleaseInfo(
			new Dictionary<string, Dictionary<string, string>>
			{
				{
					"1.0.0",
					new Dictionary<string, string>
					{
						{ "en", "	# 1.0.0\n\n- [x] test\n- [ ] test2\n\n" }
					}
				}
			}
		);

		var controller = new HealthCheckForUpdatesController(fakeService,
			service2) { ControllerContext = { HttpContext = new DefaultHttpContext() } };
		var actionResult = await controller
			.SpecificVersionReleaseInfo("1.0.0") as JsonResult;

		Assert.AreEqual("	# 1.0.0\n\n- [x] test\n- [ ] test2\n\n",
			actionResult?.Value);
	}

	[TestMethod]
	public async Task SpecificVersionReleaseInfo_NoContent()
	{
		var fakeService = new FakeICheckForUpdates(new KeyValuePair<UpdateStatus, string?>());
		var service2 = new FakeISpecificVersionReleaseInfo(
			new Dictionary<string, Dictionary<string, string>>()
		);

		var controller = new HealthCheckForUpdatesController(fakeService,
			service2) { ControllerContext = { HttpContext = new DefaultHttpContext() } };
		var actionResult = await controller
			.SpecificVersionReleaseInfo() as JsonResult;

		Assert.AreEqual(string.Empty,
			actionResult?.Value);
	}

	[TestMethod]
	public async Task SpecificVersionReleaseInfo_BadRequest()
	{
		var fakeService = new FakeICheckForUpdates(new KeyValuePair<UpdateStatus, string?>());
		var service2 = new FakeISpecificVersionReleaseInfo(
			new Dictionary<string, Dictionary<string, string>>()
		);

		var controller = new HealthCheckForUpdatesController(fakeService,
			service2) { ControllerContext = { HttpContext = new DefaultHttpContext() } };
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var result = await controller
			.SpecificVersionReleaseInfo(null!);

		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	private class TestOverWriteEnumModel
	{
		public UpdateStatus Value { get; set; }
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.health.UpdateCheck.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
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
				new KeyValuePair<UpdateStatus, string>(UpdateStatus.Disabled, string.Empty));

			var actionResult = await new HealthCheckForUpdatesController(fakeService).CheckForUpdates() as ObjectResult;
			Assert.AreEqual(208,actionResult?.StatusCode);
		}
		
		[TestMethod]
		public async Task CheckForUpdates_HttpError()
		{
			var fakeService = new FakeICheckForUpdates(
				new KeyValuePair<UpdateStatus, string>(UpdateStatus.HttpError, string.Empty));

			var actionResult = await new HealthCheckForUpdatesController(fakeService).CheckForUpdates() as ObjectResult;
			Assert.AreEqual(400,actionResult?.StatusCode);
		}
		
		[TestMethod]
		public async Task CheckForUpdates_InputNotValid()
		{
			var fakeService = new FakeICheckForUpdates(
				new KeyValuePair<UpdateStatus, string>(UpdateStatus.InputNotValid, string.Empty));

			var actionResult = await new HealthCheckForUpdatesController(fakeService).CheckForUpdates() as ObjectResult;
			Assert.AreEqual(400,actionResult?.StatusCode);
		}
		
		[TestMethod]
		public async Task CheckForUpdates_NoReleasesFound()
		{
			var fakeService = new FakeICheckForUpdates(
				new KeyValuePair<UpdateStatus, string>(UpdateStatus.NoReleasesFound, string.Empty));

			var actionResult = await new HealthCheckForUpdatesController(fakeService).CheckForUpdates() as ObjectResult;
			Assert.AreEqual(206,actionResult?.StatusCode);
		}
		
		// NeedToUpdate
		[TestMethod]
		public async Task CheckForUpdates_NeedToUpdate()
		{
			var fakeService = new FakeICheckForUpdates(
				new KeyValuePair<UpdateStatus, string>(UpdateStatus.NeedToUpdate, string.Empty));

			var actionResult = await new HealthCheckForUpdatesController(fakeService).CheckForUpdates() as ObjectResult;
			Assert.AreEqual(202,actionResult?.StatusCode);
		}
		
		[TestMethod]
		public async Task CheckForUpdates_CurrentVersionIsLatest()
		{
			var fakeService = new FakeICheckForUpdates(
				new KeyValuePair<UpdateStatus, string>(UpdateStatus.CurrentVersionIsLatest, string.Empty));

			var actionResult = await new HealthCheckForUpdatesController(fakeService).CheckForUpdates() as ObjectResult;
			Assert.AreEqual(200,actionResult?.StatusCode);
		}

	}
}

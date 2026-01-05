using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.cloudsync;
using starsky.foundation.cloudsync.Controllers;
using starsky.foundation.cloudsync.Interfaces;
using starsky.foundation.platform.Models;
using System;
using System.Threading.Tasks;

namespace starskytest.starsky.foundation.cloudsync.Controllers;

[TestClass]
public class CloudSyncControllerTest
{
	private class FakeCloudSyncService : ICloudSyncService
	{
		public bool IsSyncInProgress { get; set; }
		public CloudSyncResult? LastSyncResult { get; set; }
		public Func<CloudSyncTriggerType, Task<CloudSyncResult>>? SyncAsyncFunc { get; set; }

		public Task<CloudSyncResult> SyncAsync(CloudSyncTriggerType triggerType)
		{
			if (SyncAsyncFunc != null)
			{
				return SyncAsyncFunc(triggerType);
			}

			return Task.FromResult(new CloudSyncResult
			{
				StartTime = DateTime.UtcNow,
				EndTime = DateTime.UtcNow,
				TriggerType = triggerType,
				FilesFound = 5,
				FilesImportedSuccessfully = 5
			});
		}
	}

	[TestMethod]
	public void GetStatus_ShouldReturnCurrentStatus()
	{
		// Arrange
		var settings = new CloudSyncSettings
		{
			Enabled = true,
			Provider = "Dropbox",
			RemoteFolder = "/photos",
			SyncFrequencyMinutes = 30,
			DeleteAfterImport = true
		};
		var service = new FakeCloudSyncService();
		var controller = new CloudSyncController(service, settings);

		// Act
		var result = controller.GetStatus() as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
	}

	[TestMethod]
	public async Task TriggerSync_WhenDisabled_ShouldReturnBadRequest()
	{
		// Arrange
		var settings = new CloudSyncSettings { Enabled = false };
		var service = new FakeCloudSyncService();
		var controller = new CloudSyncController(service, settings);

		// Act
		var result = await controller.TriggerSync() as BadRequestObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
	}

	[TestMethod]
	public async Task TriggerSync_WhenAlreadyInProgress_ShouldReturnConflict()
	{
		// Arrange
		var settings = new CloudSyncSettings { Enabled = true };
		var service = new FakeCloudSyncService { IsSyncInProgress = true };
		var controller = new CloudSyncController(service, settings);

		// Act
		var result = await controller.TriggerSync() as ConflictObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(409, result.StatusCode);
	}

	[TestMethod]
	public async Task TriggerSync_WhenEnabled_ShouldStartSyncAndReturnResult()
	{
		// Arrange
		var settings = new CloudSyncSettings { Enabled = true };
		var service = new FakeCloudSyncService();
		var controller = new CloudSyncController(service, settings);

		// Act
		var result = await controller.TriggerSync() as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
		Assert.IsInstanceOfType(result.Value, typeof(CloudSyncResult));
		var syncResult = result.Value as CloudSyncResult;
		Assert.AreEqual(CloudSyncTriggerType.Manual, syncResult!.TriggerType);
	}

	[TestMethod]
	public void GetLastResult_WhenNoSyncPerformed_ShouldReturnNotFound()
	{
		// Arrange
		var settings = new CloudSyncSettings { Enabled = true };
		var service = new FakeCloudSyncService { LastSyncResult = null };
		var controller = new CloudSyncController(service, settings);

		// Act
		var result = controller.GetLastResult() as NotFoundObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(404, result.StatusCode);
	}

	[TestMethod]
	public void GetLastResult_WhenSyncPerformed_ShouldReturnResult()
	{
		// Arrange
		var settings = new CloudSyncSettings { Enabled = true };
		var lastResult = new CloudSyncResult
		{
			StartTime = DateTime.UtcNow.AddMinutes(-5),
			EndTime = DateTime.UtcNow,
			TriggerType = CloudSyncTriggerType.Scheduled,
			FilesFound = 10,
			FilesImportedSuccessfully = 8,
			FilesFailed = 2
		};
		var service = new FakeCloudSyncService { LastSyncResult = lastResult };
		var controller = new CloudSyncController(service, settings);

		// Act
		var result = controller.GetLastResult() as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
		Assert.IsInstanceOfType(result.Value, typeof(CloudSyncResult));
		var syncResult = result.Value as CloudSyncResult;
		Assert.AreEqual(10, syncResult!.FilesFound);
		Assert.AreEqual(8, syncResult.FilesImportedSuccessfully);
		Assert.AreEqual(2, syncResult.FilesFailed);
	}
}

